using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Config;
using Wemogy.CQRS.Extensions.AzureServiceBus.Processors;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Setup
{
    public class AzureServiceBusSetupEnvironment
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceCollection _serviceCollection;
        private readonly Dictionary<Type, DelayedProcessingOptions> _delayedProcessingOptions;
        private readonly Dictionary<Type, IAzureServiceBusCommandProcessor> _serviceBusProcessors;

        public AzureServiceBusSetupEnvironment(ServiceBusClient serviceBusClient, IServiceCollection serviceCollection)
        {
            _serviceBusClient = serviceBusClient;
            _serviceCollection = serviceCollection;
            _delayedProcessingOptions = new Dictionary<Type, DelayedProcessingOptions>();
            _serviceBusProcessors = new Dictionary<Type, IAzureServiceBusCommandProcessor>();
        }

        /// <summary>
        /// Creates a ServiceBusProcessor and subscribes to messages of type <typeparamref name="TCommand"/>
        /// </summary>
        public AzureServiceBusSetupEnvironment AddDelayedProcessor<TCommand>(
            int maxConcurrentCalls = 1)
            where TCommand : ICommandBase
        {
            var queueName = GetQueueName<TCommand>();

            var serviceBusProcessor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions()
            {
                MaxConcurrentCalls = maxConcurrentCalls
            });
            var processor = new AzureServiceBusCommandProcessor<TCommand>(serviceBusProcessor, _serviceCollection);

            _serviceBusProcessors.Add(typeof(TCommand), processor);

            return this;
        }

        /// <summary>
        /// Use this method to configure how the delayed command should be scheduled in Azure Service Bus
        /// </summary>
        public AzureServiceBusSetupEnvironment ConfigureDelayedProcessing<TCommand>(
            string queueName)
            where TCommand : ICommandBase
        {
            if (_serviceBusProcessors.ContainsKey(typeof(TCommand)))
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
    }
}
