using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using RestSharp;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.Common;
using Wemogy.CQRS.Extensions.FastEndpoints.RemoteCommandRunners;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.Greeting;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.LogTestContext;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.RemoteCommandRunners;

public class HttpRemoteCommandRunnerTests
{
    [Fact]
    public async Task RunAsync_ShouldRetryRequest()
    {
        // Arrange
        var command = new LogTestContextCommand();
        var responses = new Queue<RestResponse>();
        responses.Enqueue(new RestResponse()
        {
            StatusCode = HttpStatusCode.ServiceUnavailable
        });
        responses.Enqueue(new RestResponse()
        {
            StatusCode = HttpStatusCode.OK,

            // Set IsSuccessful to true
            IsSuccessStatusCode = true,
            ResponseStatus = ResponseStatus.Completed
        });

        var restClientMock = new Mock<IRestClient>();
        restClientMock
            .Setup(
                m => m.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(responses.Dequeue);

        var httpRemoteCommandRunner = new HttpRemoteCommandRunner<LogTestContextCommand>(restClientMock.Object, string.Empty);
        var request = new CommandRequest<LogTestContextCommand>(command, new List<CommandQueryDependency>());

        // Act
        var exception = await Record.ExceptionAsync(() => httpRemoteCommandRunner.RunAsync(request));

        // Assert
        exception.Should().BeNull();
        responses.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_ShouldThrowAfterMaxRetryReachedRequest()
    {
        // Arrange
        var command = new LogTestContextCommand();
        var restClientMock = new Mock<IRestClient>();
        restClientMock
            .Setup(
                m => m.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new RestResponse()
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var httpRemoteCommandRunner = new HttpRemoteCommandRunner<LogTestContextCommand>(restClientMock.Object, string.Empty);
        var request = new CommandRequest<LogTestContextCommand>(command, new List<CommandQueryDependency>());

        // Act
        var exception = await Record.ExceptionAsync(() => httpRemoteCommandRunner.RunAsync(request));

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_ShouldRetryRequestAndReturnResult()
    {
        // Arrange
        var command = new GreetingCommand("Max");
        var resultContent = JsonSerializer.Serialize("Hello, Max!", JsonOptions.JsonSerializerOptions);
        var responses = new Queue<RestResponse>();
        responses.Enqueue(new RestResponse()
        {
            StatusCode = HttpStatusCode.ServiceUnavailable
        });
        responses.Enqueue(new RestResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Content = resultContent,

            // Set IsSuccessful to true
            IsSuccessStatusCode = true,
            ResponseStatus = ResponseStatus.Completed
        });

        var restClientMock = new Mock<IRestClient>();
        restClientMock
            .Setup(
                m => m.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(responses.Dequeue);

        var httpRemoteCommandRunner = new HttpRemoteCommandRunner<GreetingCommand, string>(restClientMock.Object, string.Empty);
        var request = new CommandRequest<GreetingCommand>(command, new List<CommandQueryDependency>());

        // Act
        var result = await httpRemoteCommandRunner.RunAsync(request);

        // Assert
        result.Should().Be("Hello, Max!");
        responses.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_ShouldThrowWithoutResultAfterMaxRetryReachedRequest()
    {
        // Arrange
        var command = new GreetingCommand("Max");
        var restClientMock = new Mock<IRestClient>();
        restClientMock
            .Setup(
                m => m.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new RestResponse()
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var httpRemoteCommandRunner = new HttpRemoteCommandRunner<GreetingCommand, string>(restClientMock.Object, string.Empty);
        var request = new CommandRequest<GreetingCommand>(command, new List<CommandQueryDependency>());

        // Act
        var exception = await Record.ExceptionAsync(() => httpRemoteCommandRunner.RunAsync(request));

        // Assert
        exception.Should().NotBeNull();
    }
}
