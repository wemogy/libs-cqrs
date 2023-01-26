using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Extensions;

namespace Wemogy.CQRS.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddScopedGenericType(
        this IServiceCollection serviceCollection,
        Type genericType,
        params Type[] genericTypeArguments)
    {
        var serviceType = genericType.MakeGenericType(genericTypeArguments);
        serviceCollection.AddScoped(serviceType);
    }

    public static void AddScopedGenericTypeWithImplementationFromAssembly(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        Type genericType,
        params Type[] genericTypeArguments)
    {
        var serviceType = genericType.MakeGenericType(genericTypeArguments);
        var serviceImplementations = assembly.GetClassTypesWhichImplementInterface(serviceType);
        if (serviceImplementations.Count != 1)
        {
            throw new Exception(
                $"There must be exactly one {serviceType.FullName} registered in {assembly.FullName}.");
        }

        var serviceImplementationType = serviceImplementations[0];

        serviceCollection.AddScoped(serviceType, serviceImplementationType);
    }
}
