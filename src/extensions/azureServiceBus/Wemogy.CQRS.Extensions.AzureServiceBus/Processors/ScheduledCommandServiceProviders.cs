using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Processors
{
    /// <summary>
    /// Creates the DI scope in which a scheduled command runs.
    /// <para>
    /// A scheduled command carries its own dependencies (for example the tenant or user context
    /// that was active when it was scheduled), which must be resolvable as scoped services while
    /// it runs. Building a new <see cref="ServiceProvider"/> for every message achieves that, but it
    /// also re-creates every singleton per message and never disposes the provider. This class
    /// builds one provider per distinct set of dependency types instead and hands each scope its
    /// message's dependency values, so singletons are shared across messages.
    /// </para>
    /// </summary>
    internal sealed class ScheduledCommandServiceProviders : IAsyncDisposable
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly ConcurrentDictionary<string, Lazy<ServiceProvider>> _providers = new ();

        public ScheduledCommandServiceProviders(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// The number of service providers built so far (one per distinct set of dependency types).
        /// </summary>
        public int ProviderCount => _providers.Count;

        public IServiceScope CreateScope(List<CommandQueryDependency> dependencies)
        {
            var provider = _providers
                .GetOrAdd(GetKey(dependencies), _ => new Lazy<ServiceProvider>(() => BuildProvider(dependencies)))
                .Value;
            var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ScheduledCommandDependencyValues>().Set(dependencies);
            return scope;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var provider in _providers.Values.Where(x => x.IsValueCreated))
            {
                await provider.Value.DisposeAsync();
            }

            _providers.Clear();
        }

        private static string GetKey(List<CommandQueryDependency> dependencies)
        {
            return string.Join(
                "|",
                dependencies.Select(x => $"{x.DependencyType.AssemblyQualifiedName}>{x.ImplementationType.AssemblyQualifiedName}"));
        }

        private ServiceProvider BuildProvider(List<CommandQueryDependency> dependencies)
        {
            var services = new ServiceCollection();
            foreach (var serviceDescriptor in _serviceCollection)
            {
                services.Add(serviceDescriptor);
            }

            services.AddScoped<ScheduledCommandDependencyValues>();

            // Registered after the application's services, so a dependency overrides an
            // application registration of the same type - as the per-message registration did.
            foreach (var dependencyType in dependencies.Select(x => x.DependencyType).Distinct())
            {
                services.AddScoped(
                    dependencyType,
                    serviceProvider => serviceProvider
                        .GetRequiredService<ScheduledCommandDependencyValues>()
                        .Resolve(dependencyType));
            }

            return services.BuildServiceProvider();
        }
    }
}
