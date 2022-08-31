using System;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Runners;

public class DelayedCommandRunner<TCommand>
{
    public Task ScheduleDelayedAsync(TCommand command)
    {
        // ToDo: Call only validation & authorization
        // ToDo: enqueue command
        // ToDo: DelayedCommandHandler should call pre-processors, command handler, post-processors
        throw new NotImplementedException();
    }
}
