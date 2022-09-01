using Wemogy.CQRS.Extensions.Database.Queries.GetEntity;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Repositories;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQueryHandler : GetEntityQueryHandler<GetUserQuery, IUserRepository, User>
{
    public GetUserQueryHandler(IUserRepository databaseRepository)
        : base(databaseRepository)
    {
    }
}
