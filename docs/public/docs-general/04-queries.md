# Queries

## Introduction

TBD...

## Principles

- A Query should always only work exactly one way and not support multiple constuctors or execution ways (ref [GitHub Issue](https://github.com/wemogy/libs-cqrs/issues/51))

## Query definition

If you want to create a new query, you need to create a class, which implements the generic `IQuery<TResult>` interface, where the genic parameter represents the data type of the result. By nature every query has exactly one result.

```csharp
public class GetUserQuery : IQuery<User>
{
  public Guid Id { get; }

  public GetUserQuery(Guid id)
  {
    Id = id;
  }
}
```

## Query handling

The actual implementation of the defined query goes in the query handler.

```csharp
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>
{
  public Task<User> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
  {
    var user = new User()
    {
        Firstname = query.Id.ToString(),
    };
    return Task.FromResult(user);
  }
}
```

## Query authorization

### Restricting query execution

Please first have a look at the [Filtering](#filtering) section, which explains how to filter query results on repository level. However, if it's required to restrict the query execution itself, you can implement the `IQueryAuthorization<TQuery>` interface.

```csharp
public class GetUserQueryAuthorization : IQueryAuthorization<GetUserQuery>
{
    public Task AuthorizeAsync(GetUserQuery query)
    {
        if (query.FirstName == "ThrowExceptionInGetUserQueryAuthorization")
        {
            throw Error.Authorization(
                "Unauthorized",
                "You are not allowed to access this resource.");
        }

        return Task.CompletedTask;
    }
}
```

### Filtering

It's recommended to implement the filtering logic near to the data source request to minimize the risk that you query a data source somewhere in your application and you forget to filter which would lead to a **data breach**.

#### IDatabaseRepositoryFilter

If your query internally talks to a `IDatabaseRepository` service, it's recommended to put the filtering logic in a `DatabaseRepositoryFilter`. For more information checkout: [MyDocs](https://)

#### Custom filter

If your query internally talks to an external API or any other custom data source and you need to filter the results based on the current context, it's recommended to implement this filtering logic in the wrapper implementation of the specific data source.


## Example: Hello world

In this little sample of `Wemogy.CQRS` we will implement a query without parameters with the belonging query handler. Moreover we will register `wemogy.CQRS` in the dependency injection and finally execute the query to get a `Hello World!` string back.

### The Query model

For each query its required to create a model of the query itself, which contains all information which are required to execute the query.

```csharp
using Wemogy.CQRS.Queries.Abstractions;

public class HelloWorldQuery : IQuery<string>
{
}
```

### The Query handler

The second mandatory implementation for a query is a query handler, which contains the actual implementation of the query action.

```csharp
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;

public class HelloWorldQueryHandler : IQueryHandler<HelloWorldQuery, string>
{
  public Task<string> HandleAsync(HelloWorldQuery query)
  {
    return "Hello world!";
  }
}
```

### Registering the query

It's required to execute `services.AddCQRS();` in your dependency injection file of the assembly which contains the queries. In addition its also supported to pass one or multiple assemblies to the `AddCQRS()` extension method, in case that you need to call it from another assembly.

### Executing the query

This sample is part of a .NET Core controller class.

```csharp
using Wemogy.CQRS.Queries.Abstractions;

public class HelloWorldController : ControllerBase
{
  private readonly IQueries _queries;

  public HelloWorldController(IQueries queries)
  {
    _queries = queries;
  }

  [HttpGet]
  public async Task<ActionResult> SayHelloWorld()
  {
    // creating the query with all required information
    var helloWorldQuery = new HelloWorldQuery();

    // executing the query though the IQueries mediator
    var result = await _queries.QueryAsync(helloWorldQuery);

    return Ok(result);
  }
}
```

## FAQ

### How to allow query executing only for specific context?

As a developer I want to allow query execution only if the `IContext.IsAdmin` flag is set to true. How should I implement this?

**TBD...**

### How to set properties based on a context?

As a developer I want to set the `User.CompletedTasksThisWeek` property only for the current user when users are queried (for privacy reasons, to prevent that someone is monitoring the work speed). How should I implement this?

**TBD...**
