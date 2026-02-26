using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.AzureServiceBus.Services;
using Wemogy.CQRS.Extensions.AzureServiceBus.Setup;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.AzureServiceBus
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Try to register a <see cref="ServiceBusClient"/> using a connection string for use with scheduled commands in the CQRS setup environment.
        /// </summary>
        /// <param name="setupEnvironment">The CQRS setup environment to active Azure Service Bus in</param>
        /// <param name="azureServiceBusConnectionString">The connection string to the AzureServiceBus</param>
        /// <param name="serviceBusClientOptions">Optional <see cref="ServiceBusClientOptions"/> for configuring the <see cref="ServiceBusClient"/></param>
        public static AzureServiceBusSetupEnvironment AddAzureServiceBus(
            this CQRSSetupEnvironment setupEnvironment,
            string azureServiceBusConnectionString,
            ServiceBusClientOptions? serviceBusClientOptions = null)
        {
            var client = new ServiceBusClient(azureServiceBusConnectionString, serviceBusClientOptions);
            setupEnvironment.ServiceCollection.TryAddKeyedSingleton(
                client.FullyQualifiedNamespace,
                new AzureServiceBusSetupEnvironment(client, setupEnvironment.ServiceCollection));

            var azureServiceBusSetupEnvironment =
                setupEnvironment
                    .ServiceCollection.BuildServiceProvider().GetRequiredKeyedService<AzureServiceBusSetupEnvironment>(client.FullyQualifiedNamespace);

            setupEnvironment.ServiceCollection.AddSingleton<IScheduledCommandService, AzureServiceBusScheduledCommandService>(
                _ => new AzureServiceBusScheduledCommandService(client, azureServiceBusSetupEnvironment));

            return azureServiceBusSetupEnvironment;
        }

        /// <summary>
        /// Try to register an existing <see cref="ServiceBusClient"/> for use with scheduled commands in the CQRS setup environment.
        /// </summary>
        /// <param name="setupEnvironment">The CQRS setup environment to activate Azure Service Bus in.</param>
        /// <param name="client">The existing <see cref="ServiceBusClient"/> instance.</param>
        /// <returns>An <see cref="AzureServiceBusSetupEnvironment"/> configured with the provided client.</returns>
        public static AzureServiceBusSetupEnvironment AddAzureServiceBusWithClient(
            this CQRSSetupEnvironment setupEnvironment,
            ServiceBusClient client)
        {
            setupEnvironment.ServiceCollection.TryAddKeyedSingleton(
                client.FullyQualifiedNamespace,
                new AzureServiceBusSetupEnvironment(client, setupEnvironment.ServiceCollection));

            var azureServiceBusSetupEnvironment =
                setupEnvironment
                    .ServiceCollection.BuildServiceProvider().GetRequiredKeyedService<AzureServiceBusSetupEnvironment>(client.FullyQualifiedNamespace);

            setupEnvironment.ServiceCollection.AddSingleton<IScheduledCommandService, AzureServiceBusScheduledCommandService>(
                _ => new AzureServiceBusScheduledCommandService(client, azureServiceBusSetupEnvironment));

            return azureServiceBusSetupEnvironment;
        }

        /// <summary>
        /// Try to register a <see cref="ServiceBusClient"/> using a fully qualified namespace and <see cref="TokenCredential"/>
        /// for use with scheduled commands in the CQRS setup environment.
        /// </summary>
        /// <param name="setupEnvironment">The CQRS setup environment to activate Azure Service Bus in.</param>
        /// <param name="fullyQualifiedNamespace">The fully qualified namespace of the Azure Service Bus.</param>
        /// <param name="azureServiceBusCredential">The <see cref="TokenCredential"/> for authentication.</param>
        /// <param name="serviceBusClientOptions">Optional <see cref="ServiceBusClientOptions"/> for configuring the <see cref="ServiceBusClient"/></param>
        /// <returns>An <see cref="AzureServiceBusSetupEnvironment"/> configured with the created client.</returns>
        public static AzureServiceBusSetupEnvironment AddAzureServiceBusWithFullNamespace(
            this CQRSSetupEnvironment setupEnvironment,
            string fullyQualifiedNamespace,
            TokenCredential azureServiceBusCredential,
            ServiceBusClientOptions? serviceBusClientOptions = null)
        {
            var client = new ServiceBusClient(fullyQualifiedNamespace, azureServiceBusCredential, serviceBusClientOptions);

            setupEnvironment.ServiceCollection.TryAddKeyedSingleton(
                fullyQualifiedNamespace,
                new AzureServiceBusSetupEnvironment(client, setupEnvironment.ServiceCollection));

            var azureServiceBusSetupEnvironment =
                setupEnvironment
                    .ServiceCollection.BuildServiceProvider().GetRequiredKeyedService<AzureServiceBusSetupEnvironment>(fullyQualifiedNamespace);

            setupEnvironment.ServiceCollection.TryAddSingleton<IScheduledCommandService>(
                _ => new AzureServiceBusScheduledCommandService(client, azureServiceBusSetupEnvironment));

            return azureServiceBusSetupEnvironment;
        }
    }
}
