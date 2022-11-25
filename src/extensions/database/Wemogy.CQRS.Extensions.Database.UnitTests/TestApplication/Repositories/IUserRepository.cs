using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Repositories.ReadFilters;
using Wemogy.Infrastructure.Database.Core.Abstractions;
using Wemogy.Infrastructure.Database.Core.Attributes;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Repositories;

[RepositoryOptions(enableSoftDelete: true)]
[RepositoryReadFilter(typeof(GeneralUserReadFilter))]
public interface IUserRepository : IDatabaseRepository<User>
{
}
