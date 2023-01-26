using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.RecreateUserIndex;

public class RecreateUserIndexCommandHandler : ICommandHandler<RecreateUserIndexCommand>
{
    public Task HandleAsync(RecreateUserIndexCommand command)
    {
        throw new System.NotImplementedException();
    }
}
