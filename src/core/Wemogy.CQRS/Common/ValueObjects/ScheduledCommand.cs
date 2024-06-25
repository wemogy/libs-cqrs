using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Common.ValueObjects;

public class ScheduledCommand<TCommand> : IScheduledCommand
    where TCommand : notnull
{
    [JsonInclude]
    public List<CommandQueryDependency> Dependencies { get; private set; }

    [JsonInclude]
    public Type CommandType { get; private set; }

    public TCommand Command { get; set; }

    public ScheduledCommand(List<CommandQueryDependency> dependencies, TCommand command)
    {
        Dependencies = dependencies;
        CommandType = command.GetType();
        Command = command;
    }
}
