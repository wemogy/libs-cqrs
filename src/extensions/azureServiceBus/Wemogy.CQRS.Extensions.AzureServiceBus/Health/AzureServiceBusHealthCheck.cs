using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Trace;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Health
{
    /// <summary>
    /// This implementation is based on the https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/da70571ae83a2d83f93e45c0beb2a38a633a90d6/src/HealthChecks.AzureServiceBus/AzureServiceBusQueueHealthCheck.cs
    /// We created our own implementation, because we want to propagate the TaskCanceledException if the cancellation token has been canceled instead of returning a Unhealthy result.
    /// </summary>
    public class AzureServiceBusHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ServiceBusClient> ClientConnections =
            new ConcurrentDictionary<string, ServiceBusClient>();

        private static readonly ConcurrentDictionary<string, ServiceBusReceiver> ServiceBusReceivers =
            new ConcurrentDictionary<string, ServiceBusReceiver>();

        private readonly string _queueName;
        private readonly string _connectionString;

        public AzureServiceBusHealthCheck(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var activity = Observability.DefaultActivities.StartActivity();
            try
            {
                var client = ClientConnections.GetOrAdd(_queueName, _ => new ServiceBusClient(_connectionString));
                var receiver = ServiceBusReceivers.GetOrAdd($"{nameof(AzureServiceBusHealthCheck)}_{_queueName}", client.CreateReceiver(_queueName));
                _ = await receiver.PeekMessageAsync(cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (TaskCanceledException ex)
            {
                activity?.RecordException(ex);

                // propagate the exception if the cancellation token has been canceled
                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
