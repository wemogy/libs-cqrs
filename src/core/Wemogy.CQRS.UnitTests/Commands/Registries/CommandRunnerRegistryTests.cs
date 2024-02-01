using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Registries;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.Registries;

public class CommandRunnerRegistryTests
{
    [Fact]
    public void ConcurrencyShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandRunnerRegistry = new CommandRunnerRegistry();
        var createUserCommand = new CreateUserCommand
        {
            Firstname = "John"
        };

        Parallel.For(
            1,
            100,
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = 100
            },
            _ =>
            {
                commandRunnerRegistry.ExecuteCommandRunnerAsync(serviceProvider, createUserCommand);
            });
    }
}
