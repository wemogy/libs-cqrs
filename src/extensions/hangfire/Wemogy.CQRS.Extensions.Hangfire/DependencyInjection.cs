using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.Hangfire.Services;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.Hangfire
{
    public static class DependencyInjection
    {
        public static CQRSSetupEnvironment AddHangfire(
            this CQRSSetupEnvironment setupEnvironment,
            bool enableRecurringJobService = true,
            bool enableScheduledJobService = true)
        {
            var serviceCollection = setupEnvironment.ServiceCollection;
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
    }
}
