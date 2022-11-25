using System;
using Bogus;
using Wemogy.Infrastructure.Database.Core.Abstractions;
using Wemogy.Infrastructure.Database.Core.Attributes;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;

public class User : EntityBase
{
    public string Firstname { get; set; }

    public string Lastname { get; set; }

    [PartitionKey]
    public string TenantId { get; set; }

    public User()
        : base(Guid.NewGuid().ToString())
    {
        Firstname = string.Empty;
        Lastname = string.Empty;
        TenantId = string.Empty;
    }

    public static Faker<User> Faker
    {
        get
        {
            return new Faker<User>()
                .RuleFor(x => x.CreatedAt, f => new DateTime(2022, 2, 3, 5, 6, 0, DateTimeKind.Utc))
                .RuleFor(x => x.UpdatedAt, f => new DateTime(2022, 2, 3, 5, 6, 0, DateTimeKind.Utc))
                .RuleFor(x => x.TenantId, f => f.Random.Guid().ToString())
                .RuleFor(x => x.Firstname, f => f.Name.FirstName())
                .RuleFor(x => x.Lastname, f => f.Name.LastName());
        }
    }
}
