using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintSessionId;

public class PrintSessionIdCommandHandler : ICommandHandler<PrintSessionIdCommand>
{
    public static List<PrintSessionIdCommand> ProcessedCommandsHistory { get; } = new List<PrintSessionIdCommand>();

    public Task HandleAsync(PrintSessionIdCommand command)
    {
        Console.WriteLine($"SessionId: {command.SessionId} on TestRunId: {command.TestRunId}");
        ProcessedCommandsHistory.Add(command);
        return Task.CompletedTask;
    }
}
