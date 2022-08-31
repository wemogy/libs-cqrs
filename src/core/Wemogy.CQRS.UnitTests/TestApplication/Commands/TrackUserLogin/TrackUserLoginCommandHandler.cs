using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Structs;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;

public class TrackUserLoginCommandHandler : ICommandHandler<TrackUserLoginCommand>
{
    public Task<Void> HandleAsync(TrackUserLoginCommand command)
    {
        throw new System.NotImplementedException();
    }
}
