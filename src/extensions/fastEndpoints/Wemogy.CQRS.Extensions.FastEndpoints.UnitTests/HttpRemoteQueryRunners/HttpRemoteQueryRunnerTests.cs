using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using RestSharp;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.Common;
using Wemogy.CQRS.Extensions.FastEndpoints.RemoteQueryRunners;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Queries.RequestTestContext;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.HttpRemoteQueryRunners;

public class HttpRemoteQueryRunnerTests
{
    [Fact]
    public async Task QueryAsync_ShouldRetryRequestAndReturnResult()
    {
        // Arrange
        var query = new RequestTestContextQuery();
        var testContext = new TestContext()
        {
            UserId = Guid.NewGuid().ToString()
        };
        var resultContent = JsonSerializer.Serialize(testContext, JsonOptions.JsonSerializerOptions);
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

        var httpRemoteQueryRunner = new HttpRemoteQueryRunner<RequestTestContextQuery, TestContext>(restClientMock.Object, string.Empty);
        var request = new QueryRequest<RequestTestContextQuery>(query, new List<CommandQueryDependency>());

        // Act
        var result = await httpRemoteQueryRunner.QueryAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(testContext);
        responses.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_ShouldThrowWithoutResultAfterMaxRetryReachedRequest()
    {
        // Arrange
        var query = new RequestTestContextQuery();
        var testContext = new TestContext()
        {
            UserId = Guid.NewGuid().ToString()
        };
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

        var httpRemoteQueryRunner = new HttpRemoteQueryRunner<RequestTestContextQuery, TestContext>(restClientMock.Object, string.Empty);
        var request = new QueryRequest<RequestTestContextQuery>(query, new List<CommandQueryDependency>());

        // Act
        var exception = await Record.ExceptionAsync(() => httpRemoteQueryRunner.QueryAsync(request, CancellationToken.None));

        // Assert
        exception.Should().NotBeNull();
    }
}
