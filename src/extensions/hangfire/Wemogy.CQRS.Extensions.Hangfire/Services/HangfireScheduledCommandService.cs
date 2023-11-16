using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.States;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions.Hangfire.Services
{
    public class HangfireScheduledCommandService : IScheduledCommandService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireScheduledCommandService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task<string> ScheduleAsync<TCommand>(
            IScheduledCommandRunner<TCommand> scheduledCommandRunner,
            ScheduledCommand<TCommand> scheduledCommand,
            TimeSpan delay)
            where TCommand : notnull
        {
            string jobId;
            Expression<Func<Task>> methodCall = () => scheduledCommandRunner.RunAsync(scheduledCommand);

            if (delay == TimeSpan.Zero)
            {
                jobId = _backgroundJobClient.Enqueue(methodCall);
            }
            else
            {
                jobId = _backgroundJobClient.Create(methodCall, new ScheduledState(delay));
            }

            return Task.FromResult(jobId);
        }

        public Task DeleteAsync<TCommand>(string jobId)
            where TCommand : ICommandBase
        {
            _backgroundJobClient.ChangeState(jobId, new DeletedState());
            return Task.CompletedTask;
        }
    }
}
