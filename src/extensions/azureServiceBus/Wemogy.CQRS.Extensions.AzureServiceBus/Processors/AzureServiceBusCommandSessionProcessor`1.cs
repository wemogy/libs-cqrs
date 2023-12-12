using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        private readonly TimeSpan _renewSessionLockInterval;

        public AzureServiceBusCommandSessionProcessor(
            ServiceBusSessionProcessor serviceBusSessionProcessor,
            IServiceCollection serviceCollection,
            TimeSpan renewSessionLockInterval)
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
            _renewSessionLockInterval = renewSessionLockInterval;
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

            var renewSessionLockCancellationTokenSource = new CancellationTokenSource();
            var renewSessionLockCancellationToken = renewSessionLockCancellationTokenSource.Token;
            Task? renewSessionLockTask = null;
            try
            {
                var scheduledCommandRunner =
                    scope.ServiceProvider.GetRequiredService<IScheduledCommandRunner<TCommand>>();
                var scheduledCommandRunnerTask = scheduledCommandRunner.RunAsync(scheduledCommand);

                // renew session lock every 30 seconds
                renewSessionLockTask = Task.Run(
                    async () =>
                    {
                        while (!renewSessionLockCancellationToken.IsCancellationRequested &&
                                !scheduledCommandRunnerTask.IsCompleted)
                        {
                            await Task.Delay(_renewSessionLockInterval, renewSessionLockCancellationToken);
                            Console.WriteLine($"Renewing session lock for session{arg.SessionId}...");
                            await arg.RenewSessionLockAsync(arg.CancellationToken);
                            Console.WriteLine($"Renewed session lock for session {arg.SessionId}");
                        }
                    },
                    renewSessionLockCancellationTokenSource.Token);

                await scheduledCommandRunnerTask;
                renewSessionLockCancellationTokenSource.Cancel();
            }
            catch (Exception e)
            {
                // ToDo: Dead letter message ==> Maybe remove try/catch let AutoComplete manage this
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                renewSessionLockCancellationTokenSource.Dispose();
                renewSessionLockTask?.Dispose();
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
