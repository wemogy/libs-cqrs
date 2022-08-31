using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Structs;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.RecreateUserIndex;

public class RecreateUserIndexCommandHandler : ICommandHandler<RecreateUserIndexCommand>
{
    public Task<Void> HandleAsync(RecreateUserIndexCommand command)
    {
        throw new System.NotImplementedException();
    }
}
