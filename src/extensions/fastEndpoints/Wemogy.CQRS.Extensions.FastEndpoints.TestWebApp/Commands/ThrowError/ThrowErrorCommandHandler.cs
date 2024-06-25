using Wemogy.Core.Errors.Enums;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.ThrowError;

public class ThrowErrorCommandHandler : ICommandHandler<ThrowErrorCommand>
{
    public Task HandleAsync(ThrowErrorCommand command)
    {
        throw new CustomErrorException(
            command.ErrorType,
            "CustomError",
            "This is a custom error",
            null);
    }

    class CustomErrorException : ErrorException
    {
        public CustomErrorException(ErrorType errorType, string code, string description, Exception? innerException)
            : base(errorType, code, description, innerException)
        {
        }
    }
}
