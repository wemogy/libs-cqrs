using System;
using Bogus;
using Wemogy.Infrastructure.Database.Core.Abstractions;
using Wemogy.Infrastructure.Database.Core.CustomAttributes;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;

public class User : EntityBase
{
    public string Firstname { get; set; }

    public string Lastname { get; set; }

    [PartitionKey]
    public Guid TenantId { get; set; }

    public User()
    {
        Firstname = string.Empty;
        Lastname = string.Empty;
        TenantId = Guid.Empty;
    }

    public static Faker<User> Faker
    {
        get
        {
            return new Faker<User>()
                .RuleFor(x => x.TenantId, f => f.Random.Guid())
                .RuleFor(x => x.Firstname, f => f.Name.FirstName())
                .RuleFor(x => x.Lastname, f => f.Name.LastName());
        }
    }
}
