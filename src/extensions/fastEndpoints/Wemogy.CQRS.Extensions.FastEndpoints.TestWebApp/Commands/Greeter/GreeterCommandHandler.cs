using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.Greeter;

public class GreeterCommandHandler : ICommandHandler<GreeterCommand, string>
{
    public Task<string> HandleAsync(GreeterCommand command)
    {
        return Task.FromResult($"Hello, {command.Name}!");
    }
}
