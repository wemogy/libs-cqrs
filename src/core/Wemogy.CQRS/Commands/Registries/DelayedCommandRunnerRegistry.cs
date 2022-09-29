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

public class DelayedCommandRunnerRegistry : RegistryBase<Type, TypeMethodRegistryEntry>
{
    private readonly IServiceProvider _serviceProvider;

    public DelayedCommandRunnerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<string> ExecuteDelayedCommandRunnerAsync<TResult>(
        ICommand<TResult> command,
        TimeSpan delay)
    {
        var delayedCommandRunnerEntry = GetDelayedCommandRunnerEntry(command);
        return ExecuteDelayedCommandRunnerAsync(
            delayedCommandRunnerEntry,
            command,
            delay);
    }

    private TypeMethodRegistryEntry GetDelayedCommandRunnerEntry<TResult>(ICommand<TResult> command)
    {
        var commandType = command.GetType();
        var delayedCommandRunnerEntry = GetRegistryEntry(commandType);
        return delayedCommandRunnerEntry;
    }

    private Task<string> ExecuteDelayedCommandRunnerAsync<TResult>(
        TypeMethodRegistryEntry delayedCommandRunnerEntry,
        ICommand<TResult> command,
        TimeSpan delay)
    {
        object delayedCommandRunner = _serviceProvider.GetRequiredService(delayedCommandRunnerEntry.Type);
        var parameters = new object[]
        {
            command,
            delay
        };
        dynamic res = delayedCommandRunnerEntry.Method.Invoke(delayedCommandRunner, parameters);
        return res;
    }

    protected override TypeMethodRegistryEntry InitializeEntry(Type commandType)
    {
        commandType.InheritsOrImplements(typeof(ICommand<>), out var resultType);
        var delayedCommandRunnerType =
            typeof(DelayedCommandRunner<,>).MakeGenericType(commandType, resultType?.GenericTypeArguments[0]);
        var runAsyncMethod = delayedCommandRunnerType.GetMethods().First(x => x.Name == "ScheduleAsync");
        return new TypeMethodRegistryEntry(delayedCommandRunnerType, runAsyncMethod);
    }
}
