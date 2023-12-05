using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class VoidPostProcessingRunner<TCommand>
    where TCommand : ICommand
{
    private readonly List<ICommandPostProcessor<TCommand>> _postProcessors;

    public VoidPostProcessingRunner(List<ICommandPostProcessor<TCommand>> postProcessors)
    {
        _postProcessors = postProcessors;
    }

    public async Task RunAsync(TCommand command)
    {
        foreach (var postProcessor in _postProcessors)
        {
            using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} PostProcessor");
            await postProcessor.ProcessAsync(command);
        }
    }
}
