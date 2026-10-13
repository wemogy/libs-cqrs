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

## Message formats

The Azure Service Bus processors accept two body formats.

**The scheduled command envelope** is what this library sends. It wraps the command together
with the resolved command query dependencies and is marked with the
`wemogy-cqrs-message-format` application property:

```json
{
  "dependencies": [],
  "commandType": "MyNamespace.MyDelayedCommand, MyAssembly",
  "command": { }
}
```

The `commandType` is diagnostic information, since the receiver already knows the type it
expects, so an envelope without it is still accepted. If it is present it has to be resolvable
in the receiving process, otherwise the message is rejected.

**A raw command body** is the command payload without any envelope around it. This allows a
handler to consume messages from a foreign sender, for example an Event Grid subscription which
forwards Blob Created events into the queue. It has to be enabled per command, because a body
which is neither format would otherwise be deserialized into a command with default values and
executed instead of being rejected:

```csharp
services
    .AddAzureServiceBus(...)
    .ConfigureDelayedProcessing<BlobCreatedCommand>(builder => builder
        .WithQueueName("blob-created")
        .WithRawCommandMessageSupport())
    .AddDelayedProcessor<BlobCreatedCommand>();
```

Raw messages carry no command query dependencies, so neither the command nor its pre- and
post-processors may rely on ambient context such as a tenant or a user resolved from them.
