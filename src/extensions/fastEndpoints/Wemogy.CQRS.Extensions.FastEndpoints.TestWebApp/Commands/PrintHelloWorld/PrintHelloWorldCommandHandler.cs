using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.PrintHelloWorld;

public class PrintHelloWorldCommandHandler : ICommandHandler<PrintHelloWorldCommand>
{
    public Task HandleAsync(PrintHelloWorldCommand command)
    {
        Console.WriteLine("Hello World 👋");
        return Task.CompletedTask;
    }
}
