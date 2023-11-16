using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.AzureServiceBus.Setup;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Services
{
    public class AzureServiceBusScheduledCommandService : IScheduledCommandService
    {
        private const string ImmediateMessageJobId = "IMMEDIATE_MESSAGE";
        private readonly ServiceBusClient _serviceBusClient;
        private readonly AzureServiceBusSetupEnvironment _azureServiceBusSetupEnvironment;
        private readonly Dictionary<string, ServiceBusSender> _serviceBusSenders;

        public AzureServiceBusScheduledCommandService(ServiceBusClient serviceBusClient, AzureServiceBusSetupEnvironment azureServiceBusSetupEnvironment)
        {
            _serviceBusClient = serviceBusClient;
            _azureServiceBusSetupEnvironment = azureServiceBusSetupEnvironment;
            _serviceBusSenders = new Dictionary<string, ServiceBusSender>();
        }

        private ServiceBusSender GetServiceBusSender<TCommand>()
        {
            var queueOrTopicName = _azureServiceBusSetupEnvironment.GetQueueName<TCommand>();
            if (!_serviceBusSenders.TryGetValue(queueOrTopicName, out var serviceBusSender))
            {
                serviceBusSender = _serviceBusClient.CreateSender(queueOrTopicName);
                _serviceBusSenders.TryAdd(queueOrTopicName, serviceBusSender);
            }

            return serviceBusSender;
        }

        public async Task<string> ScheduleAsync<TCommand>(
            IScheduledCommandRunner<TCommand> scheduledCommandRunner,
            ScheduledCommand<TCommand> scheduledCommand,
            TimeSpan delay)
            where TCommand : notnull
        {
            string jobId;
            var serviceBusSender = GetServiceBusSender<TCommand>();

            // we use Newtonsoft.Json here, because it supports serializing of Type, which we use in ScheduledCommand
            var jsonBody = scheduledCommand.ToJson();

            var message = new ServiceBusMessage()
            {
                Subject = string.Empty,
                Body = new BinaryData(jsonBody),
            };

            if (delay == TimeSpan.Zero)
            {
                await serviceBusSender.SendMessageAsync(message);
                jobId = ImmediateMessageJobId;
            }
            else
            {
                var dateTimeOffset = DateTimeOffset.UtcNow.Add(delay);
                var sequenceNumber = await serviceBusSender.ScheduleMessageAsync(message, dateTimeOffset);
                jobId = sequenceNumber.ToString();
            }

            return jobId;
        }

        public Task DeleteAsync<TCommand>(string jobId)
            where TCommand : ICommandBase
        {
            if (long.TryParse(jobId, out var jobIdNumber))
            {
                var serviceBusSender = GetServiceBusSender<TCommand>();
                return serviceBusSender.CancelScheduledMessageAsync(jobIdNumber);
            }

            return Task.CompletedTask;
        }
    }
}
