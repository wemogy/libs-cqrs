using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Wemogy.Core.Errors.Enums;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.ThrowError;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.Endpoints;

public class CommandEndpointBaseExceptionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CommandEndpointBaseExceptionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Unexpected)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Authorization)]
    [InlineData(ErrorType.PreconditionFailed)]
    public async Task PrintHelloWorldCommand_ShouldReturnCorrectErrorException(ErrorType errorType)
    {
        // Arrange
        var client = _factory.CreateClient();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQRS(typeof(ThrowErrorCommand).Assembly)
            .AddRemoteHttpServer(client)
            .ConfigureRemoteCommandProcessing<ThrowErrorCommand>("api/commands/throw-error");
        var commands = serviceCollection.BuildServiceProvider().GetRequiredService<ICommands>();
        var throwErrorCommand = new ThrowErrorCommand(errorType);

        // Act
        var exception = await Record.ExceptionAsync(() => commands.RunAsync(throwErrorCommand));

        // Assert
        exception.Should().NotBeNull().And.BeAssignableTo<ErrorException>()
            .Which.ErrorType.Should().Be(errorType);
        exception?.GetType().Name.Should().Be($"{errorType}ErrorException");
    }
}
