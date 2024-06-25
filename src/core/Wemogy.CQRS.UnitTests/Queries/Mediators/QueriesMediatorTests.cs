using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.AssemblyA;
using Wemogy.CQRS.UnitTests.AssemblyA.Queries;
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
    public async Task QueryAsync_Authorization_ShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var queries = serviceProvider.GetRequiredService<IQueries>();
        var firstName = "ThrowExceptionInGetUserQueryAuthorization";
        var getUserQuery = new GetUserQuery(firstName);

        // Act
        var exception = await Record.ExceptionAsync(() => queries.QueryAsync(getUserQuery));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<AuthorizationErrorException>();
    }

    [Fact]
    public async Task QueryAsync_InvalidQueryParam_ShouldThrows()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var queries = serviceProvider.GetRequiredService<IQueries>();
        var firstName = "abc";
        var getUserQuery = new GetUserQuery(firstName);

        // Act
        var exception = await Record.ExceptionAsync(() => queries.QueryAsync(getUserQuery));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<FluentValidation.ValidationException>()
            .Which.Message.Should().Contain(nameof(GetUserQuery.FirstName));
    }

    [Fact]
    public async Task QueryAsync_ShouldSupportMultipleAssemblyDefinitions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQRS();
        serviceCollection.AddAssemblyA();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var queries = serviceProvider.GetRequiredService<IQueries>();
        var getUserQuery = new GetUserActivityQuery();

        // Act
        var userActivity = await queries.QueryAsync(getUserQuery);

        // Assert
        userActivity.Should().Be(1);
    }
}
