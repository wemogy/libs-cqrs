using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>
{
    public Task<User> HandleAsync(GetUserQuery query)
    {
        var user = new User()
        {
            Firstname = query.Id.ToString(),
        };
        return Task.FromResult(user);
    }
}
