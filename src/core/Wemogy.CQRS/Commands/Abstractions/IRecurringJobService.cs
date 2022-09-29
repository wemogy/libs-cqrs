using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IRecurringJobService
{
    Task AddOrUpdateAsync(
        string recurringJobId,
        Expression<Func<Task>> methodCall,
        string cronExpression);

    Task TriggerAsync(string recurringJobId);

    Task RemoveIfExistsAsync(string recurringJobId);
}
