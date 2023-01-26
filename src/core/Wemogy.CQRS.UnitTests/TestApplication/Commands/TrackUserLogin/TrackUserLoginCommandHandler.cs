using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;

public class TrackUserLoginCommandHandler : ICommandHandler<TrackUserLoginCommand>
{
    public Task HandleAsync(TrackUserLoginCommand command)
    {
        return Task.CompletedTask;
    }
}
