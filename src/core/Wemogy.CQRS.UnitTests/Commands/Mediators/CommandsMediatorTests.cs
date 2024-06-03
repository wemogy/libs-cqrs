using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.AssemblyA.Commands;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserActivity;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.Mediators;

[Collection("Sequential")]
public class CommandsMediatorTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnTheCorrectResult()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandsMediator = serviceProvider.GetRequiredService<ICommands>();
        var createUserCommand = new CreateUserCommand
        {
            Firstname = "John"
        };

        // Act
        var res = await commandsMediator.RunAsync(createUserCommand);

        // Assert
        res.Firstname.Should().Be(createUserCommand.Firstname);
    }

    [Fact]
    public async Task RunAsync_ShouldReturnSupportVoidCommands()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandsMediator = serviceProvider.GetRequiredService<ICommands>();
        var createUserCommand = new TrackUserLoginCommand(string.Empty);

        // Act
        var exception = await Record.ExceptionAsync(() => commandsMediator.RunAsync(createUserCommand));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_ShouldSupportMultipleAssemblyDefinitions()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandsMediator = serviceProvider.GetRequiredService<ICommands>();
        var createUserCommand = new TrackUserActivityCommand();

        // Act
        await commandsMediator.RunAsync(createUserCommand);

        // Assert
        TrackUserActivityCommandHandler.CalledCount.Should().Be(1);
    }
}
