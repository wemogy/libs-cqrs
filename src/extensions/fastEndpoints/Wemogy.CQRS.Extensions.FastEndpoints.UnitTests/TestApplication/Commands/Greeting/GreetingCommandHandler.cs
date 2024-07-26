using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.Greeting;

public class GreetingCommandHandler : ICommandHandler<GreetingCommand, string>
{
    public Task<string> HandleAsync(GreetingCommand command)
    {
        return Task.FromResult($"Hello, {command.Name}!");
    }
}
