using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;

public class TrackUserLoginCommandHandler : ICommandHandler<TrackUserLoginCommand>
{
    public static int CallCount { get; private set; }

    public Task HandleAsync(TrackUserLoginCommand command)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}
