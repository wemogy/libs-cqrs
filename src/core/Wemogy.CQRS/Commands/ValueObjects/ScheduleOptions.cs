using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.ValueObjects;

public class ScheduleOptions<TCommand>
    where TCommand : ICommandBase
{
    public DelayOptions<TCommand>? DelayOptions { get; }

    public ThrottleOptions<TCommand>? ThrottleOptions { get; }

    public ScheduleOptions(DelayOptions<TCommand> delayOptions)
    {
        DelayOptions = delayOptions;
    }

    public ScheduleOptions(ThrottleOptions<TCommand> throttleOptions)
    {
        ThrottleOptions = throttleOptions;
    }
}
