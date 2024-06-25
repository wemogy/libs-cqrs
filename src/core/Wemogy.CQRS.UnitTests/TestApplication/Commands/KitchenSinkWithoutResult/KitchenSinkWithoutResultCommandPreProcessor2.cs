using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.KitchenSinkWithoutResult;

public class KitchenSinkWithoutResultCommandPreProcessor2 : ICommandPreProcessor<KitchenSinkWithoutResultCommand>
{
    public int CalledCount { get; private set; }
    public Task ProcessAsync(KitchenSinkWithoutResultCommand command)
    {
        CalledCount++;
        return Task.CompletedTask;
    }
}
