using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>
{
    public Task<User> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = new User()
        {
            Firstname = query.FirstName
        };
        return Task.FromResult(user);
    }
}
