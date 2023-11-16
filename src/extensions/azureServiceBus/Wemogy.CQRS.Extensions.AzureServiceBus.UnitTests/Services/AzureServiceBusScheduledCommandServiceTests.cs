using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Configuration;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.Extensions.Hangfire.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Services;

public class AzureServiceBusScheduledCommandServiceTests
{
    private readonly ICommands _commands;
    public AzureServiceBusScheduledCommandServiceTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        PrintContextCommandHandler.Reset();
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBus(
                configuration["AzureServiceBusConnectionString"])

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintContextCommand>(
                "unit-testing-queue-1")
            .AddDelayedProcessor<PrintContextCommand>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _commands = serviceProvider.GetRequiredService<ICommands>();
    }

    [Fact]
    public async Task ScheduleAsync_ShouldWorkWhenPassingADelay()
    {
        // Arrange
        var command = new PrintContextCommand();

        // Act
        await _commands.ScheduleAsync(command, TimeSpan.FromSeconds(5));

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        PrintContextCommandHandler.ExecutedCount.Should().Be(0);
        await Task.Delay(TimeSpan.FromSeconds(5));
        PrintContextCommandHandler.ExecutedCount.Should().Be(1);
    }

    [Fact]
    public async Task ScheduleAsync_ShouldWorkWithoutPassingDelay()
    {
        // Arrange
        var command = new PrintContextCommand();

        // Act
        await _commands.ScheduleAsync(command);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        PrintContextCommandHandler.ExecutedCount.Should().Be(1);
    }
}
