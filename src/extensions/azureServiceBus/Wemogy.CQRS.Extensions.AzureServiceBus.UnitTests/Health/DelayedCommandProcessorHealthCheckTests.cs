using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Wemogy.Configuration;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.Health;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Health;

[Collection("AzureServiceBus")]
public class DelayedCommandProcessorHealthCheckTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private IDelayedCommandProcessorHostedService<PrintContextCommand>? _startedHostedService;

    public DelayedCommandProcessorHealthCheckTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var serviceCollection = new ServiceCollection();
        _serviceBusClient = new ServiceBusClient(configuration["AzureServiceBusConnectionString"] !);

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBusWithClient(_serviceBusClient)

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintContextCommand>(builder =>
            {
                builder.WithQueueName("unit-testing-queue-1");
            })
            .AddDelayedProcessor<PrintContextCommand>();

        serviceCollection.AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_startedHostedService is { IsAlive: true })
        {
            await _startedHostedService.StopAsync(CancellationToken.None);
        }

        await _serviceBusClient.DisposeAsync();
    }

    [Fact]
    public async Task DelayedCommandProcessorHealthCheck_ShouldReturnUnhealthyIfHostedServiceOfCommandWasNotFound()
    {
        // Arrange
        var healthCheckService = new DelayedCommandProcessorHealthCheck<PrintContextCommand>(ArraySegment<IHostedService>.Empty);

        // Act
        var healthCheckResult = await healthCheckService.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        // Assert
        healthCheckResult.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task DelayedCommandProcessorHealthCheck_ShouldReturnHealthyIfAzureServiceBusCommandProcessorIsAlive()
    {
        // Arrange
        var hostedService = await StartHostedServiceAsync();
        var healthCheckService = new DelayedCommandProcessorHealthCheck<PrintContextCommand>(new[] { hostedService });

        // Act
        var healthCheckResult = await healthCheckService.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        // Assert
        healthCheckResult.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task DelayedCommandProcessorHealthCheck_ShouldReturnUnhealthyIfAzureServiceBusCommandProcessorIsNotAlive()
    {
        // Arrange
        var hostedService = await StartHostedServiceAsync();
        await hostedService.StopAsync(CancellationToken.None);
        var healthCheckService = new DelayedCommandProcessorHealthCheck<PrintContextCommand>(new[] { hostedService });

        // Act
        var healthCheckResult = await healthCheckService.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        // Assert
        healthCheckResult.Status.Should().Be(HealthStatus.Unhealthy);
    }

    private async Task<IDelayedCommandProcessorHostedService<PrintContextCommand>> StartHostedServiceAsync()
    {
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IDelayedCommandProcessorHostedService<PrintContextCommand>>()
            .First();
        await hostedService.StartAsync(CancellationToken.None);
        _startedHostedService = hostedService;

        // wait a bit for the hosted service to start and may process deprecated messages
        await Task.Delay(TimeSpan.FromSeconds(5));

        return hostedService;
    }
}
