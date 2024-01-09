using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Health
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddAzureServiceBusCheck(
            this IHealthChecksBuilder builder,
            string connectionString,
            string queueName,
            string? name = null)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? nameof(AzureServiceBusHealthCheck),
                _ => new AzureServiceBusHealthCheck(connectionString, queueName),
                default,
                default,
                default));
        }
    }
}
