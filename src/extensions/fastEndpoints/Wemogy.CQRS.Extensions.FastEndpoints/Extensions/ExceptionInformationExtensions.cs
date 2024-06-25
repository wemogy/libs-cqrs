using Wemogy.Core.Json.ExceptionInformation;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

public static class ExceptionInformationExtensions
{
    public static Exception ToException(this ExceptionInformation exceptionInformation)
    {
        throw new NotImplementedException();
    }

    private static Type GetExceptionType(this ExceptionInformation exceptionInformation)
    {
        return Type.GetType(exceptionInformation.ExceptionType) ?? typeof(Exception);
    }
}
