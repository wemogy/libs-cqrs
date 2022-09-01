# Delayed commands

## Idea

```csharp

class MyDelayedCommand : IDelayedCommand
{
  // properties...
}

// Startup:
services
    .AddAzureServiceBus(...) // Returns AzureServiceBusSetupEnvironment
    .WithAutomaticSubscription(enabled: handlingEnabled) // Returns AzureServiceBusSetupEnvironment
    .AddSender<MyServiceBusSenderService>("my-queue-name") // Returns AzureServiceBusSetupEnvironment
    .AddHandler<MyDelayedCommand, MyDelayedCommandHandler>("my-queue-name").When(handlingEnabled) // Returns AzureServiceBusSetupEnvironment

```
