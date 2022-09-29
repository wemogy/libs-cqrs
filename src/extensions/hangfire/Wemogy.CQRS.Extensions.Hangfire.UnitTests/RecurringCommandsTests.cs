using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;
using Xunit;

namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests;

public class RecurringCommandsTests
{
    public RecurringCommandsTests()
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
        serviceCollection.AddHangfireCQRSExtension();

        serviceCollection.AddHangfire(config =>
        {
            config.UseInMemoryStorage();
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var recurringCommandRunner = serviceProvider.GetRequiredService<RecurringCommandRunner<CreateUserCommand, User>>();
        var createUserCommand = new CreateUserCommand
        {
            Firstname = "John"
        };
        var recurringCommandId = Guid.NewGuid().ToString();
        var cron = Cron.Hourly(); // every hour

        // Act
        await recurringCommandRunner.ScheduleAsync(recurringCommandId, createUserCommand, cron);

        // Assert
        CreateUserCommandValidator.CalledCount.Should().Be(1);
        CreateUserCommandAuthorization.CalledCount.Should().Be(1);
        CreateUserCommandPreProcessor.CalledCount.Should().Be(0);
        CreateUserCommandHandler.CalledCount.Should().Be(0);
        CreateUserCommandPostProcessor.CalledCount.Should().Be(0);
        JobStorage.Current.GetConnection().GetRecurringJobs().Should().HaveCount(1);

        CreateUserCommandValidator.Reset();
        CreateUserCommandAuthorization.Reset();
        await recurringCommandRunner.RunAsync(createUserCommand);
        CreateUserCommandValidator.CalledCount.Should().Be(0);
        CreateUserCommandAuthorization.CalledCount.Should().Be(0);
        CreateUserCommandPreProcessor.CalledCount.Should().Be(1);
        CreateUserCommandHandler.CalledCount.Should().Be(1);
        CreateUserCommandPostProcessor.CalledCount.Should().Be(1);
    }
}
