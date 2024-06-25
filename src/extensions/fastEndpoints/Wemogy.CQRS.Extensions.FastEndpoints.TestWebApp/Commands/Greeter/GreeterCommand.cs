using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.Greeter;

public class GreeterCommand : ICommand<string>
{
    public string Name { get; }

    public GreeterCommand(string name)
    {
        Name = name;
    }
}
