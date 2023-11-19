using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Wemogy.Core.Errors;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.ValueObjects;
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
            ScheduleOptions<TCommand> scheduleOptions)
            where TCommand : ICommandBase
        {
            string jobId;
            var serviceBusSender = GetServiceBusSender<TCommand>();
            var jsonBody = scheduledCommand.ToJson();
            var delayOptions = scheduleOptions.DelayOptions;
            var throttleOptions = scheduleOptions.ThrottleOptions;

            var message = new ServiceBusMessage()
            {
                Subject = string.Empty,
                Body = new BinaryData(jsonBody),
            };

            var sessionIdResolver = delayOptions?.SessionIdResolver ?? throttleOptions?.SessionIdResolver;

            if (sessionIdResolver != null)
            {
                _azureServiceBusSetupEnvironment.EnsureSessionIsSupported<TCommand>();
                var sessionId = sessionIdResolver(scheduledCommand.Command);
                message.SessionId = sessionId;
            }

            if (delayOptions != null)
            {
                if (delayOptions.Delay == TimeSpan.Zero)
                {
                    await serviceBusSender.SendMessageAsync(message);
                    jobId = ImmediateMessageJobId;
                }
                else
                {
                    var dateTimeOffset = DateTimeOffset.UtcNow.Add(delayOptions.Delay);
                    var sequenceNumber = await serviceBusSender.ScheduleMessageAsync(message, dateTimeOffset);
                    jobId = sequenceNumber.ToString();
                }
            }
            else if (throttleOptions != null)
            {
                var timestamp = DateTime.UtcNow;
                var throttlingKey = throttleOptions.GetThrottlingKey(scheduledCommand.Command, timestamp);
                var throttlePeriodEnd = throttleOptions.GetThrottlePeriodEnd(timestamp);

                // set messageId to throttling key, to use deduplication feature to delete other messages in period
                message.MessageId = throttlingKey;

                var sequenceNumber = await serviceBusSender.ScheduleMessageAsync(message, throttlePeriodEnd);
                jobId = sequenceNumber.ToString();
            }
            else
            {
                throw Error.Unexpected(
                    "InvalidScheduleOptions",
                    "Invalid schedule options. Please provide either delay options or throttle options.");
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
