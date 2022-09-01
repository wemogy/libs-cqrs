using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;
using Wemogy.Infrastructure.Database.Core.Abstractions;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Repositories.ReadFilters;

public class GeneralUserReadFilter : IDatabaseRepositoryReadFilter<User>
{
    public Task<Expression<Func<User, bool>>> FilterAsync()
    {
        return Task.FromResult((Expression<Func<User, bool>>)(x => x.Firstname != "John"));
    }
}
