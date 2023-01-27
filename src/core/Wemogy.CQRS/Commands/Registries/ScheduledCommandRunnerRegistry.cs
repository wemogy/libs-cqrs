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

public class ScheduledCommandRunnerRegistry : RegistryBase<Type, TypeMethodRegistryEntry>
{
    private readonly IServiceProvider _serviceProvider;

    public ScheduledCommandRunnerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<string> ExecuteScheduledCommandRunnerAsync(
        ICommandBase command,
        TimeSpan delay)
    {
        var scheduledCommandRunnerEntry = GetScheduledCommandRunnerEntry(command);
        return ExecuteScheduledCommandRunnerAsync(
            scheduledCommandRunnerEntry,
            command,
            delay);
    }

    private TypeMethodRegistryEntry GetScheduledCommandRunnerEntry(ICommandBase command)
    {
        var commandType = command.GetType();
        var scheduledCommandRunnerEntry = GetRegistryEntry(commandType);
        return scheduledCommandRunnerEntry;
    }

    private Task<string> ExecuteScheduledCommandRunnerAsync(
        TypeMethodRegistryEntry scheduledCommandRunnerEntry,
        ICommandBase command,
        TimeSpan delay)
    {
        object scheduledCommandRunner = _serviceProvider.GetRequiredService(scheduledCommandRunnerEntry.Type);
        var parameters = new object[]
        {
            command,
            delay
        };
        dynamic res = scheduledCommandRunnerEntry.Method.Invoke(scheduledCommandRunner, parameters);
        return res;
    }

    protected override TypeMethodRegistryEntry InitializeEntry(Type commandType)
    {
        if (commandType.InheritsOrImplements(typeof(ICommand<>), out var resultType))
        {
            var scheduledCommandRunnerType =
                typeof(ScheduledCommandRunner<,>).MakeGenericType(commandType, resultType?.GenericTypeArguments[0]);
            var runAsyncMethod = scheduledCommandRunnerType.GetMethods().First(x => x.Name == "ScheduleAsync");
            return new TypeMethodRegistryEntry(scheduledCommandRunnerType, runAsyncMethod);
        }

        if (commandType.InheritsOrImplements(typeof(ICommand)))
        {
            var scheduledCommandRunnerType =
                typeof(VoidScheduledCommandRunner<>).MakeGenericType(commandType);
            var runAsyncMethod = scheduledCommandRunnerType.GetMethods().First(x => x.Name == "ScheduleAsync");
            return new TypeMethodRegistryEntry(scheduledCommandRunnerType, runAsyncMethod);
        }

        throw new ArgumentException($"Command type {commandType} is not a valid command type.");
    }
}
