using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.Mediators;

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
}
