using FastEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Queries.GetAge;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.LogTestContext;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Queries.RequestTestContext;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.Endpoints;

public class QueryEndpointBaseTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public QueryEndpointBaseTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CommandEndpointBase_ShouldAddDependenciesFromQueryRequest()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var cqrsSetupEnvironment = serviceCollection.AddCQRS();
        var endpoint = Factory.Create<RequestTestContextQueryEndpoint>(
            c =>
        {
            // Add the test services to the endpoint http context
            c.AddTestServices(
                s =>
            {
                s.AddSingleton(cqrsSetupEnvironment);
            });
        });
        var testContext = new TestContext()
        {
            UserId = Guid.NewGuid().ToString()
        };
        var queryRequest = new QueryRequest<RequestTestContextQuery>(
            new RequestTestContextQuery(),
            new List<CommandQueryDependency>()
            {
                CommandQueryDependency.Create(typeof(TestContext), typeof(TestContext), testContext)
            });

        // Act
        // ToDo: fix this test
        await endpoint.HandleAsync(queryRequest, CancellationToken.None);

        // Assert
        LogTestContextCommandHandler.LogHistory.Should().ContainSingle().Which.UserId.Should().Be(testContext.UserId);
    }

    [Fact]
    public async Task GetAgeQuery_HappyPath()
    {
        // Arrange
        var client = _factory.CreateClient();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQRS(typeof(GetAgeQuery).Assembly)
            .AddRemoteHttpServer(client)
            .ConfigureRemoteQueryProcessing<GetAgeQuery>("api/queries/get-age");
        var queries = serviceCollection.BuildServiceProvider().GetRequiredService<IQueries>();
        var getAgeQuery = new GetAgeQuery("Micky Mouse");

        // Act
        var result = await queries.QueryAsync(getAgeQuery);

        // Assert
        result.Should().Be(18);
    }
}
