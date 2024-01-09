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
    public class AzureServiceBusCommandSessionProcessor<TCommand> : IAzureServiceBusCommandProcessorHostedService<TCommand>
        where TCommand : ICommandBase
    {
        private readonly ServiceBusSessionProcessor _serviceBusSessionProcessor;
        private readonly IServiceCollection _serviceCollection;
        private readonly ScheduledCommandDependencies _scheduledCommandDependencies;
        private bool _isStarted;

        /// <summary>
        /// The hosted service is alive, if it is started and the service bus session processor is not closed
        /// </summary>
        public bool IsAlive => _isStarted && !_serviceBusSessionProcessor.IsClosed;

        private readonly string _handleMessageActivityName;

        public AzureServiceBusCommandSessionProcessor(
            ServiceBusSessionProcessor serviceBusSessionProcessor,
            IServiceCollection serviceCollection)
        {
            _serviceBusSessionProcessor = serviceBusSessionProcessor;
            _serviceCollection = serviceCollection;
            _serviceBusSessionProcessor.ProcessMessageAsync += HandleMessageAsync;
            _serviceBusSessionProcessor.ProcessErrorAsync += (args) =>
            {
                Console.WriteLine(args.Exception);
                return Task.CompletedTask;
            };
            _scheduledCommandDependencies = serviceCollection
                .BuildServiceProvider()
                .GetRequiredService<ScheduledCommandDependencies>();
            _handleMessageActivityName = $"HandleMessageOf{typeof(TCommand).Name}";
        }

        public async Task HandleMessageAsync(ProcessSessionMessageEventArgs arg)
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

            foreach (var scheduledCommandDependency in _scheduledCommandDependencies)
            {
                services.AddScoped(
                    scheduledCommandDependency.Key,
                    _ =>
                    {
                        var dependency = scheduledCommand.GetDependency(scheduledCommandDependency.Key);

                        if (dependency == null)
                        {
                            throw Error.Unexpected("DependencyNotFound", $"{scheduledCommandDependency.Key} dependency not found");
                        }

                        return dependency;
                    });
            }

            var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();

            try
            {
                var scheduledCommandRunner =
                    scope.ServiceProvider.GetRequiredService<IScheduledCommandRunner<TCommand>>();
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
            await _serviceBusSessionProcessor.StartProcessingAsync(cancellationToken);
            _isStarted = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var activity =
                Observability.DefaultActivities.StartActivity($"Stopping {nameof(AzureServiceBusCommandSessionProcessor<TCommand>)}");
            await _serviceBusSessionProcessor.StopProcessingAsync(cancellationToken);
            _isStarted = false;
        }
    }
}
