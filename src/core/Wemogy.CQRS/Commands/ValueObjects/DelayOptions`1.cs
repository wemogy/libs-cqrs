using System;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.ValueObjects;

public class DelayOptions<TCommand>
    where TCommand : ICommandBase
{
    public TimeSpan Delay { get; }
    public Func<TCommand, string>? SessionIdResolver { get; }

    public DelayOptions(
        TimeSpan delay,
        Func<TCommand, string>? sessionIdResolver = null)
    {
        Delay = delay;
        SessionIdResolver = sessionIdResolver;
    }
}
