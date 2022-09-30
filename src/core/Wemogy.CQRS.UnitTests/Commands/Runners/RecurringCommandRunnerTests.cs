using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.Runners;

[Collection("Sequential")]
public class RecurringCommandRunnerTests
{
    [Fact]
    public async Task ScheduleAsync_ShouldThrowIfNoIRecurringJobServiceIsDefined()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var recurringCommandRunner = serviceProvider.GetRequiredService<RecurringCommandRunner<CreateUserCommand, User>>();
        var recurringCommandId = "recurringCommandId";
        var cron = "0 4 * * *";
        var createUserCommand = new CreateUserCommand
        {
            Firstname = string.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnexpectedErrorException>(() => recurringCommandRunner.ScheduleAsync(
            recurringCommandId,
            createUserCommand,
            cron));
    }
}
