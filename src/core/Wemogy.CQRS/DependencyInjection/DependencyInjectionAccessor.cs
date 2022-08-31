using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Wemogy.CQRS.DependencyInjection;

public static class DependencyInjectionAccessor
{
    private static IServiceCollection? _serviceCollection;
    private static IServiceProvider? _serviceProviderInstance;
    private static Assembly? _assembly;

    private static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceCollection == null)
            {
                throw new Exception(
                    "There is no service collection specified for Wemogy.CQRS. Please ensure to call services.AddCqrs() from your application layer.");
            }

            if (_serviceProviderInstance == null)
            {
                _serviceProviderInstance = _serviceCollection.BuildServiceProvider();
            }

            return _serviceProviderInstance;
        }
    }

    private static Assembly Assembly
    {
        get
        {
            if (_assembly == null)
            {
                throw new Exception(
                    "There is no Assembly specified for Wemogy.CQRS. Please ensure to call services.AddCqrs() from your application layer.");
            }

            return _assembly;
        }
    }

    public static void Initialize(IServiceCollection services, Assembly assembly)
    {
        _serviceCollection = services;
        _assembly = assembly;
    }

    public static TContext GetRequiredService<TContext>()
        where TContext : class
    {
        return ServiceProvider.GetRequiredService<TContext>();
    }

    public static List<Type> GetClassTypesWhichImplementInterface<TInterface>()
    {
        var interfaceType = typeof(TInterface);
        return Assembly.GetTypes()
            .Where(t => t.IsClass && interfaceType.IsAssignableFrom(t))
            .ToList();
    }

    public static List<TInterface> GetClassInstancesWhichImplementInterface<TInterface>()
    {
        return GetClassTypesWhichImplementInterface<TInterface>()
            .Select(t => (TInterface)ActivatorUtilities.CreateInstance(ServiceProvider, t))
            .ToList();
    }
}
