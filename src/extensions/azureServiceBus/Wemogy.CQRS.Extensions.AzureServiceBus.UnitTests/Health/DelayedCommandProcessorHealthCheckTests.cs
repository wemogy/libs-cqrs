using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
public class DelayedCommandProcessorHealthCheckTests
{
    private readonly IServiceProvider _serviceProvider;

    public DelayedCommandProcessorHealthCheckTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBus(configuration["AzureServiceBusConnectionString"] !)

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintContextCommand>(
                "unit-testing-queue-1")
            .AddDelayedProcessor<PrintContextCommand>();

        serviceCollection.AddLogging();

        _serviceProvider = serviceCollection.BuildServiceProvider();
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

        // wait a bit for the hosted service to start and may process deprecated messages
        await Task.Delay(TimeSpan.FromSeconds(5));

        return hostedService;
    }
}
