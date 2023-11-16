using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Services;
using Wemogy.CQRS.Extensions.AzureServiceBus.Setup;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.AzureServiceBus
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Call this method to use Azure Service Bus for scheduled commands
        /// </summary>
        /// <param name="setupEnvironment">The CQRS setup environment to active Azure Service Bus in</param>
        /// <param name="azureServiceBusConnectionString">The connection string to the AzureServiceBus</param>
        public static AzureServiceBusSetupEnvironment AddAzureServiceBus(
            this CQRSSetupEnvironment setupEnvironment,
            string azureServiceBusConnectionString)
        {
            var serviceBusClient = new ServiceBusClient(azureServiceBusConnectionString);
            var azureServiceBusSetupEnvironment =
                new AzureServiceBusSetupEnvironment(serviceBusClient, setupEnvironment.ServiceCollection);

            setupEnvironment.ServiceCollection.AddSingleton<IScheduledCommandService, AzureServiceBusScheduledCommandService>(
                _ => new AzureServiceBusScheduledCommandService(serviceBusClient, azureServiceBusSetupEnvironment));

            return azureServiceBusSetupEnvironment;
        }
    }
}
