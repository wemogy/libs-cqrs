using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;
using Void = Wemogy.CQRS.Commands.Structs.Void;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;

public class PrintContextCommandHandler : ICommandHandler<PrintContextCommand>
{
    public static Dictionary<string, int> ExecutedCount { get; } = new Dictionary<string, int>();

    private readonly TestContext _myTestingContext;

    public PrintContextCommandHandler(TestContext myTestingContext)
    {
        _myTestingContext = myTestingContext;
    }

    public Task HandleAsync(PrintContextCommand command)
    {
        if (ExecutedCount.TryGetValue(command.Id, out var count))
        {
            ExecutedCount[command.Id] = count + 1;
        }
        else
        {
            ExecutedCount[command.Id] = 1;
        }

        Console.WriteLine($"Context: {_myTestingContext.TenantId}");
        return Task.FromResult(Void.Value);
    }
}
