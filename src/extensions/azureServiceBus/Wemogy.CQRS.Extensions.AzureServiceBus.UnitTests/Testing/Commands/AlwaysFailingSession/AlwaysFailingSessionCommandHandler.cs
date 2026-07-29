using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.AlwaysFailingSession;

public class AlwaysFailingSessionCommandHandler : ICommandHandler<AlwaysFailingSessionCommand>
{
    public const string ExceptionMessage = "AlwaysFailingSession command exception";

    public Task HandleAsync(AlwaysFailingSessionCommand command)
    {
        throw new InvalidOperationException(ExceptionMessage);
    }
}
