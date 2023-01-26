using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.UnitTests.TestApplication;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Commands.Runners;

public class VoidCommandRunnerTests
{
    [Fact]
    public async Task RunAsync_ShouldWorkForVoidCommand()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestApplication();
        serviceCollection.AddSingleton(new TestContext
        {
            TenantId = TestContext.TenantAId
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandRunnerBase = serviceProvider.GetRequiredService<VoidCommandRunner<TrackUserLoginCommand>>();
        var createUserCommand = new TrackUserLoginCommand(string.Empty);

        // Act
        var exception = await Record.ExceptionAsync(() => commandRunnerBase.RunAsync(createUserCommand));

        // Assert
        exception.Should().BeNull();
    }
}
