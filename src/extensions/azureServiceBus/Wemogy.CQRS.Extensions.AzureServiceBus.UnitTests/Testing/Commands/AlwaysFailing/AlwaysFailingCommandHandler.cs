using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.AlwaysFailing;

public class AlwaysFailingCommandHandler : ICommandHandler<AlwaysFailingCommand>
{
    public const string ExceptionMessage = "AlwaysFailing command exception";

    public Task HandleAsync(AlwaysFailingCommand command)
    {
        throw new InvalidOperationException(ExceptionMessage);
    }
}
