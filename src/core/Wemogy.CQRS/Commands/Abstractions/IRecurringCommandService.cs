using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IRecurringCommandService
{
    Task AddOrUpdateAsync(
        string recurringJobId,
        Expression<Func<Task>> methodCall,
        string cronExpression);

    Task RemoveIfExistsAsync(string recurringJobId);
}
