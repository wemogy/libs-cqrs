using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.Hangfire.Activators;
using Wemogy.CQRS.Extensions.Hangfire.Services;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.Hangfire
{
    public static class DependencyInjection
    {
        public static CQRSSetupEnvironment AddHangfire(
            this CQRSSetupEnvironment setupEnvironment,
            IServiceCollection serviceCollection,
            Action<IGlobalConfiguration>? configurationCallback = null,
            bool enableRecurringJobService = true,
            bool enableScheduledJobService = true)
        {
            serviceCollection.AddHangfire(config =>
            {
                config.UseScheduledCommandActivator(serviceCollection);
                if (configurationCallback == null)
                {
                    config.UseInMemoryStorage();
                }
                else
                {
                    configurationCallback(config);
                }
            });

            if (enableRecurringJobService)
            {
                serviceCollection.AddScoped<IRecurringCommandService, HangfireRecurringCommandService>();
            }

            if (enableScheduledJobService)
            {
                serviceCollection.AddScoped<IScheduledCommandService, HangfireScheduledCommandService>();
            }

            return setupEnvironment;
        }

        public static IGlobalConfiguration<ScheduledCommandJobActivator> UseScheduledCommandActivator(
            this IGlobalConfiguration configuration,
            IServiceCollection serviceCollection)
        {
            var jobActivator = new ScheduledCommandJobActivator(serviceCollection);
            return configuration.UseActivator(jobActivator);
        }
    }
}
