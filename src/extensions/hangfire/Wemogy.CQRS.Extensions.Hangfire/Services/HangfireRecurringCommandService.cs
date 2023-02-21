using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.Hangfire.Services
{
    public class HangfireRecurringCommandService : IRecurringCommandService
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public HangfireRecurringCommandService(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public Task AddOrUpdateAsync(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
        {
            _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression, TimeZoneInfo.Utc);
            return Task.CompletedTask;
        }

        public Task RemoveIfExistsAsync(string recurringJobId)
        {
            _recurringJobManager.RemoveIfExists(recurringJobId);
            return Task.CompletedTask;
        }
    }
}
