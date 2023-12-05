using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wemogy.Configuration;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Processors;

[Collection("AzureServiceBus")]
public class AzureServiceBusCommandProcessorTests
{
    private readonly IServiceProvider _serviceProvider;

    public AzureServiceBusCommandProcessorTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBus(configuration["AzureServiceBusConnectionString"] !)

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintContextCommand>(builder =>
            {
                builder.WithQueueName("unit-testing-queue-1");
            })
            .AddDelayedProcessor<PrintContextCommand>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task IsAlive_ShouldBeTrueIfAzureServiceBusCommandProcessorIsObservingQueue()
    {
        // Arrange
        var hostedService = await StartHostedServiceAsync();

        // Act
        var isAlive = hostedService.IsAlive;

        // Assert
        isAlive.Should().BeTrue();
    }

    [Fact]
    public async Task IsAlive_ShouldBeFalseIfAzureServiceBusCommandProcessorWasStopped()
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
    public void IsAlive_ShouldBeFalseIfAzureServiceBusCommandProcessorWasNotStarted()
    {
        // Arrange
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<PrintContextCommand>>()
            .First();

        // Act
        var isAlive = hostedService.IsAlive;

        // Assert
        isAlive.Should().BeFalse();
    }

    private async Task<IAzureServiceBusCommandProcessorHostedService<PrintContextCommand>> StartHostedServiceAsync()
    {
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<PrintContextCommand>>()
            .First();
        await hostedService.StartAsync(CancellationToken.None);

        // wait a bit for the hosted service to start and may process deprecated messages
        await Task.Delay(TimeSpan.FromSeconds(5));

        return hostedService;
    }
}
