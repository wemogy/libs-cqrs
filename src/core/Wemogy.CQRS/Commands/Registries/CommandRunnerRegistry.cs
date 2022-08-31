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

public class CommandRunnerRegistry : RegistryBase<Type, TypeMethodRegistryEntry>
{
    private readonly IServiceProvider _serviceProvider;

    public CommandRunnerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> ExecuteCommandRunnerAsync<TResult>(ICommand<TResult> command)
    {
        var commandRunnerEntry = GetCommandRunnerEntry(command);
        return ExecuteCommandRunnerAsync(commandRunnerEntry, command);
    }

    private TypeMethodRegistryEntry GetCommandRunnerEntry<TResult>(ICommand<TResult> command)
    {
        var commandType = command.GetType();
        var commandRunnerEntry = GetRegistryEntry(commandType);
        return commandRunnerEntry;
    }

    private Task<TResult> ExecuteCommandRunnerAsync<TResult>(TypeMethodRegistryEntry commandRunnerEntry, ICommand<TResult> command)
    {
        object commandRunner = _serviceProvider.GetRequiredService(commandRunnerEntry.Type);
        dynamic res = commandRunnerEntry.Method.Invoke(commandRunner, new object[] { command });
        return res;
    }

    protected override TypeMethodRegistryEntry InitializeEntry(Type commandType)
    {
        commandType.InheritsOrImplements(typeof(ICommand<>), out var resultType);
        var commandRunnerType =
            typeof(CommandRunner<,>).MakeGenericType(commandType, resultType?.GenericTypeArguments[0]);
        var runAsyncMethod = commandRunnerType.GetMethods().First(x => x.Name == "RunAsync");
        return new TypeMethodRegistryEntry(commandRunnerType, runAsyncMethod);
    }
}
