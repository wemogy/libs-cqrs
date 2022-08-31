using System;
using System.Reflection;

namespace Wemogy.CQRS.Common.ValueObjects;

public class TypeMethodRegistryEntry
{
    public Type Type { get; }

    public MethodInfo Method { get; }

    public TypeMethodRegistryEntry(Type type, MethodInfo method)
    {
        Type = type;
        Method = method;
    }
}
