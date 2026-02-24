using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Trace;
using Wemogy.Core.Errors;
using Wemogy.Core.Json;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Processors
{
    public class AzureServiceBusCommandProcessor<TCommand> : IAzureServiceBusCommandProcessorHostedService<TCommand>
        where TCommand : ICommandBase
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private readonly IServiceCollection _serviceCollection;
        private readonly string _handleMessageActivityName;
        private bool _isStarted;

        /// <summary>
        /// The hosted service is alive, if it is started and the service bus processor is not closed
        /// </summary>
        public bool IsAlive => _isStarted && !_serviceBusProcessor.IsClosed;

        public AzureServiceBusCommandProcessor(
            ServiceBusProcessor serviceBusProcessor,
            IServiceCollection serviceCollection)
        {
            _serviceBusProcessor = serviceBusProcessor;
            _serviceCollection = serviceCollection;
            _serviceBusProcessor.ProcessMessageAsync += HandleMessageAsync;
            _serviceBusProcessor.ProcessErrorAsync += (args) =>
            {
                Console.WriteLine("ProcessErrorAsync: " + args.Exception);
                return Task.CompletedTask;
            };
            _handleMessageActivityName = $"HandleMessageOf{typeof(TCommand).Name}";
        }

        public async Task HandleMessageAsync(ProcessMessageEventArgs arg)
        {
            using var activity = Observability.DefaultActivities.StartActivity(_handleMessageActivityName);
            var services = new ServiceCollection();
            foreach (var serviceDescriptor in _serviceCollection)
            {
                services.Add(serviceDescriptor);
            }

            var scheduledCommand = JsonSerializer.Deserialize<ScheduledCommand<TCommand>>(arg.Message.Body, WemogyJson.Options);

            if (scheduledCommand == null)
            {
                throw Error.Unexpected(
                    "ScheduledCommandIsNull",
                    "Scheduled command is null");
            }

            services.AddCommandQueryDependencies(scheduledCommand.Dependencies);

            try
            {
                using var scope = services.BuildServiceProvider().CreateScope();

                var scheduledCommandRunner = scope.ServiceProvider.GetRequiredService<IScheduledCommandRunner<TCommand>>();

                await scheduledCommandRunner.RunAsync(scheduledCommand);
            }
            catch (Exception e)
            {
                activity?.RecordException(e);
                throw;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var activity =
                Observability.DefaultActivities.StartActivity($"Starting {nameof(AzureServiceBusCommandSessionProcessor<TCommand>)}");
            await _serviceBusProcessor.StartProcessingAsync(cancellationToken);
            _isStarted = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var activity =
                Observability.DefaultActivities.StartActivity($"Stopping {nameof(AzureServiceBusCommandSessionProcessor<TCommand>)}");
            await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
            await _serviceBusProcessor.DisposeAsync();
            _isStarted = false;
        }
    }
}
