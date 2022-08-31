using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class PostProcessingRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly List<ICommandPostProcessor<TCommand, TResult>> _postProcessors;

    public PostProcessingRunner(List<ICommandPostProcessor<TCommand, TResult>> postProcessors)
    {
        _postProcessors = postProcessors;
    }

    public async Task RunAsync(TCommand command, TResult result)
    {
        foreach (var postProcessor in _postProcessors)
        {
            await postProcessor.ProcessAsync(command, result);
        }
    }
}
