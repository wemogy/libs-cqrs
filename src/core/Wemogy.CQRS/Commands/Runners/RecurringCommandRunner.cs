using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class RecurringCommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly PreProcessingRunner<TCommand, TResult> _preProcessingRunner;
    private readonly ICommandHandler<TCommand, TResult> _commandHandler;
    private readonly PostProcessingRunner<TCommand, TResult> _postProcessingRunner;
    private readonly IRecurringJobService? _recurringJobService;

    public RecurringCommandRunner(
        PreProcessingRunner<TCommand, TResult> preProcessingRunner,
        ICommandHandler<TCommand, TResult> commandHandler,
        PostProcessingRunner<TCommand, TResult> postProcessingRunner,
        IRecurringJobService? recurringJobService = null)
    {
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _recurringJobService = recurringJobService;
    }

    public async Task ScheduleAsync(string recurringCommandId, TCommand command, string cronExpression)
    {
        if (_recurringJobService == null)
        {
            throw Error.Unexpected(
                "RecurringJobServiceNotRegistered",
                "RecurringJobService is not registered. Please register it in the DI container.");
        }

        // pre-checking
        await _preProcessingRunner.RunPreChecksAsync(command);

        // schedule recurring command
        await _recurringJobService.AddOrUpdateAsync(
            recurringCommandId,
            () => RunAsync(command),
            cronExpression);
    }

    public async Task RunAsync(TCommand command)
    {
        // pre-processing
        await _preProcessingRunner.RunPreProcessorsAsync(command);

        // processing
        var result = await _commandHandler.HandleAsync(command);

        // post-processing
        await _postProcessingRunner.RunAsync(command, result);
    }
}
