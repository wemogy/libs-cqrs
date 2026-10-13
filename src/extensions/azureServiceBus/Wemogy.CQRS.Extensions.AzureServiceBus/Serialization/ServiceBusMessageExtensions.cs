using Azure.Messaging.ServiceBus;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Serialization
{
    internal static class ServiceBusMessageExtensions
    {
        /// <summary>
        /// Reads the message format which was stamped onto the message by the sender.
        /// Returns null for messages which were not sent by this library, or which were sent
        /// by a version of this library that did not stamp the format yet.
        /// </summary>
        internal static string? GetMessageFormat(this ServiceBusReceivedMessage message)
        {
            if (message.ApplicationProperties.TryGetValue(ScheduledCommandMessageFormat.PropertyName, out var messageFormat))
            {
                return messageFormat as string;
            }

            return null;
        }
    }
}
