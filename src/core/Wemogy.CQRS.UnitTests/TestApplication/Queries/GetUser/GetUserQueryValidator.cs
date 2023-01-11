using FluentValidation;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQueryValidator : FluentValidationQueryValidator<GetUserQuery>, IQueryValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id).Empty();
    }
}
