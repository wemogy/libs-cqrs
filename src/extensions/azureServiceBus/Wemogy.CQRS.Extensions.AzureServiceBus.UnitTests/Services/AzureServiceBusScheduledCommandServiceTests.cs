using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wemogy.Configuration;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintHelloWorld;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Services;

[Collection("AzureServiceBus")]
public class AzureServiceBusScheduledCommandServiceTests
{
    private readonly ICommands _commands;
    private readonly IServiceProvider _serviceProvider;
    public AzureServiceBusScheduledCommandServiceTests()
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
            .AddDelayedProcessor<PrintContextCommand>()
            .AddDelayedProcessor<PrintHelloWorld>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _commands = _serviceProvider.GetRequiredService<ICommands>();
    }

    [Fact]
    public async Task ScheduleAsync_ShouldWorkWhenPassingADelay()
    {
        // Arrange
        var command = new PrintContextCommand();
        await StartHostedServiceAsync();

        // Act
        await _commands.ScheduleAsync(command, TimeSpan.FromSeconds(5));

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        PrintContextCommandHandler.ExecutedCount.Should().NotContainKey(command.Id);
        await Task.Delay(TimeSpan.FromSeconds(5));
        PrintContextCommandHandler.ExecutedCount[command.Id].Should().Be(1);
    }

    [Fact]
    public async Task ScheduleAsync_ShouldWorkWithoutPassingDelay()
    {
        // Arrange
        var command = new PrintContextCommand();
        await StartHostedServiceAsync();

        // Act
        await _commands.ScheduleAsync(command);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        PrintContextCommandHandler.ExecutedCount[command.Id].Should().Be(1);
    }

    [Fact]
    public async Task ScheduleAsync_ShouldWorkWhenCommandRunnerWasUsedBefore()
    {
        // Arrange
        var command = new PrintContextCommand();
        await StartHostedServiceAsync();
        await _commands.RunAsync(command);
        PrintContextCommandHandler.ExecutedCount.Clear();

        // Act
        await _commands.ScheduleAsync(command);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        PrintContextCommandHandler.ExecutedCount[command.Id].Should().Be(1);
    }

    private async Task StartHostedServiceAsync()
    {
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<PrintContextCommand>>()
            .First();
        await hostedService.StartAsync(CancellationToken.None);

        // wait a bit for the hosted service to start and may process deprecated messages
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}
