using System;
using System.Threading.Tasks;
using FluentAssertions;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Queries.GetUser;
using Xunit;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.Queries;

public class GetEntityQueryTests : QueryTestsBase
{
    [Fact]
    public async Task GetEntityQuery_ShouldWork()
    {
        // Arrange
        var user = User.Faker.Generate();
        await UserRepository.CreateAsync(user);
        var getUserQuery = new GetUserQuery(user.Id, user.TenantId);

        // Act
        var userFromDb = await Queries.QueryAsync(getUserQuery);

        // Assert
        userFromDb.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetEntityQuery_ShouldThrowNotFoundExceptionFromClient()
    {
        // Arrange
        var user = User.Faker.Generate();
        await UserRepository.CreateAsync(user);
        var getUserQuery = new GetUserQuery(Guid.NewGuid(), user.TenantId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundErrorException>(() => Queries.QueryAsync(getUserQuery));
    }

    [Fact]
    public async Task GetEntityQuery_ShouldThrowNotFoundExceptionFromRepository()
    {
        // Arrange
        var user = User.Faker.Generate();
        user.Deleted = true;
        await UserRepository.CreateAsync(user);
        var getUserQuery = new GetUserQuery(user.Id, user.TenantId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundErrorException>(() => Queries.QueryAsync(getUserQuery));
    }

    [Fact]
    public async Task GetEntityQuery_ShouldThrowNotFoundExceptionFromFilter()
    {
        // Arrange
        var user = User.Faker.Generate();
        user.Firstname = "John";
        await UserRepository.CreateAsync(user);
        var getUserQuery = new GetUserQuery(user.Id, user.TenantId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundErrorException>(() => Queries.QueryAsync(getUserQuery));
    }
}
