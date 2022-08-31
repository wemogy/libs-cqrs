using System;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;

public class CreateUserCommand : ICommand<User>
{
    public Guid Id { get; set; }

    public string Firstname { get; set; }

    public CreateUserCommand()
    {
        Firstname = string.Empty;
    }
}
