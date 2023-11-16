using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.Hangfire.UnitTests.Testing.Commands.PrintContext;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests.Services;

public class HangfireDelayedJobServiceTests
{
    public HangfireDelayedJobServiceTests()
    {
        PrintContextCommandHandler.Reset();
    }

    [Fact(Skip = "Skip")]
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
        var scheduledCommandRunner = serviceProvider.GetRequiredService<IScheduledCommandRunner<PrintContextCommand>>();
        var scheduledCommand = new ScheduledCommand<PrintContextCommand>(
            new List<ScheduledCommandDependency>(),
            new PrintContextCommand());

        // Act
        var jobId = await scheduledCommandService.ScheduleAsync(
            scheduledCommandRunner,
            scheduledCommand,
            TimeSpan.FromMinutes(1));
        var jobDataAfterScheduling = storageConnection.GetJobData(jobId);
        await scheduledCommandService.DeleteAsync<PrintContextCommand>(jobId);
        var jobDataAfterDelete = storageConnection.GetJobData(jobId);

        // Assert
        jobDataAfterScheduling.Should().NotBeNull();
        jobDataAfterScheduling.State.Should().Be("Scheduled");
        jobDataAfterDelete.Should().NotBeNull();
        jobDataAfterDelete.State.Should().Be("Deleted");
    }
}
