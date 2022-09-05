using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Queries.Mediators;

[Collection("Sequential")]
public class QueriesMediatorTests
{
    [Fact]
    public async Task QueryAsync_ShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var queries = serviceProvider.GetRequiredService<IQueries>();
        var id = Guid.NewGuid();
        var getUserQuery = new GetUserQuery(id);

        // Act
        var user = await queries.QueryAsync(getUserQuery);

        // Assert
        user.Firstname.Should().Be(id.ToString());
    }
}
