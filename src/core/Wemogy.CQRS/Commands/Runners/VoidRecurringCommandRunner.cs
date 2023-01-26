using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class VoidRecurringCommandRunner<TCommand>
    where TCommand : ICommand
{
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly VoidPostProcessingRunner<TCommand> _postProcessingRunner;
    private readonly IRecurringCommandService? _recurringCommandService;

    public VoidRecurringCommandRunner(
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand> commandHandler,
        VoidPostProcessingRunner<TCommand> postProcessingRunner,
        IRecurringCommandService? recurringCommandService = null)
    {
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _recurringCommandService = recurringCommandService;
    }

    public async Task ScheduleAsync(string recurringCommandId, TCommand command, string cronExpression)
    {
        if (_recurringCommandService == null)
        {
            throw Error.Unexpected(
                "RecurringJobServiceNotRegistered",
                "RecurringJobService is not registered. Please register it in the DI container.");
        }

        // pre-checking
        await _preProcessingRunner.RunPreChecksAsync(command);

        // schedule recurring command
        await _recurringCommandService.AddOrUpdateAsync(
            recurringCommandId,
            () => RunAsync(command),
            cronExpression);
    }

    public async Task RunAsync(TCommand command)
    {
        // pre-processing
        await _preProcessingRunner.RunPreProcessorsAsync(command);

        // processing
        await _commandHandler.HandleAsync(command);

        // post-processing
        await _postProcessingRunner.RunAsync(command);
    }
}
