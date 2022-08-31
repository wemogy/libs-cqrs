using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class CommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly PreProcessingRunner<TCommand, TResult> _preProcessingRunner;
    private readonly ICommandHandler<TCommand, TResult> _commandHandler;
    private readonly PostProcessingRunner<TCommand, TResult> _postProcessingRunner;

    public CommandRunner(
        PreProcessingRunner<TCommand, TResult> preProcessingRunner,
        ICommandHandler<TCommand, TResult> commandHandler,
        PostProcessingRunner<TCommand, TResult> postProcessingRunner)
    {
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _preProcessingRunner = preProcessingRunner;
    }

    public async Task<TResult> RunAsync(TCommand command)
    {
        // pre-processing
        await _preProcessingRunner.RunAsync(command);

        // processing
        var result = await _commandHandler.HandleAsync(command);

        // post-processing
        await _postProcessingRunner.RunAsync(command, result);

        return result;
    }
}
