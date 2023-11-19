using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wemogy.Configuration;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.ValueObjects;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintHelloWorld;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Services;

public class AzureServiceBusScheduledCommandServiceDebounceTests
{
    private readonly ICommands _commands;
    private readonly IServiceProvider _serviceProvider;
    public AzureServiceBusScheduledCommandServiceDebounceTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBus(configuration["AzureServiceBusConnectionString"] !)

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintContextCommand>(
            "unit-testing-queue-duplicate-detection")
            .AddDelayedProcessor<PrintContextCommand>()
            .AddDelayedProcessor<PrintHelloWorld>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _commands = _serviceProvider.GetRequiredService<ICommands>();
    }

    [Fact]
    public async Task ScheduleAsyncDebounced_ShouldWork()
    {
        // Arrange
        var command = new PrintContextCommand();
        await StartHostedServiceAsync();

        // Act
        for (int i = 0; i < 10; i++)
        {
            await _commands.ScheduleAsync(command, new ThrottleOptions<PrintContextCommand>(
                x =>
                    x.TenantId ?? "default",
                TimeSpan.FromSeconds(5)));
        }

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
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
