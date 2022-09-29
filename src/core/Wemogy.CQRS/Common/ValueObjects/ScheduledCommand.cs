using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Common.ValueObjects;

public class ScheduledCommand<TCommand> : IScheduledCommand
{
    [JsonInclude]
    public List<ScheduledCommandDependency> Dependencies { get; private set; }

    public TCommand Command { get; set; }

    public ScheduledCommand(List<ScheduledCommandDependency> dependencies, TCommand command)
    {
        Dependencies = dependencies;
        Command = command;
    }

    public object? GetDependency(Type type)
    {
        var dependency = Dependencies
            .FirstOrDefault(x => x.DependencyType == type);

        var json = dependency?.Value;

        if(json is null)
        {
            return null;
        }

        return json?.FromJson(dependency.ImplementationType);
    }
}
