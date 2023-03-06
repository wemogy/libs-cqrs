using System.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions.Hangfire.Activators
{
    public class ScheduledCommandJobActivator : JobActivator
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly ScheduledCommandDependencies _scheduledCommandDependencies;

        public ScheduledCommandJobActivator(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _scheduledCommandDependencies = serviceCollection
                .BuildServiceProvider()
                .GetRequiredService<ScheduledCommandDependencies>();
        }

        public override JobActivatorScope BeginScope(PerformContext context)
        {
            var services = new ServiceCollection();
            foreach (var serviceDescriptor in _serviceCollection)
            {
                services.Add(serviceDescriptor);
            }

            var scheduledCommand = context.BackgroundJob.Job.Args.FirstOrDefault() as IScheduledCommand;

            foreach (var scheduledCommandDependency in _scheduledCommandDependencies)
            {
                services.AddScoped(
                    scheduledCommandDependency.Key,
                    _ =>
                    {
                        var dependency = scheduledCommand?.GetDependency(scheduledCommandDependency.Key);

                        if (dependency == null)
                        {
                            throw Error.Unexpected("DependencyNotFound", $"{scheduledCommandDependency.Key} dependency not found");
                        }

                        return dependency;
                    });
            }

            var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            return new ScheduledCommandJobActivatorScope(scope);
        }
    }
}
