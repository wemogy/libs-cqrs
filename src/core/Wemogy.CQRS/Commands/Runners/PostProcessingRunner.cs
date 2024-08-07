using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class PostProcessingRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IEnumerable<ICommandPostProcessor<TCommand, TResult>> _postProcessors;

    public PostProcessingRunner(IEnumerable<ICommandPostProcessor<TCommand, TResult>> postProcessors)
    {
        _postProcessors = postProcessors;
    }

    public async Task RunAsync(TCommand command, TResult result)
    {
        foreach (var postProcessor in _postProcessors)
        {
            using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} PostProcessor");
            await postProcessor.ProcessAsync(command, result);
        }
    }
}
