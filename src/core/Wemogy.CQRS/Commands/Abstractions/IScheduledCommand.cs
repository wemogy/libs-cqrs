using System;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IScheduledCommand
{
    public object? GetDependency(Type type);
}
