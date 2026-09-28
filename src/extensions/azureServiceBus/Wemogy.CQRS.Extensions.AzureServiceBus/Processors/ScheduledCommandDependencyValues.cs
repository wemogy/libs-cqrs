using System;
using System.Collections.Generic;
using System.Linq;
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
            var dependency = _dependencies.Last(x => x.DependencyType == dependencyType);
            return dependency.Value.FromJson(dependency.ImplementationType) !;
        }
    }
}
