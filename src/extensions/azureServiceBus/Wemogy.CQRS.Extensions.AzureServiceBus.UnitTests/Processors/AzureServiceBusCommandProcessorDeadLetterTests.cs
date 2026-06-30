using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wemogy.Configuration;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.AlwaysFailing;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Processors;

[Collection("AzureServiceBus")]
public class AzureServiceBusCommandProcessorDeadLetterTests
{
    private const string QueueName = "unit-testing-queue-failing";
    private readonly ICommands _commands;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;

    public AzureServiceBusCommandProcessorDeadLetterTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        var connectionString = configuration["AzureServiceBusConnectionString"]!;
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddTestApplication()
            .AddAzureServiceBus(connectionString)
            .ConfigureDelayedProcessing<AlwaysFailingCommand>(builder =>
            {
                builder.WithQueueName(QueueName);
            })
            .AddDelayedProcessor<AlwaysFailingCommand>(maxDeliveryCount: 1);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _commands = _serviceProvider.GetRequiredService<ICommands>();
        _serviceBusClient = new ServiceBusClient(connectionString);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldDeadLetterWithActualException_WhenMaxDeliveryCountReached()
    {
        // Arrange
        var hostedService = _serviceProvider
            .GetServices<IHostedService>()
            .OfType<IAzureServiceBusCommandProcessorHostedService<AlwaysFailingCommand>>()
            .First();
        await hostedService.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Act
        await _commands.ScheduleAsync(new AlwaysFailingCommand());
        await Task.Delay(TimeSpan.FromSeconds(10));

        // Assert
        var dlqReceiver = _serviceBusClient.CreateReceiver(
            QueueName,
            new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

        var dlqMessage = await dlqReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
        dlqMessage.Should().NotBeNull();
        dlqMessage!.DeadLetterReason.Should().Be(nameof(InvalidOperationException));
        dlqMessage.DeadLetterErrorDescription.Should().Be(AlwaysFailingCommandHandler.ExceptionMessage);

        await dlqReceiver.CompleteMessageAsync(dlqMessage);
        await hostedService.StopAsync(CancellationToken.None);
    }
}
