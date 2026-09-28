using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Serialization;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Processors
{
    public class AzureServiceBusCommandSessionProcessor<TCommand> : IAzureServiceBusCommandProcessorHostedService<TCommand>
        where TCommand : ICommandBase
    {
        private readonly ServiceBusSessionProcessor _serviceBusSessionProcessor;
        private readonly ScheduledCommandServiceProviders _serviceProviders;
        private readonly bool _isRawCommandMessageSupported;
        private bool _isStarted;

        /// <summary>
        /// The hosted service is alive, if it is started and the service bus session processor is not closed
        /// </summary>
        public bool IsAlive => _isStarted && !_serviceBusSessionProcessor.IsClosed;

        private readonly string _handleMessageActivityName;

        public AzureServiceBusCommandSessionProcessor(
            ServiceBusSessionProcessor serviceBusSessionProcessor,
            IServiceCollection serviceCollection,
            bool isRawCommandMessageSupported = false)
        {
            _serviceBusSessionProcessor = serviceBusSessionProcessor;
            _serviceProviders = new ScheduledCommandServiceProviders(serviceCollection);
            _isRawCommandMessageSupported = isRawCommandMessageSupported;
            _serviceBusSessionProcessor.ProcessMessageAsync += HandleMessageAsync;
            _serviceBusSessionProcessor.ProcessErrorAsync += (args) =>
            {
                Console.WriteLine(args.Exception);
                return Task.CompletedTask;
            };
            _handleMessageActivityName = $"HandleMessageOf{typeof(TCommand).Name}";
        }

        public async Task HandleMessageAsync(ProcessSessionMessageEventArgs arg)
        {
            using var activity = Observability.DefaultActivities.StartActivity(_handleMessageActivityName);
            var scheduledCommand = ScheduledCommandDeserializer.Deserialize<TCommand>(
                arg.Message.Body,
                _isRawCommandMessageSupported,
                arg.Message.GetMessageFormat());

            using var scope = _serviceProviders.CreateScope(scheduledCommand.Dependencies);

            try
            {
                var scheduledCommandRunner =
                    scope.ServiceProvider.GetRequiredService<IScheduledCommandRunner<TCommand>>();
                await scheduledCommandRunner.RunAsync(scheduledCommand);
            }
            catch (Exception e)
            {
                activity?.AddException(e);
                throw;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var activity =
                Observability.DefaultActivities.StartActivity($"Starting {nameof(AzureServiceBusCommandSessionProcessor<TCommand>)}");
            await _serviceBusSessionProcessor.StartProcessingAsync(cancellationToken);
            _isStarted = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var activity =
                Observability.DefaultActivities.StartActivity($"Stopping {nameof(AzureServiceBusCommandSessionProcessor<TCommand>)}");
            await _serviceBusSessionProcessor.StopProcessingAsync(cancellationToken);
            await _serviceProviders.DisposeAsync();
            _isStarted = false;
        }
    }
}
