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
    private const string RawEventGridEvent =
        """
        {"topic": "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account","eventType": "Microsoft.Storage.BlobCreated","data": {"api": "PutBlob","contentType": "application/x-msdownload","contentLength": 6,"blobType": "BlockBlob","storageDiagnostics": { "batchId": "b0b5a3e2-0c1f-4a2c-8a62-8f3c6f9a1b2d" }},"dataVersion": "","metadataVersion": "1","eventTime": "2026-10-12T03:04:45.5662086Z"}
        """;

    [Fact]
    public void Deserialize_ShouldSupportScheduledCommandEnvelope()
    {
        // Arrange
        var body = CreateEnvelope(new PrintContextCommand { TenantId = "tenant-1" });

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(body);

        // Assert
        result.Command.TenantId.Should().Be("tenant-1");
        result.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_ShouldSupportScheduledCommandEnvelopeWhichIsAnnouncedByTheSender()
    {
        // Arrange
        var body = CreateEnvelope(new PrintContextCommand { TenantId = "tenant-1" });

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(
            body,
            messageFormat: ScheduledCommandMessageFormat.ScheduledCommandEnvelope);

        // Assert
        result.Command.TenantId.Should().Be("tenant-1");
    }

    [Fact]
    public void Deserialize_ShouldSupportRawEventGridEvent()
    {
        // Arrange
        var body = BinaryData.FromString(RawEventGridEvent);

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<BlobCreatedCommand>(
            body,
            isRawCommandMessageSupported: true);

        // Assert
        result.Dependencies.Should().BeEmpty();
        result.CommandType.Should().Be(typeof(BlobCreatedCommand));
        result.Command.EventType.Should().Be("Microsoft.Storage.BlobCreated");
        result.Command.EventTime.Should().Be(DateTimeOffset.Parse("2026-10-12T03:04:45.5662086Z"));
        result.Command.Data.ContentLength.Should().Be(6);
    }

    [Fact]
    public void Deserialize_ShouldRejectRawEventIfRawCommandMessagesAreNotSupported()
    {
        // Arrange
        var body = BinaryData.FromString(RawEventGridEvent);

        // Act
        var exception = Record.Exception(() => ScheduledCommandDeserializer.Deserialize<BlobCreatedCommand>(body));

        // Assert
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("WithRawCommandMessageSupport");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeUnknownBodyLenientlyIfRawCommandMessagesAreSupported()
    {
        // Arrange
        var body = BinaryData.FromString("""{"somethingElse": 1}""");

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(
            body,
            isRawCommandMessageSupported: true);

        // Assert: documents that raw messages are deserialized leniently, which is why they are opt-in
        result.Command.TenantId.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ShouldRejectEnvelopeWithoutCommand()
    {
        // Arrange
        var body = BinaryData.FromString("""{"dependencies": [], "commandType": "Some.Command, Some.Assembly", "command": null}""");

        // Act
        var exception = Record.Exception(() => ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(
            body,
            isRawCommandMessageSupported: true));

        // Assert
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("Scheduled command is null");
    }

    [Fact]
    public void Deserialize_ShouldSupportEnvelopeWithoutCommandType()
    {
        // Arrange: the command type is diagnostic information, the receiver knows the type already
        var body = BinaryData.FromString(
            """
            {"dependencies": [], "command": {"id": "a", "tenantId": "tenant-1"}}
            """);

        // Act
        var result = ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(
            body,
            isRawCommandMessageSupported: true);

        // Assert
        result.Command.TenantId.Should().Be("tenant-1");
        result.CommandType.Should().Be(typeof(PrintContextCommand));
    }

    [Fact]
    public void Deserialize_ShouldRejectEnvelopeWithoutCommandAndWithoutCommandType()
    {
        // Arrange
        var body = BinaryData.FromString(
            """
            {"dependencies": [], "command": null}
            """);

        // Act
        var exception = Record.Exception(() => ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(body));

        // Assert: neither format, so it is rejected instead of being executed with default values
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("WithRawCommandMessageSupport");
    }

    [Fact]
    public void Deserialize_ShouldRejectBodyWhichIsAnnouncedAsEnvelopeButIsNotOne()
    {
        // Arrange
        var body = BinaryData.FromString(RawEventGridEvent);

        // Act
        var exception = Record.Exception(() => ScheduledCommandDeserializer.Deserialize<BlobCreatedCommand>(
            body,
            isRawCommandMessageSupported: true,
            messageFormat: ScheduledCommandMessageFormat.ScheduledCommandEnvelope));

        // Assert
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("Scheduled command is null");
    }

    [Fact]
    public void Deserialize_ShouldRejectBodyWhichIsNotAJsonObject()
    {
        // Arrange
        var body = BinaryData.FromString("[1, 2, 3]");

        // Act
        var exception = Record.Exception(() => ScheduledCommandDeserializer.Deserialize<PrintContextCommand>(
            body,
            isRawCommandMessageSupported: true));

        // Assert
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("Array");
    }

    private static BinaryData CreateEnvelope(PrintContextCommand command)
    {
        var scheduledCommand = new ScheduledCommand<PrintContextCommand>(new List<CommandQueryDependency>(), command);
        return BinaryData.FromString(JsonSerializer.Serialize(scheduledCommand, WemogyJson.Options));
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
