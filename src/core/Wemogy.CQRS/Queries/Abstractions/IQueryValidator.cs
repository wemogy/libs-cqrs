namespace Wemogy.CQRS.Queries.Abstractions;

public interface IQueryValidator<in TQuery>
    where TQuery : IQueryBase
{
    void Validate(TQuery query);
}
