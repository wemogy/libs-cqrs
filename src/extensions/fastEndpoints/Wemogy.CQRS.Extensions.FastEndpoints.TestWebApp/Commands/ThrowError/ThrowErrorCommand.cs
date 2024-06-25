using Wemogy.Core.Errors.Enums;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.ThrowError;

public class ThrowErrorCommand : ICommand
{
    public ErrorType ErrorType { get; }

    public ThrowErrorCommand(ErrorType errorType)
    {
        ErrorType = errorType;
    }
}
