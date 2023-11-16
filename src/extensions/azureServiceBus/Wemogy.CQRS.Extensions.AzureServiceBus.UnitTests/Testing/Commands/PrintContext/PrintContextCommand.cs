using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;

public class PrintContextCommand : ICommand
{
    public string? TenantId { get; set; }
}
