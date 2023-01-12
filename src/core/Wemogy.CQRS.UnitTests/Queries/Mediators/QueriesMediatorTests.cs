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
        var firstName = Guid.NewGuid().ToString();
        var getUserQuery = new GetUserQuery(firstName);

        // Act
        var user = await queries.QueryAsync(getUserQuery);

        // Assert
        user.Firstname.Should().Be(firstName);
    }

    [Fact]
    public async Task QueryAsync_InvalidQueryParam_ShouldThrows()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var queries = serviceProvider.GetRequiredService<IQueries>();
        var firstName = Guid.NewGuid().ToString()[..9];
        var getUserQuery = new GetUserQuery(firstName);

        // Act
        var exception = await Record.ExceptionAsync(() => queries.QueryAsync(getUserQuery));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<FluentValidation.ValidationException>();
        exception.Message.Should().Contain(nameof(GetUserQuery.FirstName));
    }
}
