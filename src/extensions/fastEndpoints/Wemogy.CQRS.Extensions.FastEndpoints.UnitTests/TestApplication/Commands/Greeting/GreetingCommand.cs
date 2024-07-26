using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.Greeting;

public class GreetingCommand : ICommand<string>
{
    public string Name { get; }

    public GreetingCommand(string name)
    {
        Name = name;
    }
}
