using System;
using System.Collections.Generic;
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
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintSessionId;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Services;

[Collection("AzureServiceBus")]
public class AzureServiceBusScheduledCommandServiceSessionTests
{
    private readonly ICommands _commands;
    private readonly IServiceProvider _serviceProvider;
    public AzureServiceBusScheduledCommandServiceSessionTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddTestApplication()

            // tell CQRS to use Azure Service Bus for delayed processing
            .AddAzureServiceBus(configuration["AzureServiceBusConnectionString"] !)

            // Configure QueueName, Message Session ID and etc.
            .ConfigureDelayedProcessing<PrintSessionIdCommand>(builder =>
            {
                builder
                    .WithQueueName("unit-testing-queue-sessions")
                    .WithSessionSupport();
            })
            .AddDelayedSessionProcessor<PrintSessionIdCommand>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _commands = _serviceProvider.GetRequiredService<ICommands>();
    }

    [Fact]
    public async Task Sessions_ShouldWork()
    {
        // Arrange
        var testRunId = Guid.NewGuid().ToString();
        var tenants = new[]
        {
            "tenant-1",
            "tenant-2",
            "tenant-3"
        };
        var totalMessagesCount = 0;
        List<PrintSessionIdCommand> GetProcessedCommandsHistory()
        {
            return PrintSessionIdCommandHandler.ProcessedCommandsHistory
                .Where(x => x.TestRunId == testRunId)
                .ToList();
        }

        // Act
        for (int i = 0; i < 10; i++)
        {
            foreach (var tenant in tenants)
            {
                var command = new PrintSessionIdCommand(tenant, testRunId);
                await _commands.ScheduleAsync(command, new DelayOptions<PrintSessionIdCommand>(
                    TimeSpan.Zero,
                    x => x.SessionId));
                totalMessagesCount++;
            }
        }

        // Start the hosted service and wait a while for it to process the messages
        var hostedService = await StartHostedServiceAsync();
        while (GetProcessedCommandsHistory().Count < totalMessagesCount)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        // Stop the hosted service
        await hostedService.StopAsync(CancellationToken.None);

        // Assert
        GetProcessedCommandsHistory()
            .GetRange(0, 10)
            .DistinctBy(x => x.SessionId)
            .Should()
            .HaveCount(1);

        GetProcessedCommandsHistory()
            .GetRange(10, 10)
            .DistinctBy(x => x.SessionId)
            .Should()
            .HaveCount(1);

        GetProcessedCommandsHistory()
            .GetRange(20, 10)
            .DistinctBy(x => x.SessionId)
            .Should()
            .HaveCount(1);
    }

    private async Task<IHostedService> StartHostedServiceAsync()
    {
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<PrintSessionIdCommand>>()
            .First();
        await hostedService.StartAsync(CancellationToken.None);

        // wait a bit for the hosted service to start and may process deprecated messages
        await Task.Delay(TimeSpan.FromSeconds(5));

        return hostedService;
    }
}
