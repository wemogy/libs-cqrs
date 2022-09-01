using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Repositories;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.Infrastructure.Database.InMemory.Setup;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.Queries;

public abstract class QueryTestsBase
{
    protected IUserRepository UserRepository { get; }
    protected IQueries Queries { get; }
    protected QueryTestsBase()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCqrs();

        serviceCollection
            .AddInMemoryDatabaseClient()
            .AddRepository<IUserRepository>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        UserRepository = serviceProvider.GetRequiredService<IUserRepository>();
        Queries = serviceProvider.GetRequiredService<IQueries>();
    }
}
