using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.AzureServiceBus.Processors;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Processors;

public class ScheduledCommandServiceProvidersTests
{
    [Fact]
    public async Task CreateScope_ShouldResolveTheDependencyValueOfEachMessage()
    {
        // Arrange
        await using var serviceProviders = new ScheduledCommandServiceProviders(new ServiceCollection());

        // Act
        using var firstScope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-a" }));
        using var secondScope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-b" }));

        // Assert
        firstScope.ServiceProvider.GetRequiredService<MessageContext>().TenantId.Should().Be("tenant-a");
        secondScope.ServiceProvider.GetRequiredService<MessageContext>().TenantId.Should().Be("tenant-b");
    }

    [Fact]
    public async Task CreateScope_ShouldShareSingletonsAcrossMessagesWithTheSameDependencyTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<SingletonProbe>();
        await using var serviceProviders = new ScheduledCommandServiceProviders(services);

        // Act
        using var firstScope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-a" }));
        using var secondScope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-b" }));

        // Assert
        firstScope.ServiceProvider.GetRequiredService<SingletonProbe>()
            .Should().BeSameAs(secondScope.ServiceProvider.GetRequiredService<SingletonProbe>());
        serviceProviders.ProviderCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateScope_ShouldBuildOneProviderPerDistinctSetOfDependencyTypes()
    {
        // Arrange
        await using var serviceProviders = new ScheduledCommandServiceProviders(new ServiceCollection());

        // Act
        using var withContext = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-a" }));
        using var withoutDependencies = serviceProviders.CreateScope(new List<CommandQueryDependency>());

        // Assert
        serviceProviders.ProviderCount.Should().Be(2);
        withoutDependencies.ServiceProvider.GetService<MessageContext>().Should().BeNull();
    }

    [Fact]
    public async Task CreateScope_ShouldLetADependencyOverrideAnApplicationRegistrationOfTheSameType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped(_ => new MessageContext { TenantId = "application" });
        await using var serviceProviders = new ScheduledCommandServiceProviders(services);

        // Act
        using var scope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "message" }));

        // Assert
        scope.ServiceProvider.GetRequiredService<MessageContext>().TenantId.Should().Be("message");
    }

    [Fact]
    public async Task CreateScope_ShouldReuseTheScheduledCommandServiceAcrossMessages()
    {
        // Arrange
        // The scheduled command service caches one ServiceBusSender per queue. When every message
        // got its own instance, every message opened new sender links on the shared client until
        // the connection hit the broker's limit of 4999 links ("Cannot allocate more handles").
        // The client does not connect before the first send, so a placeholder namespace is enough.
        var services = new ServiceCollection();
        services
            .AddCQRS()
            .AddAzureServiceBus("Endpoint=sb://placeholder.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=c2VjcmV0");
        await using var serviceProviders = new ScheduledCommandServiceProviders(services);

        // Act
        using var firstScope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-a" }));
        using var secondScope = serviceProviders.CreateScope(Dependencies(new MessageContext { TenantId = "tenant-b" }));

        // Assert
        firstScope.ServiceProvider.GetRequiredService<IScheduledCommandService>()
            .Should().BeSameAs(secondScope.ServiceProvider.GetRequiredService<IScheduledCommandService>());
    }

    private static List<CommandQueryDependency> Dependencies(MessageContext context)
    {
        return new List<CommandQueryDependency>
        {
            CommandQueryDependency.Create(typeof(MessageContext), typeof(MessageContext), context)
        };
    }

    public class MessageContext
    {
        public string TenantId { get; set; } = string.Empty;
    }

    public class SingletonProbe
    {
    }
}
