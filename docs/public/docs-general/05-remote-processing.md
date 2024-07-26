# Remote Processing

The remote processing concept was created when using the CQRS pattern in a [modular-monolith architecture](https://www.milanjovanovic.tech/blog/what-is-a-modular-monolith). The challenge was to find a way to execute commands and queries between modules, which are hosted in separated hosts. The mediator of the wemogy CQRS library was extended to support invocation of commands and queries in a remote host via HTTP.

The mediator will not send the raw command or query to the remote host. Instead it creates a `CommandRequest` or `QueryRequest` object which contains also the configured dependencies for the command or query processing.

## Setup the sender side

The sender side is the side which sends the command or query to the receiver side. The sender side requires the following setup:

```csharp
  services.AddCQRS()
    // Configure that the remote HTTP server is available
    .AddRemoteHttpServer(new Uri(configuration.GetRequiredValue("ModuleB:BaseUrl")))
    // Configure that the query GetProductByIdQuery should be processed by the remote HTTP server using the given endpoint
    .ConfigureRemoteQueryProcessing<GetProductByIdQuery>("api/queries/get-product-by-id")
    // Configure that the command CreateProductCommand should be processed by the remote HTTP server using the given endpoint
    .ConfigureRemoteCommandProcessing<CreateProductCommand>("api/commands/create-product");
```

## Setup the receiver side

The receiver side is the side which receives the command or query from the sender side. The receiver side requires the following setup:

### Query endpoint

The `QueryEndpointBase` will setup the scoped dependencies for the query processing and executes the query processing.

```csharp
using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;

public class GetProductByIdQueryEndpoint : QueryEndpointBase<GetProductByIdQuery, Domain.Entities.Product>
{
}
```

### Command without result endpoint

The `CommandEndpointBase` will setup the scoped dependencies for the command processing and executes the command processing.

```csharp
using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;

public class PrintHelloWorldCommandEndpoint : CommandEndpointBase<PrintHelloWorldCommand>
{
}
```

### Command with result endpoint

The `CommandEndpointBase` will setup the scoped dependencies for the command processing, executes the command processing and returns the result.

```csharp
using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;

public class GreeterCommandEndpoint : CommandEndpointBase<GreeterCommand, string>
{
}
```
