namespace Wemogy.CQRS.Extensions.AzureServiceBus.Config
{
    public class DelayedProcessingOptions
    {
        /// <summary>
        /// Defines the name of the queue where the messages should be sent to/received from
        /// </summary>
        public string? QueueName { get; set; }

        public bool IsSessionSupported { get; set; }

        /// <summary>
        /// Allows message bodies which are not a scheduled command envelope to be deserialized
        /// into the command type directly, e.g. Event Grid events which are forwarded to the queue.
        /// Disabled by default, so that unexpected message bodies are rejected instead of being
        /// executed as a command with default values.
        /// </summary>
        public bool IsRawCommandMessageSupported { get; set; }
    }
}
