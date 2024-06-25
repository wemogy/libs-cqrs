using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.AssemblyA.Commands.PrintHelloAssemblyA;

public class PrintHelloAssemblyACommandHandler : ICommandHandler<PrintHelloAssemblyACommand>
{
    public static int CallCount { get; private set; }

    public Task HandleAsync(PrintHelloAssemblyACommand command)
    {
        Console.WriteLine("Hello from Assembly A!");
        CallCount++;
        return Task.CompletedTask;
    }
}
