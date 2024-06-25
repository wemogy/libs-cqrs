using System.Collections.Generic;
using System.Text.Json.Serialization;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Common.ValueObjects;

public class CommandRequest<TCommand>
    where TCommand : ICommandBase
{
    public TCommand Command { get; set; }

    public List<CommandQueryDependency> Dependencies { get; set; }

    public CommandRequest(
        TCommand command,
        List<CommandQueryDependency> dependencies)
    {
        Command = command;
        Dependencies = dependencies;
    }
}
