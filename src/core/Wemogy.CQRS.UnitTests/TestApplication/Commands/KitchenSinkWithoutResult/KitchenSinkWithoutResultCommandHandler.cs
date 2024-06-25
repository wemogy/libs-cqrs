using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.KitchenSinkWithoutResult;

public class KitchenSinkWithoutResultCommandHandler : ICommandHandler<KitchenSinkWithoutResultCommand>
{
    public int CalledCount { get; private set; }

    public Task HandleAsync(KitchenSinkWithoutResultCommand command)
    {
        CalledCount++;
        return Task.CompletedTask;
    }
}
