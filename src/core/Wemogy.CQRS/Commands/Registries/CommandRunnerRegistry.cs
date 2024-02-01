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
    public Task ExecuteCommandRunnerAsync(IServiceProvider serviceProvider, ICommand command)
    {
        var commandRunnerEntry = GetCommandRunnerEntry(command);
        return ExecuteCommandRunnerAsync(serviceProvider, commandRunnerEntry, command);
    }

    private TypeMethodRegistryEntry GetCommandRunnerEntry(ICommand command)
    {
        var commandType = command.GetType();
        var commandRunnerEntry = GetRegistryEntry(commandType);
        return commandRunnerEntry;
    }

    private Task ExecuteCommandRunnerAsync(IServiceProvider serviceProvider, TypeMethodRegistryEntry commandRunnerEntry, ICommand command)
    {
        object commandRunner = serviceProvider.GetRequiredService(commandRunnerEntry.Type);
        dynamic res = commandRunnerEntry.Method.Invoke(commandRunner, new object[] { command });
        return res;
    }

    public Task<TResult> ExecuteCommandRunnerAsync<TResult>(IServiceProvider serviceProvider, ICommand<TResult> command)
    {
        var commandRunnerEntry = GetCommandRunnerEntry(command);
        return ExecuteCommandRunnerAsync(serviceProvider, commandRunnerEntry, command);
    }

    private TypeMethodRegistryEntry GetCommandRunnerEntry<TResult>(ICommand<TResult> command)
    {
        var commandType = command.GetType();
        var commandRunnerEntry = GetRegistryEntry(commandType);
        return commandRunnerEntry;
    }

    private Task<TResult> ExecuteCommandRunnerAsync<TResult>(
        IServiceProvider serviceProvider,
        TypeMethodRegistryEntry commandRunnerEntry,
        ICommand<TResult> command)
    {
        object commandRunner = serviceProvider.GetRequiredService(commandRunnerEntry.Type);
        dynamic res = commandRunnerEntry.Method.Invoke(commandRunner, new object[] { command });
        return res;
    }

    protected override TypeMethodRegistryEntry InitializeEntry(Type commandType)
    {
        if (commandType.InheritsOrImplements(typeof(ICommand<>), out var resultType))
        {
            var commandRunnerType =
                typeof(CommandRunner<,>).MakeGenericType(commandType, resultType?.GenericTypeArguments[0]);
            var runAsyncMethod = commandRunnerType.GetMethods().First(x => x.Name == "RunAsync");
            return new TypeMethodRegistryEntry(commandRunnerType, runAsyncMethod);
        }

        if (commandType.InheritsOrImplements(typeof(ICommand)))
        {
            var commandRunnerType =
                typeof(VoidCommandRunner<>).MakeGenericType(commandType);
            var runAsyncMethod = commandRunnerType.GetMethods().First(x => x.Name == "RunAsync");
            return new TypeMethodRegistryEntry(commandRunnerType, runAsyncMethod);
        }

        throw new ArgumentException($"Command type {commandType} is not a valid command type.");
    }
}
