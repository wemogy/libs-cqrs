using FluentValidation;

namespace Wemogy.CQRS.Queries.Abstractions;

public abstract class FluentValidationQueryValidator<TQuery> : AbstractValidator<TQuery>, IQueryValidator<TQuery>
    where TQuery : IQueryBase
{
    public new void Validate(TQuery query)
    {
        this.ValidateAndThrow(query);
    }
}
