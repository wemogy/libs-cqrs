using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintSessionId;

public class PrintSessionIdCommand : ICommand
{
    public string SessionId { get; }

    public string TestRunId { get; }

    public PrintSessionIdCommand(string sessionId, string testRunId)
    {
        SessionId = sessionId;
        TestRunId = testRunId;
    }
}
