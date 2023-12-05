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

        public DelayedProcessingOptions Build()
        {
            return _delayedProcessingOptions;
        }
    }
}
