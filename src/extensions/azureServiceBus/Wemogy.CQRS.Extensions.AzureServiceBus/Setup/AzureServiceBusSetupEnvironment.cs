using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors;
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
            int maxConcurrentCalls = 1)
            where TCommand : ICommandBase
        {
            var queueName = GetQueueName<TCommand>();

            _serviceCollection.AddHostedService(_ =>
            {
                var serviceBusProcessor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions()
                {
                    MaxConcurrentCalls = maxConcurrentCalls
                });
                var processor = new AzureServiceBusCommandProcessor<TCommand>(serviceBusProcessor, _serviceCollection);

                return processor;
            });

            _commandTypesWithRegisteredProcessor.Add(typeof(TCommand));

            return this;
        }

        /// <summary>
        /// Creates a ServiceBusProcessor and subscribes to messages of type <typeparamref name="TCommand"/>
        /// </summary>
        public AzureServiceBusSetupEnvironment AddDelayedSessionProcessor<TCommand>(
            int maxConcurrentSessions = 1)
            where TCommand : ICommandBase
        {
            var queueName = GetQueueName<TCommand>();

            if (!_delayedProcessingOptions.TryGetValue(typeof(TCommand), out var delayedProcessingOptions))
            {
                delayedProcessingOptions = new DelayedProcessingOptions();
                _delayedProcessingOptions.Add(typeof(TCommand), delayedProcessingOptions);
            }

            delayedProcessingOptions.IsSessionSupported = true;

            _serviceCollection.AddHostedService(_ =>
            {
                var serviceBusSessionProcessor = _serviceBusClient.CreateSessionProcessor(
                    queueName,
                    new ServiceBusSessionProcessorOptions()
                    {
                        MaxConcurrentSessions = maxConcurrentSessions
                    });
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
        public AzureServiceBusSetupEnvironment ConfigureDelayedProcessing<TCommand>(
            string? queueName = null,
            Func<TCommand, string>? sessionIdResolver = null)
            where TCommand : ICommandBase
        {
            if (_commandTypesWithRegisteredProcessor.Contains(typeof(TCommand)))
            {
                throw Error.Unexpected(
                    "WrongCallOrder",
                    "You must configure the delayed processing options before adding the delayed processor");
            }

            _delayedProcessingOptions.Add(typeof(TCommand), new DelayedProcessingOptions()
            {
                QueueName = queueName
            });
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
