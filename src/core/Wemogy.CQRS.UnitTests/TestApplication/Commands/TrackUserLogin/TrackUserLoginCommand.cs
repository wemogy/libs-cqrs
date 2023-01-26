using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;

public class TrackUserLoginCommand : ICommand
{
    public string UserId { get; }

    public TrackUserLoginCommand(string userId)
    {
        UserId = userId;
    }
}
