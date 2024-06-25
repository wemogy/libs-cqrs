using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Resolvers;

public class CommandQueryDependencyResolver : ICommandQueryDependencyResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandQueryDependencies _dependencyTypesMap;

    public CommandQueryDependencyResolver(IServiceProvider serviceProvider, CommandQueryDependencies dependencyTypesMap)
    {
        _serviceProvider = serviceProvider;
        _dependencyTypesMap = dependencyTypesMap;
    }

    public List<CommandQueryDependency> ResolveDependencies()
    {
        var result = new List<CommandQueryDependency>();

        foreach (var dependencyTypeMap in _dependencyTypesMap)
        {
            var dependency = _serviceProvider.GetRequiredService(dependencyTypeMap.Key);
            result.Add(CommandQueryDependency.Create(dependencyTypeMap.Key, dependencyTypeMap.Value, dependency));
        }

        return result;
    }
}
