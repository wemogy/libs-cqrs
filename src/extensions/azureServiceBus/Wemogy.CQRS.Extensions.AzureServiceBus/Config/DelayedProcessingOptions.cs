namespace Wemogy.CQRS.Extensions.AzureServiceBus.Config
{
    public class DelayedProcessingOptions
    {
        /// <summary>
        /// Defines the name of the queue where the messages should be sent to/received from
        /// </summary>
        public string? QueueName { get; set; }

        public bool IsSessionSupported { get; set; }
    }
}
