using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.Hangfire.Services
{
    public class HangfireRecurringJobService : IRecurringJobService
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public HangfireRecurringJobService(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public Task AddOrUpdateAsync(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
        {
            _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);
            return Task.CompletedTask;
        }

        public Task TriggerAsync(string recurringJobId)
        {
            _recurringJobManager.Trigger(recurringJobId);
            return Task.CompletedTask;
        }

        public Task RemoveIfExistsAsync(string recurringJobId)
        {
            _recurringJobManager.RemoveIfExists(recurringJobId);
            return Task.CompletedTask;
        }
    }
}
