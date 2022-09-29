using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IDelayedJobService
{
    Task<string> ScheduleAsync(Expression<Func<Task>> methodCall, TimeSpan delay);

    Task CancelAsync(string jobId);
}
