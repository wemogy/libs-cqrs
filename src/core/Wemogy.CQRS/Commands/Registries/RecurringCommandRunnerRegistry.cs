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
    public Task ExecuteRecurringCommandRunnerAsync(
        IServiceProvider serviceProvider,
        string recurringCommandId,
        ICommandBase command,
        string cronExpression)
    {
        var recurringCommandRunnerEntry = GetRecurringCommandRunnerEntry(command);
        return ExecuteRecurringCommandRunnerAsync(
            recurringCommandRunnerEntry,
            serviceProvider,
            recurringCommandId,
            command,
            cronExpression);
    }

    private TypeMethodRegistryEntry GetRecurringCommandRunnerEntry(ICommandBase command)
    {
        var commandType = command.GetType();
        var recurringCommandRunnerEntry = GetRegistryEntry(commandType);
        return recurringCommandRunnerEntry;
    }

    private Task ExecuteRecurringCommandRunnerAsync(
        TypeMethodRegistryEntry recurringCommandRunnerEntry,
        IServiceProvider serviceProvider,
        string recurringCommandId,
        ICommandBase command,
        string cronExpression)
    {
        object recurringCommandRunner = serviceProvider.GetRequiredService(recurringCommandRunnerEntry.Type);
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
        if (commandType.InheritsOrImplements(typeof(ICommand<>), out var resultType))
        {
            var recurringCommandRunnerType =
                typeof(RecurringCommandRunner<,>).MakeGenericType(commandType, resultType?.GenericTypeArguments[0]);
            var runAsyncMethod = recurringCommandRunnerType.GetMethods().First(x => x.Name == "ScheduleAsync");
            return new TypeMethodRegistryEntry(recurringCommandRunnerType, runAsyncMethod);
        }

        if (commandType.InheritsOrImplements(typeof(ICommand)))
        {
            var recurringCommandRunnerType =
                typeof(VoidRecurringCommandRunner<>).MakeGenericType(commandType);
            var runAsyncMethod = recurringCommandRunnerType.GetMethods().First(x => x.Name == "ScheduleAsync");
            return new TypeMethodRegistryEntry(recurringCommandRunnerType, runAsyncMethod);
        }

        throw new ArgumentException($"Command type {commandType} is not a valid command type.");
    }
}
