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

This scans your assembly for all Comamnds, Queries, Command Handlers, Query Handlers, and so on and adds them to the DI containers along with the `ICommands` and `IQueries` executioners.

## Commands

Define your first command, by creating a class for it, wich implements the `ICommand<TReturnType>` interface. The command itself does not contain any execution logic and only collects the information needed for the exectution. The command execution itself is done at the Command Handler.

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

Each command needs a Command Handler, which handles the execution of a command. To create a Command Handler, implement the `ICommandHandler<TCommand, TReturnType>` interface.

```csharp
public class FooCommandHandler : ICommandHandler<FooCommand, string>
{
    public async Task<string> HandleAsync(FooCommand command)
    {
        string result = "";

        // Do something...

        return result;
    }
}
```

To execute a Command, get the `ICommands` executioner from the DI, instanciate the Command and pass it to the executioner.

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

---

Checkout the full [Documentation](https://libs-cqrs.docs.wemogy.com/) to get information about the available classes and extensions.

