using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions;

public static class ServiceCollectionExtensions
{
    public static void TryAddScopedGenericType(
        this IServiceCollection serviceCollection,
        Type genericType,
        params Type[] genericTypeArguments)
    {
        var serviceType = genericType.MakeGenericType(genericTypeArguments);
        serviceCollection.TryAddScoped(serviceType);
    }

    public static void AddCommandQueryDependencies(
        this IServiceCollection services,
        List<CommandQueryDependency> commandQueryDependencies)
    {
        foreach (var commandQueryDependency in commandQueryDependencies)
        {
            services.AddScoped(
                commandQueryDependency.DependencyType,
                _ => commandQueryDependency.Value.FromJson(commandQueryDependency.ImplementationType) !);
        }
    }

    public static void AddImplementationsOfGenericInterfaceScoped(
        this IServiceCollection serviceCollection,
        Type genericInterfaceType,
        List<Assembly> assemblies)
    {
        var implementationTypes = assemblies.GetClassTypesWhichImplementInterface(genericInterfaceType);
        foreach (var implementationType in implementationTypes)
        {
            // find interface of implementation type that is of type interfaceType
            var interfaceTypeOfImplementation = implementationType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);

            serviceCollection.TryAddScopedExact(interfaceTypeOfImplementation, implementationType);
        }
    }

    /// <summary>
    /// The difference between this method and Microsoft.Extensions.DependencyInjection.Extensions.TryAddScoped is that this method will check the service type AND implementation type.
    /// The TryAddScoped method only checks the service type, which means that if the service type is the same but the implementation type is different, it will add only the first one.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the service to.</param>
    /// <param name="serviceType">The type of the service to add.</param>
    /// <param name="implementationType">The type of the implementation to add.</param>
    public static void TryAddScopedExact(
        this IServiceCollection serviceCollection,
        Type serviceType,
        Type implementationType)
    {
        // skip if already registered a service with the same interface AND implementation
        if (serviceCollection.Any(
            x => x.ServiceType == serviceType && x.ImplementationType == implementationType))
        {
            return;
        }

        serviceCollection.AddScoped(serviceType, implementationType);
    }
}
