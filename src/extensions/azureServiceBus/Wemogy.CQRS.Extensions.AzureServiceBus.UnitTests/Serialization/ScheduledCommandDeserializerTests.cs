using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Wemogy.Core.Json;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.AzureServiceBus.Serialization;
using Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Testing.Commands.PrintContext;
using Xunit;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.UnitTests.Serialization;

public class ScheduledCommandDeserializerTests
{
    [Fact]
    public void Deserialize_ShouldSupportScheduledCommandEnvelope()
    {
        // Arrange
        var command = new PrintContextCommand { TenantId = "tenant-1" };
        var scheduledCommand = new ScheduledCommand<PrintContextCommand>(new List<CommandQueryDependency>(), command);
        var body = BinaryData.FromString(
            JsonSerializer.Serialize(scheduledCommand, WemogyJson.Options));

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(body);

        // Assert
        result.Command.Id.Should().Be(command.Id);
        result.Command.TenantId.Should().Be("tenant-1");
        result.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_ShouldSupportRawEventGridEvent()
    {
        // Arrange
        var body = BinaryData.FromString(
            """
            {"topic": "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account","eventType": "Microsoft.Storage.BlobCreated","data": {"api": "PutBlob","contentType": "application/x-msdownload","contentLength": 6,"blobType": "BlockBlob","storageDiagnostics": { "batchId": "b0b5a3e2-0c1f-4a2c-8a62-8f3c6f9a1b2d" }},"dataVersion": "","metadataVersion": "1","eventTime": "2026-10-12T03:04:45.5662086Z"}
            """);

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<BlobCreatedCommand>(body);

        // Assert
        result.Dependencies.Should().BeEmpty();
        result.CommandType.Should().Be(typeof(BlobCreatedCommand));
        result.Command.EventType.Should().Be("Microsoft.Storage.BlobCreated");
        result.Command.EventTime.Should().Be(DateTimeOffset.Parse("2026-10-12T03:04:45.5662086Z"));
        result.Command.Data.ContentLength.Should().Be(6);
    }

    public class BlobCreatedCommand : ICommand
    {
        public string Id { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public DateTimeOffset EventTime { get; set; }

        public BlobCreatedData Data { get; set; } = new BlobCreatedData();
    }

    public class BlobCreatedData
    {
        public string Api { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public long ContentLength { get; set; }
    }
}
