using System;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;

public class PrintContextCommand : ICommand
{
    public string Id { get; set; }

    public string? TenantId { get; set; }

    public PrintContextCommand()
    {
        Id = Guid.NewGuid().ToString();
    }
}
