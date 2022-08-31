using System;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.TrackUserLogin;

public class TrackUserLoginCommand : ICommand
{
    public Guid UserId { get; set; }
}
