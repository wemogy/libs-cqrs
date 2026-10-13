using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Wemogy.Core.Errors;
using Wemogy.Core.Json;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Serialization
{
    /// <summary>
    /// Deserializes a Service Bus message body into a <see cref="ScheduledCommand{TCommand}"/>.
    /// Supports two message formats:
    /// 1. The wemogy CQRS envelope (<c>{ "dependencies": [...], "commandType": "...", "command": { ... } }</c>)
    ///    which is produced when a command is scheduled via the CQRS library.
    /// 2. A raw command payload (e.g. an Event Grid event forwarded to Service Bus), which is
    ///    deserialized directly into the command type without any dependencies.
    /// The raw format has to be enabled per command via
    /// <c>ConfigureDelayedProcessing&lt;TCommand&gt;(builder =&gt; builder.WithRawCommandMessageSupport())</c>.
    /// Without it, a body which is not an envelope is rejected instead of being executed as a command
    /// with default values.
    /// </summary>
    public static class ScheduledCommandDeserializer
    {
        private const string CommandPropertyName = "command";
        private const string CommandTypePropertyName = "commandType";
        private const string MessageFormatTagName = "wemogy.cqrs.message_format";

        /// <param name="body">The raw message body.</param>
        /// <param name="isRawCommandMessageSupported">
        /// Whether a body which is not an envelope may be deserialized into the command type directly.
        /// </param>
        /// <param name="messageFormat">
        /// The format the sender stamped onto the message, see <see cref="ScheduledCommandMessageFormat"/>.
        /// Null for messages from foreign senders or from older versions of this library, in which case
        /// the format is derived from the body content.
        /// </param>
        public static ScheduledCommand<TCommand> Deserialize<TCommand>(
            BinaryData body,
            bool isRawCommandMessageSupported = false,
            string? messageFormat = null)
            where TCommand : ICommandBase
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw Error.Unexpected(
                    "UnexpectedMessageFormat",
                    $"Expected a JSON object as message body, but got {root.ValueKind}.");
            }

            if (IsScheduledCommandEnvelope(root, messageFormat))
            {
                Activity.Current?.SetTag(MessageFormatTagName, ScheduledCommandMessageFormat.ScheduledCommandEnvelope);
                return DeserializeEnvelope<TCommand>(root);
            }

            if (!isRawCommandMessageSupported)
            {
                var message = $"The message body is not a scheduled command envelope. To deserialize it into "
                              + $"{typeof(TCommand).Name} directly, enable raw command messages via "
                              + "ConfigureDelayedProcessing(builder => builder.WithRawCommandMessageSupport()).";

                throw Error.Unexpected("UnexpectedMessageFormat", message);
            }

            Activity.Current?.SetTag(MessageFormatTagName, ScheduledCommandMessageFormat.RawCommand);
            return DeserializeRawCommand<TCommand>(root);
        }

        private static ScheduledCommand<TCommand> DeserializeEnvelope<TCommand>(JsonElement root)
            where TCommand : ICommandBase
        {
            // The command has to be checked before deserializing, because the constructor of
            // ScheduledCommand dereferences it, which would fail with a NullReferenceException.
            if (!TryGetProperty(root, CommandPropertyName, out var command) || command.ValueKind != JsonValueKind.Object)
            {
                throw Error.Unexpected(
                    "ScheduledCommandIsNull",
                    "Scheduled command is null.");
            }

            var scheduledCommand = root.Deserialize<ScheduledCommand<TCommand>>(WemogyJson.Options);

            if (scheduledCommand is null || scheduledCommand.Command is null)
            {
                throw Error.Unexpected(
                    "ScheduledCommandIsNull",
                    "Scheduled command is null.");
            }

            if (scheduledCommand.Dependencies is null)
            {
                // An envelope without dependencies is valid, but the dependency registration
                // expects a list instead of null.
                return new ScheduledCommand<TCommand>(new List<CommandQueryDependency>(), scheduledCommand.Command);
            }

            return scheduledCommand;
        }

        private static ScheduledCommand<TCommand> DeserializeRawCommand<TCommand>(JsonElement root)
            where TCommand : ICommandBase
        {
            var command = root.Deserialize<TCommand>(WemogyJson.Options);

            if (command is null)
            {
                throw Error.Unexpected(
                    "CommandIsNull",
                    $"Message body could not be deserialized into {typeof(TCommand).Name}.");
            }

            return new ScheduledCommand<TCommand>(new List<CommandQueryDependency>(), command);
        }

        private static bool IsScheduledCommandEnvelope(JsonElement root, string? messageFormat)
        {
            if (messageFormat != null)
            {
                return string.Equals(
                    messageFormat,
                    ScheduledCommandMessageFormat.ScheduledCommandEnvelope,
                    StringComparison.OrdinalIgnoreCase);
            }

            // Classify by the properties of the envelope itself rather than by the command inside it,
            // so that an envelope with a missing or null command is still recognized as an envelope
            // and reports a clear error instead of being deserialized as a raw command.
            return TryGetProperty(root, CommandTypePropertyName, out _)
                   || (TryGetProperty(root, CommandPropertyName, out var command)
                       && command.ValueKind == JsonValueKind.Object);
        }

        private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement value)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
