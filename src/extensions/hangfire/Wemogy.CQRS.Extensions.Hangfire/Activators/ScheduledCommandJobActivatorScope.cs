using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Wemogy.CQRS.Extensions.Hangfire.Activators
{
    public class ScheduledCommandJobActivatorScope : JobActivatorScope
    {
        private readonly IServiceScope _serviceScope;

        public ScheduledCommandJobActivatorScope(IServiceScope serviceScope)
        {
            _serviceScope = serviceScope;
        }

        public override object Resolve(Type type)
        {
            var inst = ActivatorUtilities.CreateInstance(_serviceScope.ServiceProvider, type);
            return inst;
        }
    }
}
