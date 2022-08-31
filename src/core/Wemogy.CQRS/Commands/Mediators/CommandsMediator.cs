using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Registries;

namespace Wemogy.CQRS.Commands.Mediators;

public class CommandsMediator : ICommands
{
    private readonly CommandRunnerRegistry _commandRunnerRegistry;

    public CommandsMediator(CommandRunnerRegistry commandRunnerRegistry)
    {
        _commandRunnerRegistry = commandRunnerRegistry;
    }

    public Task<TResult> RunAsync<TResult>(ICommand<TResult> command)
    {
        return _commandRunnerRegistry.ExecuteCommandRunnerAsync(command);
    }

    public Task<TResult> ScheduleDelayedAsync<TResult>(ICommand<TResult> command)
    {
        throw new NotImplementedException();
    }
}
