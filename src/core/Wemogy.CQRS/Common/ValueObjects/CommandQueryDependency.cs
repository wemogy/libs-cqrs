using System;
using Wemogy.Core.Extensions;

namespace Wemogy.CQRS.Common.ValueObjects;

public class CommandQueryDependency
{
    public Type DependencyType { get; set; }

    public Type ImplementationType { get; set; }

    public string Value { get; set; }

    public CommandQueryDependency(Type dependencyType, Type implementationType, string value)
    {
        DependencyType = dependencyType;
        ImplementationType = implementationType;
        Value = value;
    }

    public static CommandQueryDependency Create(Type dependencyType, Type implementationType, object value)
    {
        return new CommandQueryDependency(dependencyType, implementationType, value.ToJson());
    }
}
