using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.States;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.Hangfire.Services
{
    public class HangfireDelayedJobService : IDelayedJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireDelayedJobService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task<string> ScheduleAsync(Expression<Func<Task>> methodCall, TimeSpan delay)
        {
            var jobId = _backgroundJobClient.Create(methodCall, new ScheduledState(delay));
            return Task.FromResult(jobId);
        }

        public Task CancelAsync(string jobId)
        {
            _backgroundJobClient.ChangeState(jobId, new DeletedState());
            return Task.CompletedTask;
        }
    }
}
