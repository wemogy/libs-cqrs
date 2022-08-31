using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.DependencyInjection;
using Wemogy.CQRS.UnitTests.DependencyInjection.Testing.Classes;
using Wemogy.CQRS.UnitTests.DependencyInjection.Testing.Interfaces;
using Xunit;

namespace Wemogy.CQRS.UnitTests.DependencyInjection;

public class DependencyInjectionAccessorTests
{
    [Fact]
    public void GetTypesWhichImplementInterface_ShouldReturnAllTypesOfInterface()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        DependencyInjectionAccessor.Initialize(serviceCollection, Assembly.GetExecutingAssembly());

        // Act
        var classesOfInterfaceA = DependencyInjectionAccessor.GetClassTypesWhichImplementInterface<IInterfaceA>();

        // Assert
        classesOfInterfaceA.Should().HaveCount(4);
        classesOfInterfaceA.Should().Contain(typeof(ClassOfInterfaceAc));
        classesOfInterfaceA.Should().NotContain(typeof(ClassOfInterfaceB));
    }

    [Fact]
    public void GetClassInstancesWhichImplementInterfaceGetClassInstancesWhichImplementInterface_ShouldReturnAllInstancesOfInterface()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        DependencyInjectionAccessor.Initialize(serviceCollection, Assembly.GetExecutingAssembly());

        // Act
        var classesOfInterfaceA = DependencyInjectionAccessor.GetClassInstancesWhichImplementInterface<IInterfaceA>();

        // Assert
        classesOfInterfaceA.Should().HaveCount(4);
        classesOfInterfaceA.Should().Contain(x => x.GetType() == typeof(ClassOfInterfaceAc));
        classesOfInterfaceA.Should().NotContain(x => x.GetType() == typeof(ClassOfInterfaceB));
    }
}
