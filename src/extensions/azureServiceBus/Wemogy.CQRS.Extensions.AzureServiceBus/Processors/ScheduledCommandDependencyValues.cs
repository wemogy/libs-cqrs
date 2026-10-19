using System;
using System.Collections.Generic;
using System.Linq;
using Wemogy.Core.Errors;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Processors
{
    /// <summary>
    /// Holds the dependency values of the scheduled command that runs in the current scope.
    /// </summary>
    internal sealed class ScheduledCommandDependencyValues
    {
        private IReadOnlyList<CommandQueryDependency> _dependencies = Array.Empty<CommandQueryDependency>();

        public void Set(IReadOnlyList<CommandQueryDependency> dependencies)
        {
            _dependencies = dependencies;
        }

        public object Resolve(Type dependencyType)
        {
            // The last dependency of a type wins, as the last registration did before.
            var dependency = _dependencies.LastOrDefault(x => x.DependencyType == dependencyType);

            if (dependency == null)
            {
                // The values are only set on the scope the processor creates for a message. Getting
                // here means the dependency was resolved outside of it, typically by a singleton
                // that injects it. That used to work while every message had its own provider.
                var message =
                    $"The scheduled command dependency {dependencyType.FullName} was resolved outside of the scope of a message. "
                    + "Singletons are shared across messages, so a service that injects a scheduled command dependency must be registered scoped or transient.";
                throw Error.Unexpected("ScheduledCommandDependencyResolvedOutsideOfMessageScope", message);
            }

            return dependency.Value.FromJson(dependency.ImplementationType) !;
        }
    }
}
