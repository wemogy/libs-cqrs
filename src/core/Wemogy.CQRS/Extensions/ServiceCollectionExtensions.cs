using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors;
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
        List<Assembly> assemblies,
        Type genericType,
        params Type[] genericTypeArguments)
    {
        var serviceType = genericType.MakeGenericType(genericTypeArguments);
        var serviceImplementations = assemblies.GetClassTypesWhichImplementInterface(serviceType);

        if (serviceImplementations.Count != 1)
        {
            throw Error.Unexpected(
                "InvalidServiceImplementation",
                $"There must be exactly one {serviceType.FullName} declared in the assemblies.");
        }

        var serviceImplementationType = serviceImplementations[0];

        serviceCollection.AddScoped(serviceType, serviceImplementationType);
    }
}
