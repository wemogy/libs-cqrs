using Azure.Core;
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

        /// <summary>
        /// Registers an existing <see cref="ServiceBusClient"/> for use with scheduled commands in the CQRS setup environment.
        /// </summary>
        /// <param name="setupEnvironment">The CQRS setup environment to activate Azure Service Bus in.</param>
        /// <param name="client">The existing <see cref="ServiceBusClient"/> instance.</param>
        /// <returns>An <see cref="AzureServiceBusSetupEnvironment"/> configured with the provided client.</returns>
        public static AzureServiceBusSetupEnvironment AddAzureServiceBusWithClient(
            this CQRSSetupEnvironment setupEnvironment,
            ServiceBusClient client)
        {
            var azureServiceBusSetupEnvironment =
                new AzureServiceBusSetupEnvironment(client, setupEnvironment.ServiceCollection);

            setupEnvironment.ServiceCollection.AddSingleton<IScheduledCommandService, AzureServiceBusScheduledCommandService>(
                _ => new AzureServiceBusScheduledCommandService(client, azureServiceBusSetupEnvironment));

            return azureServiceBusSetupEnvironment;
        }

        /// <summary>
        /// Registers a <see cref="ServiceBusClient"/> using a fully qualified namespace and <see cref="TokenCredential"/>
        /// for use with scheduled commands in the CQRS setup environment.
        /// </summary>
        /// <param name="setupEnvironment">The CQRS setup environment to activate Azure Service Bus in.</param>
        /// <param name="fullyQualifiedNamespace">The fully qualified namespace of the Azure Service Bus.</param>
        /// <param name="azureServiceBusCredential">The <see cref="TokenCredential"/> for authentication.</param>
        /// <returns>An <see cref="AzureServiceBusSetupEnvironment"/> configured with the created client.</returns>
        public static AzureServiceBusSetupEnvironment AddAzureServiceBusWithFullNamespace(
            this CQRSSetupEnvironment setupEnvironment,
            string fullyQualifiedNamespace,
            TokenCredential azureServiceBusCredential)
        {
            var client = new ServiceBusClient(fullyQualifiedNamespace, azureServiceBusCredential);
            var azureServiceBusSetupEnvironment =
                new AzureServiceBusSetupEnvironment(client, setupEnvironment.ServiceCollection);

            setupEnvironment.ServiceCollection.AddSingleton<IScheduledCommandService, AzureServiceBusScheduledCommandService>(
                _ => new AzureServiceBusScheduledCommandService(client, azureServiceBusSetupEnvironment));

            return azureServiceBusSetupEnvironment;
        }
    }
}
