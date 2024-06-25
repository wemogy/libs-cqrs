using FastEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.Greeter;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.PrintHelloWorld;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.LogTestContext;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.Endpoints;

public class CommandEndpointBaseTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CommandEndpointBaseTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CommandEndpointBase_ShouldAddDependenciesFromCommandRequest()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var cqrsSetupEnvironment = serviceCollection.AddCQRS();
        var endpoint = Factory.Create<LogTestContextCommandEndpoint>(
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
        var commandRequest = new CommandRequest<LogTestContextCommand>(
            new LogTestContextCommand(),
            new List<CommandQueryDependency>()
            {
                CommandQueryDependency.Create(typeof(TestContext), typeof(TestContext), testContext)
            });

        // Act
        await endpoint.HandleAsync(commandRequest, CancellationToken.None);

        // Assert
        LogTestContextCommandHandler.LogHistory.Should().ContainSingle().Which.UserId.Should().Be(testContext.UserId);
    }

    [Fact]
    public async Task PrintHelloWorldCommand_HappyPath()
    {
        // Arrange
        var client = _factory.CreateClient();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQRS(typeof(PrintHelloWorldCommand).Assembly)
            .AddRemoteHttpServer(client)
            .ConfigureRemoteCommandProcessing<PrintHelloWorldCommand>("api/commands/print-hello-world");
        var commands = serviceCollection.BuildServiceProvider().GetRequiredService<ICommands>();
        var printHelloWorldCommand = new PrintHelloWorldCommand();

        // Act
        var exception = await Record.ExceptionAsync(() => commands.RunAsync(printHelloWorldCommand));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task GreeterCommand_HappyPath()
    {
        // Arrange
        var client = _factory.CreateClient();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQRS(typeof(GreeterCommand).Assembly)
            .AddRemoteHttpServer(client)
            .ConfigureRemoteCommandProcessing<GreeterCommand>("api/commands/greeter");
        var commands = serviceCollection.BuildServiceProvider().GetRequiredService<ICommands>();
        var greeterCommand = new GreeterCommand("Mickey Mouse");

        // Act
        var result = await commands.RunAsync(greeterCommand);

        // Assert
        result.Should().Be("Hello, Mickey Mouse!");
    }
}
