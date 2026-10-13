namespace Wemogy.CQRS.Extensions.AzureServiceBus.Serialization
{
    /// <summary>
    /// Describes how the body of a Service Bus message is structured.
    /// The format is stamped onto every message which is sent by this library, so that the
    /// receiving side does not have to guess it from the body content.
    /// </summary>
    public static class ScheduledCommandMessageFormat
    {
        /// <summary>
        /// Name of the application property which carries the message format.
        /// </summary>
        public const string PropertyName = "wemogy-cqrs-message-format";

        /// <summary>
        /// The body is a <see cref="Wemogy.CQRS.Common.ValueObjects.ScheduledCommand{TCommand}"/> envelope.
        /// </summary>
        public const string ScheduledCommandEnvelope = "scheduled-command-envelope";

        /// <summary>
        /// The body is the command payload itself, without any envelope around it.
        /// </summary>
        public const string RawCommand = "raw-command";
    }
}
