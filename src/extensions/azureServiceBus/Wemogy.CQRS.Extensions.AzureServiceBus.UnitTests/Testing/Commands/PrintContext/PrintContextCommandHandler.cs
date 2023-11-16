using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;
using Void = Wemogy.CQRS.Commands.Structs.Void;

namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests.Testing.Commands.PrintContext;

public class PrintContextCommandHandler : ICommandHandler<PrintContextCommand>
{
    public static int ExecutedCount { get; private set; }

    public static void Reset()
    {
        ExecutedCount = 0;
    }

    private readonly TestContext _myTestingContext;

    public PrintContextCommandHandler(TestContext myTestingContext)
    {
        _myTestingContext = myTestingContext;
    }

    public Task HandleAsync(PrintContextCommand command)
    {
        ExecutedCount++;
        Console.WriteLine($"Context: {_myTestingContext.TenantId}");
        return Task.FromResult(Void.Value);
    }
}
