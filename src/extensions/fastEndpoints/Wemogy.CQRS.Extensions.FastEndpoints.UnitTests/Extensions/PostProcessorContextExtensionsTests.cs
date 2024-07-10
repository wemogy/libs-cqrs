using FastEndpoints;
using FluentAssertions;
using Moq;
using Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.Extensions;

public class PostProcessorContextExtensionsTests
{
    /// <summary>
    /// This method checks if the MarkExceptionAsHandled implementation from FastEndpoints is working as expected
    /// This is required, because the implementation is only internal accessible, so we need to hardcode the key
    /// </summary>
    [Fact]
    public void MarkExceptionAsHandled_ShouldHaveTheExpectedImplementation()
    {
        // Arrange
        var defaultHttpContext = new DefaultHttpContext();
        var mockContext = new Mock<IPostProcessorContext>
        {
            // enable CallBase to tell Moq to use the default interface implementations (like MarkExceptionAsHandled)
            CallBase = true
        };
        mockContext.SetupGet(m => m.HttpContext).Returns(defaultHttpContext);

        // Act
        mockContext.Object.MarkExceptionAsHandled();

        // Assert
        defaultHttpContext.Items.Should().ContainSingle(x => ReferenceEquals(x.Key, "3") && x.Value == null);
    }

    [Fact]
    public void IsExceptionHandled_ShouldBeTrueIfMarkExceptionAsHandledWasCalled()
    {
        // Arrange
        var defaultHttpContext = new DefaultHttpContext();
        var mockContext = new Mock<IPostProcessorContext>
        {
            // enable CallBase to tell Moq to use the default interface implementations (like MarkExceptionAsHandled)
            CallBase = true
        };
        mockContext.SetupGet(m => m.HttpContext).Returns(defaultHttpContext);
        mockContext.Object.MarkExceptionAsHandled();

        // Act
        var result = mockContext.Object.IsExceptionHandled();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExceptionHandled_ShouldBeFalseIfMarkExceptionAsHandledWasNotCalled()
    {
        // Arrange
        var defaultHttpContext = new DefaultHttpContext();
        var mockContext = new Mock<IPostProcessorContext>
        {
            // enable CallBase to tell Moq to use the default interface implementations (like MarkExceptionAsHandled)
            CallBase = true
        };
        mockContext.SetupGet(m => m.HttpContext).Returns(defaultHttpContext);

        // Act
        var result = mockContext.Object.IsExceptionHandled();

        // Assert
        result.Should().BeFalse();
    }
}
