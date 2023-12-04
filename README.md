# ![wemogy logo](https://wemogyimages.blob.core.windows.net/logos/wemogy-github-tiny.png) CQRS Framework

A framework to simply use the CQRS pattern in any .NET project.

## Getting started

Install the [NuGet package](https://www.nuget.org/packages/Wemogy.CQRS) into your project.

```bash
dotnet add package wemogy.CQRS
```

Add CQRS to your dependency Injection container.

```csharp
services.AddCQRS();
```

This scans the calling assembly for all Commands, Queries, Command Handlers, Query Handlers, and so on and adds them to the DI containers along with the `ICommands` and `IQueries` executioners.

## Commands

Define your first Command, by creating a class for it, wich implements the `ICommand<TReturnType>` interface. The Command itself does not contain any execution logic and only collects the information needed for the exectution. The Command execution is done at the Command Handler.

```csharp
public class FooCommand : ICommand<string>
{
    public string Demo { get; }

    public FooCommand(string demo)
    {
        Demo = demo;
    }
}
```

Each Command needs a Command Handler, which handles the execution of a Command. To create a Command Handler, implement the `ICommandHandler<TCommand, TReturnType>` interface.

```csharp
public class FooCommandHandler : ICommandHandler<FooCommand, string>
{
    private readonly MyExternalApi _client;

    public FooQueryHandler(MyExternalApi client)
    {
        _client = client;
    }
    public async Task<string> HandleAsync(FooCommand command)
    {
        // Do something...
        var result = _client.PostAsync(command.Demo);
        return result;
    }
}
```

To execute a Command, get the `ICommands` executioner from the DI, instantiate the Command and pass it to the executioner.

```csharp
public class Demo
{
    private readonly ICommands _commands { get; }
    private readonly IQueries _queries { get; }

    public MainControllerBase(ICommands commands, IQueries queries)
    {
        Commands = commands;
        Queries = queries;
    }

    public async Task<string> Something()
    {
        var command = new FooCommand("bar");
        var result = await Commands.RunAsync(command);
        return result;
    }
}
```

### Delayed command processing

Commands can be executed immediately or be delayed to be processed in the background. Delaying command works by registering a delayed command handling processor.

This library implements the following delayed command processing providers

- Azure Service Bus
- Hangfire ([deprecated](https://github.com/wemogy/libs-cqrs/tree/cd68317355449ac9386aa2b9685f0c6bab61149a))


## Queries

Define your first Query, by creating a class for it, wich implements the `IQuery<TReturnType>` interface. The Query itself does not contain any execution logic and only collects the information needed for the exectution. The Query execution is done at the Command Handler.

```csharp
public class FooQuery : IQuery<string>
{
    public string Demo { get; }

    public FooQuery(string demo)
    {
        Demo = demo;
    }
}
```

Each Query needs a Query Handler, which handles the execution of a Query. To create a Query Handler, implement the `IQueryHandler<TCommand, TReturnType>` interface.

```csharp
public class FooQueryHandler : IQueryHandler<FooQuery, string>
{
    private readonly MyDatabaseClient _client;

    public FooQueryHandler(MyDatabaseClient client)
    {
        _client = client;
    }

    public async Task<string> HandleAsync(FooQuery query)
    {
        // Do something...
        var result = await _client.GetById(query.Demo);
        return result;
    }
}
```

To execute a Query, get the `IQueries` executioner from the DI, instantiate the Query and pass it to the executioner.

```csharp
public class Demo
{
    private readonly ICommands _commands { get; }
    private readonly IQueries _queries { get; }

    public MainControllerBase(ICommands commands, IQueries queries)
    {
        Commands = commands;
        Queries = queries;
    }

    public async Task<string> GetSomething()
    {
        var query = new FooQuery("bar");
        var result = await Queries.QueryAsync(query);
        return result;
    }
}
```

---

Checkout the full [Documentation](https://libs-cqrs.docs.wemogy.com/) to get information about the available classes and extensions.
