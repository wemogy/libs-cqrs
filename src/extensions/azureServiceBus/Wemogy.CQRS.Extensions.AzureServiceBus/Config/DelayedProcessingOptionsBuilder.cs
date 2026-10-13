namespace Wemogy.CQRS.Extensions.AzureServiceBus.Config
{
    public class DelayedProcessingOptionsBuilder
    {
        private readonly DelayedProcessingOptions _delayedProcessingOptions;

        public DelayedProcessingOptionsBuilder()
        {
            _delayedProcessingOptions = new DelayedProcessingOptions();
        }

        public DelayedProcessingOptionsBuilder WithQueueName(string queueName)
        {
            _delayedProcessingOptions.QueueName = queueName;
            return this;
        }

        public DelayedProcessingOptionsBuilder WithSessionSupport()
        {
            _delayedProcessingOptions.IsSessionSupported = true;
            return this;
        }

        /// <summary>
        /// Allows message bodies which are not a scheduled command envelope to be deserialized
        /// into the command type directly, e.g. Event Grid events which are forwarded to the queue.
        /// Such messages carry no command query dependencies, so the command and its processors
        /// must not rely on them.
        /// </summary>
        public DelayedProcessingOptionsBuilder WithRawCommandMessageSupport()
        {
            _delayedProcessingOptions.IsRawCommandMessageSupported = true;
            return this;
        }

        public DelayedProcessingOptions Build()
        {
            return _delayedProcessingOptions;
        }
    }
}
