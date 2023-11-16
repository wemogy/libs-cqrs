using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintHelloWorld;

public class PrintHelloWorldCommandHandler : ICommandHandler<PrintHelloWorld>
{
    public Task HandleAsync(PrintHelloWorld command)
    {
        Console.WriteLine("Hello World!");
        return Task.CompletedTask;
    }
}
