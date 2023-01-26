using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQuery : IQuery<User>
{
    public string FirstName { get; }

    public GetUserQuery(string firstName)
    {
        FirstName = firstName;
    }
}
