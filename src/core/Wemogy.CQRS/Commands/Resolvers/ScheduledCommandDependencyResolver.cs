using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Resolvers;

public class ScheduledCommandDependencyResolver : IScheduledCommandDependencyResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _dependencyTypesMap;

    public ScheduledCommandDependencyResolver(IServiceProvider serviceProvider, Dictionary<Type, Type> dependencyTypesMap)
    {
        _serviceProvider = serviceProvider;
        _dependencyTypesMap = dependencyTypesMap;
    }

    public List<ScheduledCommandDependency> ResolveDependencies()
    {
        var result = new List<ScheduledCommandDependency>();

        foreach (var dependencyTypeMap in _dependencyTypesMap)
        {
            var dependency = _serviceProvider.GetRequiredService(dependencyTypeMap.Key);
            result.Add(ScheduledCommandDependency.Create(dependencyTypeMap.Key, dependencyTypeMap.Value, dependency));
        }

        return result;
    }
}
