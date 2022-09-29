using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.Hangfire.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.Extensions.Hangfire.UnitTests.Testing.Models;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests.Services;

public class HangfireDelayedJobServiceTests
{
    public HangfireDelayedJobServiceTests()
    {
        PrintContextCommandHandler.Reset();
    }

    [Fact]
    public async Task DeleteAsync_ShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddTestApplication()
            .AddHangfire(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scheduledCommandService = serviceProvider.GetRequiredService<IScheduledCommandService>();
        var storageConnection = JobStorage.Current.GetConnection();

        // Act
        var jobId = await scheduledCommandService.ScheduleAsync(() => DummyAction(), TimeSpan.FromMinutes(1));
        var jobDataAfterScheduling = storageConnection.GetJobData(jobId);
        await scheduledCommandService.DeleteAsync(jobId);
        var jobDataAfterDelete = storageConnection.GetJobData(jobId);

        // Assert
        jobDataAfterScheduling.Should().NotBeNull();
        jobDataAfterScheduling.State.Should().Be("Scheduled");
        jobDataAfterDelete.Should().NotBeNull();
        jobDataAfterDelete.State.Should().Be("Deleted");
    }

    [Fact]
    public async Task ScheduleAsync_ShouldSaveDependencies()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddCQRS()
            .AddHangfire(serviceCollection);
        serviceCollection.AddSingleton<MyTestingContext>(_ => new MyTestingContext()
        {
            Name = "singleton"
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commands = serviceProvider.GetRequiredService<ICommands>();
        var command = new PrintContextCommand();

        // Act
        await commands.ScheduleAsync(command, TimeSpan.FromSeconds(1));

        await Task.Delay(3000);

        // Assert
        PrintContextCommandHandler.ExecutedCount.Should().Be(1);
    }

    public Task DummyAction()
    {
        // No-op
        return Task.CompletedTask;
    }
}
