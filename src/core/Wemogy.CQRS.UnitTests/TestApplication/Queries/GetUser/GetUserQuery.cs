using System;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQuery : IQuery<User>
{
    public Guid Id { get; }

    public GetUserQuery(Guid id)
    {
        Id = id;
    }
}
