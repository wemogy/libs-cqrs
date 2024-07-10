using Wemogy.Core.Errors;
using Wemogy.Core.Errors.Enums;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.ThrowError;

public class ThrowErrorCommandHandler : ICommandHandler<ThrowErrorCommand>
{
    private const string DummyErrorCode = "DummyErrorCode";
    private const string DummyErrorDescription = "DummyErrorDescription";

    public Task HandleAsync(ThrowErrorCommand command)
    {
        switch (command.ErrorType)
        {
            case ErrorType.Failure:
                throw Error.Failure(DummyErrorCode, DummyErrorDescription);
            case ErrorType.Unexpected:
                throw Error.Unexpected(DummyErrorCode, DummyErrorDescription);
            case ErrorType.Validation:
                throw Error.Validation(DummyErrorCode, DummyErrorDescription);
            case ErrorType.Conflict:
                throw Error.Conflict(DummyErrorCode, DummyErrorDescription);
            case ErrorType.NotFound:
                throw Error.NotFound(DummyErrorCode, DummyErrorDescription);
            case ErrorType.Authorization:
                throw Error.Authorization(DummyErrorCode, DummyErrorDescription);
            case ErrorType.PreconditionFailed:
                throw Error.PreconditionFailed(DummyErrorCode, DummyErrorDescription);
        }

        throw Error.Unexpected(
            "ErrorTypeNotSupported",
            $"The ErrorType {command.ErrorType} is not supported!");
    }
}
