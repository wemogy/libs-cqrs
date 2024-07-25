using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.AssemblyA;
using Wemogy.CQRS.UnitTests.AssemblyA.Commands.PrintHelloAssemblyA;
using Wemogy.CQRS.UnitTests.AssemblyB;
using Wemogy.CQRS.UnitTests.AssemblyB.Commands.PrintHelloAssemblyB;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;
using Xunit;

namespace Wemogy.CQRS.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public async Task CallingAddCQRSMultipleTimesInDifferentAssembliesShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQRS();
        serviceCollection.AddAssemblyA();
        serviceCollection.AddAssemblyB();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commands = serviceProvider.GetRequiredService<ICommands>();
        var helloAssemblyACommand = new PrintHelloAssemblyACommand();
        var helloAssemblyBCommand = new PrintHelloAssemblyBCommand();
        var trackUserLoginCommand = new TrackUserLoginCommand(Guid.NewGuid().ToString());

        // Act
        var trackUserLoginCommandException = await Record.ExceptionAsync(() => commands.RunAsync(trackUserLoginCommand));
        var helloAssemblyACommandException = await Record.ExceptionAsync(() => commands.RunAsync(helloAssemblyACommand));
        var helloAssemblyBCommandException = await Record.ExceptionAsync(() => commands.RunAsync(helloAssemblyBCommand));

        // Assert
        TrackUserLoginCommandHandler.ExecutedCount[trackUserLoginCommand.UserId].Should().Be(1);
        trackUserLoginCommandException.Should().BeNull();
        PrintHelloAssemblyACommandHandler.CallCount.Should().Be(1);
        helloAssemblyACommandException.Should().BeNull();
        PrintHelloAssemblyBCommandHandler.CallCount.Should().Be(1);
        helloAssemblyBCommandException.Should().BeNull();
    }
}
