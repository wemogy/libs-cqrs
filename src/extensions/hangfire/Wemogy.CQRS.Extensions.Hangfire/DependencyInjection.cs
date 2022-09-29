using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.Hangfire.Services;

namespace Wemogy.CQRS.Extensions.Hangfire
{
    public static class DependencyInjection
    {
        public static void AddHangfireCQRSExtension(
            this IServiceCollection serviceCollection,
            bool enableRecurringJobService = true,
            bool enableDelayedJobService = true)
        {
            if (enableRecurringJobService)
            {
                serviceCollection.AddScoped<IRecurringJobService, HangfireRecurringJobService>();
            }

            if (enableDelayedJobService)
            {
                serviceCollection.AddScoped<IDelayedJobService, HangfireDelayedJobService>();
            }
        }
    }
}
