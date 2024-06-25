using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.AssemblyB.Commands.PrintHelloAssemblyB;

public class PrintHelloAssemblyBCommandHandler : ICommandHandler<PrintHelloAssemblyBCommand>
{
    public static int CallCount { get; private set; }

    public Task HandleAsync(PrintHelloAssemblyBCommand command)
    {
        Console.WriteLine("Hello from Assembly B!");
        CallCount++;
        return Task.CompletedTask;
    }
}
