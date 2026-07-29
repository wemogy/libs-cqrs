using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.AlwaysFailingSession;

public class AlwaysFailingSessionCommand : ICommand
{
    public string SessionId { get; }

    public AlwaysFailingSessionCommand(string sessionId)
    {
        SessionId = sessionId;
    }
}
