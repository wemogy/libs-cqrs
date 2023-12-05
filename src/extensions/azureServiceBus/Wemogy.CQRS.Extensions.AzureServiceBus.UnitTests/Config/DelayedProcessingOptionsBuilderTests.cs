using FluentAssertions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Config;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Config;

public class DelayedProcessingOptionsBuilderTests
{
    [Fact]
    public void Default_ShouldBuildDefaultOptions()
    {
        // Arrange
        var builder = new DelayedProcessingOptionsBuilder();
        var defaultOptions = new DelayedProcessingOptions();

        // Act
        var result = builder.Build();

        // Assert
        result.Should().BeEquivalentTo(defaultOptions);
    }

    [Fact]
    public void WithQueueName_ShouldSetQueueName()
    {
        // Arrange
        var builder = new DelayedProcessingOptionsBuilder();
        var queueName = "TestQueue";

        // Act
        var result = builder.WithQueueName(queueName).Build();

        // Assert
        result.QueueName.Should().Be(queueName);
    }

    [Fact]
    public void WithSessionSupport_ShouldSetSessionSupport()
    {
        // Arrange
        var builder = new DelayedProcessingOptionsBuilder();

        // Act
        var result = builder.WithSessionSupport().Build();

        // Assert
        result.IsSessionSupported.Should().BeTrue();
    }

    [Fact]
    public void Build_ShouldReturnDelayedProcessingOptions()
    {
        // Arrange
        var builder = new DelayedProcessingOptionsBuilder();

        // Act
        var result = builder.Build();

        // Assert
        result.Should().BeOfType<DelayedProcessingOptions>();
    }
}
