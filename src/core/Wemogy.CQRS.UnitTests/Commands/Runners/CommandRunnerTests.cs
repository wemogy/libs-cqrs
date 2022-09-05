using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.Runners;

public class CommandRunnerTests
{
    public CommandRunnerTests()
    {
        // reset static counters
        CreateUserCommandValidator.Reset();
        CreateUserCommandAuthorization.Reset();
        CreateUserCommandPreProcessor.Reset();
        CreateUserCommandHandler.Reset();
        CreateUserCommandPostProcessor.Reset();
    }

    [Fact]
    public async Task RunAsync_ShouldCallAllHandlers()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandRunnerBase = serviceProvider.GetRequiredService<CommandRunner<CreateUserCommand, User>>();
        var createUserCommand = new CreateUserCommand
        {
            Firstname = "John"
        };

        // Act
        var res = await commandRunnerBase.RunAsync(createUserCommand);

        // Assert
        CreateUserCommandValidator.CalledCount.Should().Be(1);
        CreateUserCommandAuthorization.CalledCount.Should().Be(1);
        CreateUserCommandPreProcessor.CalledCount.Should().Be(1);
        CreateUserCommandHandler.CalledCount.Should().Be(1);
        CreateUserCommandPostProcessor.CalledCount.Should().Be(1);
        CreateUserCommandPostProcessor.PassedResult.Should().BeSameAs(res);
    }

    [Fact]
    public async Task RunAsync_ShouldThrowValidationExceptionFromValidator()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandRunnerBase = serviceProvider.GetRequiredService<CommandRunner<CreateUserCommand, User>>();
        var createUserCommand = new CreateUserCommand
        {
            Firstname = string.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => commandRunnerBase.RunAsync(createUserCommand));
    }

    [Fact]
    public async Task RunAsync_ShouldThrowAuthorizationErrorExceptionFromCommandAuthorization()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        serviceCollection.AddSingleton(new TestContext
        {
            TenantId = TestContext.TenantAId
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandRunnerBase = serviceProvider.GetRequiredService<CommandRunner<CreateUserCommand, User>>();
        var createUserCommand = new CreateUserCommand
        {
            Firstname = "John"
        };

        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationErrorException>(() => commandRunnerBase.RunAsync(createUserCommand));
    }
}
