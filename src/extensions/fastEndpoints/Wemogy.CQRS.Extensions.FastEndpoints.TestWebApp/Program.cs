using FastEndpoints;
using Wemogy.CQRS;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.FastEndpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.Extensions;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.Greeter;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.PrintHelloWorld;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Queries.GetAge;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FastEndpoints
builder.Services.AddFastEndpoints();

// Add CQRS
builder.Services.AddCQRS()
    .AddRemoteHttpServer(new Uri("http://localhost:6005"))
    .ConfigureRemoteCommandProcessing<PrintHelloWorldCommand>("api/commands/print-hello-world")
    .ConfigureRemoteCommandProcessing<GreeterCommand>("api/commands/greeter")
    .ConfigureRemoteQueryProcessing<GetAgeQuery>("api/queries/get-age");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// FastEndpoints
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep =>
    {
        ep.AddErrorHandlerPostProcessor();
    };
});

app.MapGet("/demo/print-hello-world", async (ICommands commands) =>
{
    var cmd = new PrintHelloWorldCommand();
    await commands.RunAsync(cmd);

    return "Done";
});

app.Run();

public partial class Program
{
    // Expose the implicitly defined Program class to the test project
}
