using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.AssemblyA.Commands;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserActivity;

public class TrackUserActivityCommandHandler : ICommandHandler<TrackUserActivityCommand>
{
    public static int CalledCount { get; private set; }
    public Task HandleAsync(TrackUserActivityCommand command)
    {
        CalledCount++;
        return Task.CompletedTask;
    }
}
