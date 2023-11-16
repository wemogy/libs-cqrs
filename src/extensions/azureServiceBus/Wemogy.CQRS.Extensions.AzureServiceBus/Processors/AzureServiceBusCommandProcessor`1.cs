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
    public class AzureServiceBusCommandProcessor<TCommand> : IAzureServiceBusCommandProcessorHostedService<TCommand>
        where TCommand : ICommandBase
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private readonly IServiceCollection _serviceCollection;
        private readonly ScheduledCommandDependencies _scheduledCommandDependencies;

        public AzureServiceBusCommandProcessor(
            ServiceBusProcessor serviceBusProcessor,
            IServiceCollection serviceCollection)
        {
            _serviceBusProcessor = serviceBusProcessor;
            _serviceCollection = serviceCollection;
            _serviceBusProcessor.ProcessMessageAsync += HandleMessageAsync;
            _serviceBusProcessor.ProcessErrorAsync += (args) =>
            {
                Console.WriteLine(args.Exception);
                return Task.CompletedTask;
            };
            _scheduledCommandDependencies = serviceCollection
                .BuildServiceProvider()
                .GetRequiredService<ScheduledCommandDependencies>();
        }

        public async Task HandleMessageAsync(ProcessMessageEventArgs arg)
        {
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
                var scheduledCommandRunner = scope.ServiceProvider.GetRequiredService<IScheduledCommandRunner<TCommand>>();

                await scheduledCommandRunner.RunAsync(scheduledCommand);
            }
            catch (Exception e)
            {
                // ToDo: Dead letter message ==> Maybe remove try/catch let AutoComplete manage this
                Console.WriteLine(e);
                throw;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _serviceBusProcessor.StartProcessingAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _serviceBusProcessor.StopProcessingAsync(cancellationToken);
        }
    }
}
