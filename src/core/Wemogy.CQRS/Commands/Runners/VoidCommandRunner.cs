using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class VoidCommandRunner<TCommand>
    where TCommand : ICommand
{
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly VoidPostProcessingRunner<TCommand> _postProcessingRunner;

    public VoidCommandRunner(
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand> commandHandler,
        VoidPostProcessingRunner<TCommand> postProcessingRunner)
    {
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
    }

    public async Task RunAsync(TCommand command)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Run");

        // pre-processing
        await _preProcessingRunner.RunAsync(command);

        // processing
        using var commandHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} CommandHandler");
        await _commandHandler.HandleAsync(command);
        commandHandlerActivity?.Stop();

        // post-processing
        await _postProcessingRunner.RunAsync(command);
    }
}
