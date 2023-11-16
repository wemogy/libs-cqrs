using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Services
{
    public class AzureServiceBusRecurringCommandService : IRecurringCommandService
    {
        public Task AddOrUpdateAsync(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
        {
            throw new NotImplementedException("AzureServiceBus doesn't support recurring commands");
        }

        public Task RemoveIfExistsAsync(string recurringJobId)
        {
            throw new NotImplementedException("AzureServiceBus doesn't support recurring commands");
        }
    }
}
