using System;
using Wemogy.Core.Extensions;

namespace Wemogy.CQRS.Common.ValueObjects;

public class ScheduledCommandDependency
{
    public Type DependencyType { get; set; }

    public Type ImplementationType { get; set; }

    public string Value { get; set; }

    public ScheduledCommandDependency(Type dependencyType, Type implementationType, string value)
    {
        DependencyType = dependencyType;
        ImplementationType = implementationType;
        Value = value;
    }

    public static ScheduledCommandDependency Create(Type dependencyType, Type implementationType, object value)
    {
        return new ScheduledCommandDependency(dependencyType, implementationType, value.ToJson());
    }
}
