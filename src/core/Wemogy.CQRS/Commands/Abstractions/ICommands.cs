using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommands
{
    Task<TResult> RunAsync<TResult>(ICommand<TResult> command);

    Task<TResult> ScheduleDelayedAsync<TResult>(ICommand<TResult> command);
}
