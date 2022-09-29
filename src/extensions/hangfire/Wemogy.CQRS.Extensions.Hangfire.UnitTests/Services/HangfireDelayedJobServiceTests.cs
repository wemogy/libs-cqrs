using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication;
using Xunit;

namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests.Services;

public class HangfireDelayedJobServiceTests
{
    [Fact]
    public async Task DeleteAsync_ShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddTestApplication()
            .AddHangfire();

        serviceCollection.AddHangfire(config =>
        {
            config.UseInMemoryStorage();
        });

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

    public Task DummyAction()
    {
        // No-op
        return Task.CompletedTask;
    }
}
