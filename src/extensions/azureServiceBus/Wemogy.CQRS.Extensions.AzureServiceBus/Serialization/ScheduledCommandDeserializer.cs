using System;
using System.Collections.Generic;
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
    /// </summary>
    public static class ScheduledCommandDeserializer
    {
        private const string CommandPropertyName = "command";
        private const string CommandTypePropertyName = "commandType";

        public static ScheduledCommand<TCommand> Deserialize<TCommand>(BinaryData body)
            where TCommand : ICommandBase
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (IsScheduledCommandEnvelope(root))
            {
                var scheduledCommand = root.Deserialize<ScheduledCommand<TCommand>>(WemogyJson.Options);

                if (scheduledCommand is null || scheduledCommand.Command is null)
                {
                    throw Error.Unexpected(
                        "ScheduledCommandIsNull",
                        "Scheduled command is null.");
                }

                return scheduledCommand;
            }

            var command = root.Deserialize<TCommand>(WemogyJson.Options);
            if (command is null)
            {
                throw Error.Unexpected(
                    "CommandIsNull",
                    $"Message body could not be deserialized into {typeof(TCommand).Name}.");
            }

            return new ScheduledCommand<TCommand>(new List<CommandQueryDependency>(), command);
        }

        private static bool IsScheduledCommandEnvelope(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var hasCommand = false;
            var hasCommandType = false;

            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, CommandPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    hasCommand = property.Value.ValueKind == JsonValueKind.Object;
                }
                else if (string.Equals(property.Name, CommandTypePropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    hasCommandType = property.Value.ValueKind == JsonValueKind.String;
                }
            }

            return hasCommand && hasCommandType;
        }
    }
}
