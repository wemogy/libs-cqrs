using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.KitchenSinkWithoutResult;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.KitchenSinkWithResult;
using Wemogy.CQRS.UnitTests.TestApplication.Commands.RecalculateTotalUserCount;
using Xunit;

namespace Wemogy.CQRS.UnitTests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddImplementationsOfInterfaceScoped_ShouldWork()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            Assembly.GetCallingAssembly()
        }.ToList();

        // Act
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandPreProcessor<>),
            assemblies);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.GetServices<ICommandPreProcessor<KitchenSinkWithoutResultCommand>>()
            .Should().HaveCount(2);
        serviceProvider.GetServices<ICommandPreProcessor<CreateUserCommand>>()
            .Should().HaveCount(1);
        serviceProvider.GetServices<ICommandPreProcessor<RecalculateTotalUserCountCommand>>()
            .Should().HaveCount(0);
    }

    [Fact]
    public void AddImplementationsOfInterfaceScoped_ShouldAddOnlyOnceIfCalledManyTimes()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            Assembly.GetCallingAssembly()
        }.ToList();

        // Act
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandPreProcessor<>),
            assemblies);
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandPreProcessor<>),
            assemblies);
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandPreProcessor<>),
            assemblies);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.GetServices<ICommandPreProcessor<KitchenSinkWithoutResultCommand>>()
            .Should().HaveCount(2);
        serviceProvider.GetServices<ICommandPreProcessor<CreateUserCommand>>()
            .Should().HaveCount(1);
        serviceProvider.GetServices<ICommandPreProcessor<RecalculateTotalUserCountCommand>>()
            .Should().HaveCount(0);
    }

    [Fact]
    public void AddImplementationsOfInterfaceScoped_ShouldWorkForInterfaceWithTwoGenerics()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            Assembly.GetCallingAssembly()
        }.ToList();

        // Act
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandHandler<,>),
            assemblies);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.GetServices<ICommandHandler<KitchenSinkWithResultCommand, int>>()
            .Should().ContainSingle();
    }
}
