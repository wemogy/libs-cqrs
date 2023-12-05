using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class CommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand, TResult> _commandHandler;
    private readonly PostProcessingRunner<TCommand, TResult> _postProcessingRunner;

    public CommandRunner(
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand, TResult> commandHandler,
        PostProcessingRunner<TCommand, TResult> postProcessingRunner)
    {
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
    }

    public async Task<TResult> RunAsync(TCommand command)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Run");

        // pre-processing
        await _preProcessingRunner.RunAsync(command);

        // processing
        using var commandHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} CommandHandler");
        var result = await _commandHandler.HandleAsync(command);
        commandHandlerActivity?.Stop();

        // post-processing
        await _postProcessingRunner.RunAsync(command, result);

        return result;
    }
}
