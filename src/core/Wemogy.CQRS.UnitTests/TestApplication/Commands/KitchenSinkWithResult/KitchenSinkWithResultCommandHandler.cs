using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.KitchenSinkWithResult;

public class KitchenSinkWithResultCommandHandler : ICommandHandler<KitchenSinkWithResultCommand, int>
{
    public int CalledCount { get; private set; }

    public Task<int> HandleAsync(KitchenSinkWithResultCommand command)
    {
        CalledCount++;
        return Task.FromResult(42);
    }
}
