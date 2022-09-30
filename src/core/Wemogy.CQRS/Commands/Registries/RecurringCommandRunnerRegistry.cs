using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.Common.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Registries;

public class RecurringCommandRunnerRegistry : RegistryBase<Type, TypeMethodRegistryEntry>
{
    private readonly IServiceProvider _serviceProvider;

    public RecurringCommandRunnerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task ExecuteRecurringCommandRunnerAsync<TResult>(
        string recurringCommandId,
        ICommand<TResult> command,
        string cronExpression)
    {
        var recurringCommandRunnerEntry = GetRecurringCommandRunnerEntry(command);
        return ExecuteRecurringCommandRunnerAsync(
            recurringCommandRunnerEntry,
            recurringCommandId,
            command,
            cronExpression);
    }

    private TypeMethodRegistryEntry GetRecurringCommandRunnerEntry<TResult>(ICommand<TResult> command)
    {
        var commandType = command.GetType();
        var recurringCommandRunnerEntry = GetRegistryEntry(commandType);
        return recurringCommandRunnerEntry;
    }

    private Task ExecuteRecurringCommandRunnerAsync<TResult>(
        TypeMethodRegistryEntry recurringCommandRunnerEntry,
        string recurringCommandId,
        ICommand<TResult> command,
        string cronExpression)
    {
        object recurringCommandRunner = _serviceProvider.GetRequiredService(recurringCommandRunnerEntry.Type);
        var parameters = new object[]
        {
            recurringCommandId,
            command,
            cronExpression
        };
        dynamic res = recurringCommandRunnerEntry.Method.Invoke(recurringCommandRunner, parameters);
        return res;
    }

    protected override TypeMethodRegistryEntry InitializeEntry(Type commandType)
    {
        commandType.InheritsOrImplements(typeof(ICommand<>), out var resultType);
        var recurringCommandRunnerType =
            typeof(RecurringCommandRunner<,>).MakeGenericType(commandType, resultType?.GenericTypeArguments[0]);
        var runAsyncMethod = recurringCommandRunnerType.GetMethods().First(x => x.Name == "ScheduleAsync");
        return new TypeMethodRegistryEntry(recurringCommandRunnerType, runAsyncMethod);
    }
}
