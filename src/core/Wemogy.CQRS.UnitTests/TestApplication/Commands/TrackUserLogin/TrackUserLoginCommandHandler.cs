using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;

public class TrackUserLoginCommandHandler : ICommandHandler<TrackUserLoginCommand>
{
    public static Dictionary<string, int> ExecutedCount { get; } = new ();

    public Task HandleAsync(TrackUserLoginCommand command)
    {
        if (ExecutedCount.TryGetValue(command.UserId, out var count))
        {
            ExecutedCount[command.UserId] = count + 1;
        }
        else
        {
            ExecutedCount[command.UserId] = 1;
        }

        return Task.CompletedTask;
    }
}
