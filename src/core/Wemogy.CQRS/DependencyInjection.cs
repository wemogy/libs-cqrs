using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Mediators;
using Wemogy.CQRS.Commands.Registries;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Queries.Mediators;
using Wemogy.CQRS.Queries.Registries;
using Wemogy.CQRS.Queries.Runners;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS;

public static class DependencyInjection
{
    public static CQRSSetupEnvironment AddCQRS(
        this IServiceCollection serviceCollection,
        Assembly? assembly = null)
    {
        // Set assembly to the calling assembly if not specified
        if (assembly == null)
        {
            assembly = Assembly.GetCallingAssembly();
        }

        return serviceCollection.AddCQRS(new List<Assembly>
        {
            assembly
        });
    }

    public static CQRSSetupEnvironment AddCQRS(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies)
    {
        serviceCollection.AddCommands(assemblies);
        serviceCollection.AddQueries(assemblies);
        return new CQRSSetupEnvironment(serviceCollection);
    }

    private static void AddCommands(this IServiceCollection serviceCollection, List<Assembly> assemblies)
    {
        var commandTypes = assemblies.GetClassTypesWhichImplementInterface(typeof(ICommand<>));

        foreach (var commandType in commandTypes)
        {
            var assembly = commandType.Assembly;
            if (!commandType.InheritsOrImplements(typeof(ICommand<>), out Type? genericCommandType) || genericCommandType == null)
            {
                throw new Exception("Command type must inherit from ICommand<>");
            }

            var resultType = genericCommandType.GenericTypeArguments[0];

            // pre-processing
            serviceCollection.AddPreProcessing(assembly, commandType, resultType);

            // handlers
            var commandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
            var commandHandlers = assembly.GetClassTypesWhichImplementInterface(commandHandlerType);
            if (commandHandlers.Count != 1)
            {
                throw new Exception(
                    $"There must be exactly one ICommandHandler registered for command type {commandType.FullName}");
            }

            var commandHandlerImplementationType = commandHandlers[0];
            serviceCollection.AddScoped(commandHandlerType, commandHandlerImplementationType);

            // command runner
            var commandRunnerType = typeof(CommandRunner<,>).MakeGenericType(commandType, resultType);
            serviceCollection.AddScoped(commandRunnerType);

            // delayed command runner
            var scheduledCommandRunnerType = typeof(ScheduledCommandRunner<,>).MakeGenericType(commandType, resultType);
            serviceCollection.AddScoped(scheduledCommandRunnerType);

            // recurring command runner
            var recurringCommandRunnerType = typeof(RecurringCommandRunner<,>).MakeGenericType(commandType, resultType);
            serviceCollection.AddScoped(recurringCommandRunnerType);

            // post-processing
            serviceCollection.AddPostProcessing(assembly, commandType, resultType);
        }

        // Add ICommands mediator
        serviceCollection.AddScoped<ICommands, CommandsMediator>();

        // Add Registries
        serviceCollection.AddScoped<CommandRunnerRegistry>();
        serviceCollection.AddScoped<ScheduledCommandRunnerRegistry>();
        serviceCollection.AddScoped<RecurringCommandRunnerRegistry>();
    }

    private static void AddQueries(this IServiceCollection serviceCollection, List<Assembly> assemblies)
    {
        var queryTypes = assemblies.GetClassTypesWhichImplementInterface(typeof(IQuery<>));

        foreach (var queryType in queryTypes)
        {
            var assembly = queryType.Assembly;
            if (!queryType.InheritsOrImplements(typeof(IQuery<>), out Type? genericQueryType) || genericQueryType == null)
            {
                throw new Exception("Query type must inherit from IQuery<>");
            }

            var resultType = genericQueryType.GenericTypeArguments[0];

            // handlers
            var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, resultType);
            var queryHandlers = assembly.GetClassTypesWhichImplementInterface(queryHandlerType);
            if (queryHandlers.Count != 1)
            {
                throw new Exception(
                    $"There must be exactly one IQueryHandler registered for query type {queryType.FullName}");
            }

            var queryHandlerImplementationType = queryHandlers[0];
            serviceCollection.AddScoped(queryHandlerType, queryHandlerImplementationType);

            // query runner
            var queryRunnerType = typeof(QueryRunner<,>).MakeGenericType(queryType, resultType);
            serviceCollection.AddScoped(queryRunnerType);
        }

        // Add IQueries mediator
        serviceCollection.AddScoped<IQueries, QueriesMediator>();

        // Add QueryRunnerRegistry
        serviceCollection.AddScoped<QueryRunnerRegistry>();
    }

    private static void AddImplementation(
        this IServiceCollection serviceCollection,
        Type genericImplementationType,
        Type commandType,
        Type resultType)
    {
        var implementationType = genericImplementationType.MakeGenericType(commandType, resultType);
        serviceCollection.AddScoped(implementationType);
    }

    private static void AddPreProcessing(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        Type commandType,
        Type resultType)
    {
        // validators
        serviceCollection.AddImplementationCollection(assembly, commandType, typeof(ICommandValidator<>));

        // authorization
        serviceCollection.AddImplementationCollection(assembly, commandType, typeof(ICommandAuthorization<>));

        // pre-processors
        serviceCollection.AddImplementationCollection(assembly, commandType, typeof(ICommandPreProcessor<>));

        // PreProcessingRunner
        serviceCollection.AddImplementation(typeof(PreProcessingRunner<,>), commandType, resultType);
    }

    private static void AddPostProcessing(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        Type commandType,
        Type resultType)
    {
        // validators
        serviceCollection.AddImplementationCollection(
            assembly,
            commandType,
            resultType,
            typeof(ICommandPostProcessor<,>));

        // PostProcessingRunner
        serviceCollection.AddImplementation(typeof(PostProcessingRunner<,>), commandType, resultType);
    }

    private static void AddImplementationCollection(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        Type commandType,
        Type genericInterfaceType)
    {
        var interfaceType = genericInterfaceType.MakeGenericType(commandType);
        var implementationTypes = assembly.GetClassTypesWhichImplementInterface(interfaceType);
        var implementationCollectionType = typeof(List<>).MakeGenericType(interfaceType);
        serviceCollection.AddScoped(implementationCollectionType, serviceProvider =>
        {
            var implementationInstances =
                implementationTypes
                    .Select(x => ActivatorUtilities.CreateInstance(serviceProvider, x))
                    .ToListOfType(implementationCollectionType);
            return implementationInstances;
        });
    }

    private static void AddImplementationCollection(
        this IServiceCollection serviceCollection,
        Assembly assembly,
        Type commandType,
        Type resultType,
        Type genericInterfaceType)
    {
        var interfaceType = genericInterfaceType.MakeGenericType(commandType, resultType);
        var implementationTypes = assembly.GetClassTypesWhichImplementInterface(interfaceType);
        var implementationCollectionType = typeof(List<>).MakeGenericType(interfaceType);
        serviceCollection.AddScoped(implementationCollectionType, serviceProvider =>
        {
            var implementationInstances =
                implementationTypes
                    .Select(x => ActivatorUtilities.CreateInstance(serviceProvider, x))
                    .ToListOfType(implementationCollectionType);
            return implementationInstances;
        });
    }
}
