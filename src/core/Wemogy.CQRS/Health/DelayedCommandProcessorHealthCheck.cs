using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Health;

public class DelayedCommandProcessorHealthCheck<TCommand> : IHealthCheck
    where TCommand : ICommandBase
{
    private readonly IDelayedCommandProcessorHostedService<TCommand>? _delayedCommandProcessorHostedService;

    public DelayedCommandProcessorHealthCheck(IEnumerable<IHostedService> hostedServices)
    {
        _delayedCommandProcessorHostedService = hostedServices
            .OfType<IDelayedCommandProcessorHostedService<TCommand>>()
            .FirstOrDefault();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_delayedCommandProcessorHostedService is null)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"No hosted service found for the specified command type. Please make sure you have called AddDelayedProcessor<{nameof(TCommand)}> in your Startup.cs"));
        }

        if (_delayedCommandProcessorHostedService.IsAlive)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }

        return Task.FromResult(HealthCheckResult.Unhealthy());
    }
}
