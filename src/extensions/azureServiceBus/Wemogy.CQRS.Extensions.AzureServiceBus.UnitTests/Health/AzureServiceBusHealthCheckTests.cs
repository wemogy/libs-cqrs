using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wemogy.Configuration;
using Wemogy.CQRS.Extensions.AzureServiceBus.Health;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Health;

[Collection("AzureServiceBus")]
public class AzureServiceBusHealthCheckTests
{
    private const string QueueName = "unit-testing-queue-1";
    private readonly string _azureServiceBusConnectionString;

    public AzureServiceBusHealthCheckTests()
    {
        var configuration = ConfigurationFactory.BuildConfiguration("Development");
        _azureServiceBusConnectionString = configuration["AzureServiceBusConnectionString"] !;
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnHealthyIfAzureServiceBusIsAlive()
    {
        // Arrange
        var healthCheckService = new AzureServiceBusHealthCheck(_azureServiceBusConnectionString, QueueName);

        // Act
        var healthCheckResult = await healthCheckService.CheckHealthAsync(
            new HealthCheckContext());

        // Assert
        healthCheckResult.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldThrowIfCancellationWasRequested()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var healthCheckService = new AzureServiceBusHealthCheck(_azureServiceBusConnectionString, QueueName);

        // Act
        cancellationTokenSource.Cancel();
        var exception = await Record.ExceptionAsync(() => healthCheckService.CheckHealthAsync(
            new HealthCheckContext(),
            cancellationTokenSource.Token));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<TaskCanceledException>();
    }
}
