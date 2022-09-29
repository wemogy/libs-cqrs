using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Xunit;

namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests.Services;

public class HangfireDelayedJobServiceTests
{
    [Fact]
    public async Task DeleteAsync_ShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddHangfireCQRSExtension();

        serviceCollection.AddHangfire(config =>
        {
            config.UseInMemoryStorage();
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var delayedJobService = serviceProvider.GetRequiredService<IDelayedJobService>();
        var storageConnection = JobStorage.Current.GetConnection();

        // Act
        var jobId = await delayedJobService.ScheduleAsync(() => DummyAction(), TimeSpan.FromMinutes(1));
        var jobDataAfterScheduling = storageConnection.GetJobData(jobId);
        await delayedJobService.CancelAsync(jobId);
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
