using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wemogy.Configuration;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintSessionId;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Processors;

[Collection("AzureServiceBus")]
public class AzureServiceBusCommandSessionProcessorTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private IAzureServiceBusCommandProcessorHostedService<PrintSessionIdCommand>? _startedHostedService;

    public AzureServiceBusCommandSessionProcessorTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var serviceCollection = new ServiceCollection();
        _serviceBusClient = new ServiceBusClient(configuration["AzureServiceBusConnectionString"] !);

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBusWithClient(_serviceBusClient)

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintSessionIdCommand>(builder =>
            {
                builder
                    .WithQueueName("unit-testing-queue-sessions")
                    .WithSessionSupport();
            })
            .AddDelayedSessionProcessor<PrintSessionIdCommand>();

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
    public async Task IsAlive_ShouldBeTrueIfAzureServiceBusCommandSessionProcessorIsObservingQueue()
    {
        // Arrange
        var hostedService = await StartHostedServiceAsync();

        // Act
        var isAlive = hostedService.IsAlive;

        // Assert
        isAlive.Should().BeTrue();
    }

    [Fact]
    public async Task IsAlive_ShouldBeFalseIfAzureServiceBusCommandSessionProcessorWasStopped()
    {
        // Arrange
        var hostedService = await StartHostedServiceAsync();
        await hostedService.StopAsync(CancellationToken.None);

        // Act
        var isAlive = hostedService.IsAlive;

        // Assert
        isAlive.Should().BeFalse();
    }

    [Fact]
    public void IsAlive_ShouldBeFalseIfAzureServiceBusCommandSessionProcessorWasNotStarted()
    {
        // Arrange
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<PrintSessionIdCommand>>()
            .First();

        // Act
        var isAlive = hostedService.IsAlive;

        // Assert
        isAlive.Should().BeFalse();
    }

    private async Task<IAzureServiceBusCommandProcessorHostedService<PrintSessionIdCommand>> StartHostedServiceAsync()
    {
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<PrintSessionIdCommand>>()
            .First();
        await hostedService.StartAsync(CancellationToken.None);
        _startedHostedService = hostedService;

        // wait a bit for the hosted service to start and may process deprecated messages
        await Task.Delay(TimeSpan.FromSeconds(5));

        return hostedService;
    }
}
