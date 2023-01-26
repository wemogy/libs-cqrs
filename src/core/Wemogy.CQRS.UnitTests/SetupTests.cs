using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;
using Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;
using Xunit;

namespace Wemogy.CQRS.UnitTests;

[Collection("Sequential")]
public class SetupTests
{
    [Fact]
    public void SetupShould_WorkCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddTestApplication();

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Commands
        serviceProvider.GetRequiredService<List<ICommandValidator<CreateUserCommand>>>().Should().HaveCount(1);
        serviceProvider.GetRequiredService<List<ICommandAuthorization<CreateUserCommand>>>().Should().HaveCount(1);
        serviceProvider.GetRequiredService<List<ICommandPreProcessor<CreateUserCommand>>>().Should().HaveCount(1);
        serviceProvider.GetRequiredService<List<ICommandPostProcessor<CreateUserCommand, User>>>().Should()
            .HaveCount(1);
        serviceProvider.GetService<ICommandHandler<CreateUserCommand, User>>().Should().NotBeNull();
        serviceProvider.GetService<PreProcessingRunner<CreateUserCommand>>().Should().NotBeNull();
        serviceProvider.GetService<CommandRunner<CreateUserCommand, User>>().Should().NotBeNull();
        serviceProvider.GetService<PostProcessingRunner<CreateUserCommand, User>>().Should().NotBeNull();
        serviceProvider.GetService<ICommands>().Should().NotBeNull();

        // Queries
        serviceProvider.GetRequiredService<List<IQueryValidator<GetUserQuery>>>().Should().HaveCount(1);
        serviceProvider.GetService<IQueries>().Should().NotBeNull();
    }
}
