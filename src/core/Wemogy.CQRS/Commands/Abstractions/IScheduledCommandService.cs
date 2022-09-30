using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IScheduledCommandService
{
    Task<string> ScheduleAsync(Expression<Func<Task>> methodCall, TimeSpan delay);

    Task DeleteAsync(string jobId);
}
