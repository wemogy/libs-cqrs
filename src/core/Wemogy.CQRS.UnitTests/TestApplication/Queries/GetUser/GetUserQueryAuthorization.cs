using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQueryAuthorization : IQueryAuthorization<GetUserQuery>
{
    public Task AuthorizeAsync(GetUserQuery query)
    {
        if (query.FirstName == "ThrowExceptionInGetUserQueryAuthorization")
        {
            throw Error.Authorization(
                "Unauthorized",
                "You are not allowed to access this resource.");
        }

        return Task.CompletedTask;
    }
}
