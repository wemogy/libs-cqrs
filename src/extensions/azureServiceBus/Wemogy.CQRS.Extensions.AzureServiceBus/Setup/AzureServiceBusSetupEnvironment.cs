using System;
using System.Collections.Generic;
using System.Threading;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Config;
using Wemogy.CQRS.Extensions.AzureServiceBus.Processors;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Setup
{
    public class AzureServiceBusSetupEnvironment
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceCollection _serviceCollection;
        private readonly Dictionary<Type, DelayedProcessingOptions> _delayedProcessingOptions;
        private readonly HashSet<Type> _commandTypesWithRegisteredProcessor;

        public AzureServiceBusSetupEnvironment(ServiceBusClient serviceBusClient, IServiceCollection serviceCollection)
        {
            _serviceBusClient = serviceBusClient;
            _serviceCollection = serviceCollection;
            _delayedProcessingOptions = new Dictionary<Type, DelayedProcessingOptions>();
            _commandTypesWithRegisteredProcessor = new HashSet<Type>();
        }

        /// <summary>
        /// Creates a ServiceBusProcessor and subscribes to messages of type <typeparamref name="TCommand"/>
        /// </summary>
        public AzureServiceBusSetupEnvironment AddDelayedProcessor<TCommand>(
            int maxConcurrentCalls = 1,
            Action<ServiceBusProcessorOptions>? configureServiceBusProcessorOptions = null)
            where TCommand : ICommandBase
        {
            var queueName = GetQueueName<TCommand>();

            _serviceCollection.AddHostedService<IDelayedCommandProcessorHostedService<TCommand>>(_ =>
            {
                var serviceBusProcessorOptions = new ServiceBusProcessorOptions()
                {
                    MaxConcurrentCalls = maxConcurrentCalls
                };

                configureServiceBusProcessorOptions?.Invoke(serviceBusProcessorOptions);

                var serviceBusProcessor = _serviceBusClient.CreateProcessor(queueName, serviceBusProcessorOptions);
                var processor = new AzureServiceBusCommandProcessor<TCommand>(serviceBusProcessor, _serviceCollection);

                return processor;
            });

            _commandTypesWithRegisteredProcessor.Add(typeof(TCommand));

            return this;
        }

        /// <summary>
        /// Creates a ServiceBusProcessor and subscribes to messages of type <typeparamref name="TCommand"/>
        /// </summary>
        /// <param name="maxConcurrentSessions">The maximum number of concurrent sessions (default 1)</param>
        /// <param name="maxConcurrentCallsPerSession">The maximum number of concurrent calls per session (default 1)</param>
        /// <param name="configureSessionProcessorOptions">Optional custom configuration of the ServiceBusSessionProcessorOptions</param>
        public AzureServiceBusSetupEnvironment AddDelayedSessionProcessor<TCommand>(
            int maxConcurrentSessions = 1,
            int maxConcurrentCallsPerSession = 1,
            Action<ServiceBusSessionProcessorOptions>? configureSessionProcessorOptions = null)
            where TCommand : ICommandBase
        {
            var queueName = GetQueueName<TCommand>();

            if (!_delayedProcessingOptions.TryGetValue(typeof(TCommand), out var delayedProcessingOptions) || !delayedProcessingOptions.IsSessionSupported)
            {
                throw Error.PreconditionFailed(
                    "SessionSupportIsNotEnabled",
                    "You need to enable session support using ConfigureDelayedProcessing before using AddDelayedSessionProcessor");
            }

            _serviceCollection.AddHostedService<IDelayedCommandProcessorHostedService<TCommand>>(_ =>
            {
                var serviceBusSessionProcessorOptions = new ServiceBusSessionProcessorOptions()
                {
                    MaxConcurrentSessions = maxConcurrentSessions,
                    MaxConcurrentCallsPerSession = maxConcurrentCallsPerSession,
                    SessionIdleTimeout = TimeSpan.FromSeconds(2),
                    MaxAutoLockRenewalDuration = Timeout.InfiniteTimeSpan
                };

                configureSessionProcessorOptions?.Invoke(serviceBusSessionProcessorOptions);

                var serviceBusSessionProcessor = _serviceBusClient.CreateSessionProcessor(
                    queueName,
                    serviceBusSessionProcessorOptions);
                var processor = new AzureServiceBusCommandSessionProcessor<TCommand>(
                    serviceBusSessionProcessor,
                    _serviceCollection);

                return processor;
            });

            _commandTypesWithRegisteredProcessor.Add(typeof(TCommand));

            return this;
        }

        /// <summary>
        /// Use this method to configure how the delayed command should be scheduled in Azure Service Bus
        /// </summary>
        public AzureServiceBusSetupEnvironment ConfigureDelayedProcessing<TCommand>(Action<DelayedProcessingOptionsBuilder> configure)
            where TCommand : ICommandBase
        {
            if (_commandTypesWithRegisteredProcessor.Contains(typeof(TCommand)))
            {
                throw Error.Unexpected(
                    "WrongCallOrder",
                    "You must configure the delayed processing options before adding the delayed processor");
            }

            var builder = new DelayedProcessingOptionsBuilder();
            configure(builder);
            var delayedProcessingOptions = builder.Build();

            _delayedProcessingOptions.Add(typeof(TCommand), delayedProcessingOptions);
            return this;
        }

        public string GetQueueName<TCommand>()
        {
            _delayedProcessingOptions.TryGetValue(typeof(TCommand), out var delayedProcessingOptions);
            return delayedProcessingOptions?.QueueName ?? typeof(TCommand).Name.ToLower();
        }

        public void EnsureSessionIsSupported<TCommand>()
        {
            if (!_delayedProcessingOptions.TryGetValue(typeof(TCommand), out var delayedProcessingOptions) ||
                !delayedProcessingOptions.IsSessionSupported)
            {
                throw Error.PreconditionFailed(
                    "SessionSupportIsNotEnabled",
                    "You need to enable session support using AddDelayedSessionProcessor instead of AddDelayedProcessor");
            }
        }
    }
}
