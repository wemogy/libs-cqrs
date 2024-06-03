using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.Core.Extensions;
using Wemogy.Core.Monitoring;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Mediators;
using Wemogy.CQRS.Commands.Registries;
using Wemogy.CQRS.Commands.Resolvers;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions;
using Wemogy.CQRS.Health;
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
        Assembly? assembly = null,
        Dictionary<Type, Type>? dependencies = null)
    {
        // Set assembly to the calling assembly if not specified
        if (assembly == null)
        {
            assembly = Assembly.GetCallingAssembly();
        }

        return serviceCollection.AddCQRS(
            new List<Assembly>
            {
                assembly
            },
            dependencies);
    }

    public static CQRSSetupEnvironment AddCQRS(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies,
        Dictionary<Type, Type>? dependencies = null)
    {
        // remove duplicates from assemblies, which can happen, if Assembly.GetCallingAssembly() and Assembly.GetExecutingAssembly() are both added
        assemblies = assemblies.Distinct().ToList();

        dependencies ??= new Dictionary<Type, Type>();
        serviceCollection.AddCommands(assemblies, dependencies);
        serviceCollection.AddQueries(assemblies);
        return new CQRSSetupEnvironment(serviceCollection);
    }

    private static void AddCommands(this IServiceCollection serviceCollection, List<Assembly> assemblies, Dictionary<Type, Type> dependencies)
    {
        var commandTypes = assemblies.GetClassTypesWhichImplementInterface(typeof(ICommand<>));
        commandTypes.AddRange(assemblies.GetClassTypesWhichImplementInterface(typeof(ICommand)));

        foreach (var commandType in commandTypes)
        {
            if (!commandType.InheritsOrImplements(typeof(ICommand<>), out Type? genericCommandType) || genericCommandType == null)
            {
                if (!commandType.InheritsOrImplements(typeof(ICommand), out genericCommandType) || genericCommandType == null)
                {
                    throw new Exception("Command type must inherit from ICommand<> or ICommand");
                }
            }

            var resultType = genericCommandType.GenericTypeArguments.ElementAtOrDefault(0);

            // pre-processing
            serviceCollection.AddPreProcessing(assemblies, commandType);

            if (resultType == null)
            {
                // command handler
                serviceCollection.AddScopedGenericTypeWithImplementationFromAssembly(
                    assemblies,
                    typeof(ICommandHandler<>),
                    commandType);

                // command runners
                serviceCollection.AddCommandRunners(commandType);

                // post-processing
                serviceCollection.AddPostProcessing(assemblies, commandType);
            }
            else
            {
                // command handler
                serviceCollection.AddScopedGenericTypeWithImplementationFromAssembly(
                    assemblies,
                    typeof(ICommandHandler<,>),
                    commandType,
                    resultType);

                // command runners
                serviceCollection.AddCommandRunners(commandType, resultType);

                // post-processing
                serviceCollection.AddPostProcessing(assemblies, commandType, resultType);
            }
        }

        // ScheduledCommandDependencyResolver
        serviceCollection.AddSingleton(
            new ScheduledCommandDependencies(dependencies));
        serviceCollection.AddScoped<IScheduledCommandDependencyResolver>(
            provider =>
            new ScheduledCommandDependencyResolver(provider, dependencies));

        // Add ICommands mediator
        serviceCollection.AddScoped<ICommands, CommandsMediator>();

        // Add Registries
        serviceCollection.AddSingleton<CommandRunnerRegistry>();
        serviceCollection.AddSingleton<ScheduledCommandRunnerRegistry>();
        serviceCollection.AddSingleton<RecurringCommandRunnerRegistry>();
    }

    private static void AddQueries(this IServiceCollection serviceCollection, List<Assembly> assemblies)
    {
        var queryTypes = assemblies.GetClassTypesWhichImplementInterface(typeof(IQuery<>));

        foreach (var queryType in queryTypes)
        {
            if (!queryType.InheritsOrImplements(typeof(IQuery<>), out Type? genericQueryType) || genericQueryType == null)
            {
                throw new Exception("Query type must inherit from IQuery<>");
            }

            var resultType = genericQueryType.GenericTypeArguments[0];

            // validators
            serviceCollection.AddImplementationCollection(assemblies, queryType, typeof(IQueryValidator<>));

            // authorization
            serviceCollection.AddImplementationCollection(assemblies, queryType, typeof(IQueryAuthorization<>));

            // handlers
            var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, resultType);
            var queryHandlers = assemblies.GetClassTypesWhichImplementInterface(queryHandlerType);
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
        serviceCollection.AddSingleton<QueryRunnerRegistry>();
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

    private static void AddImplementation(
        this IServiceCollection serviceCollection,
        Type genericImplementationType,
        Type commandType)
    {
        var implementationType = genericImplementationType.MakeGenericType(commandType);
        serviceCollection.AddScoped(implementationType);
    }

    private static void AddPreProcessing(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies,
        Type commandType)
    {
        // validators
        serviceCollection.AddImplementationCollection(assemblies, commandType, typeof(ICommandValidator<>));

        // authorization
        serviceCollection.AddImplementationCollection(assemblies, commandType, typeof(ICommandAuthorization<>));

        // pre-processors
        serviceCollection.AddImplementationCollection(assemblies, commandType, typeof(ICommandPreProcessor<>));

        // PreProcessingRunner
        serviceCollection.AddImplementation(typeof(PreProcessingRunner<>), commandType);
    }

    private static void AddPostProcessing(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies,
        Type commandType,
        Type resultType)
    {
        // validators
        serviceCollection.AddImplementationCollection(
            assemblies,
            commandType,
            resultType,
            typeof(ICommandPostProcessor<,>));

        // PostProcessingRunner
        serviceCollection.AddImplementation(typeof(PostProcessingRunner<,>), commandType, resultType);
    }

    private static void AddPostProcessing(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies,
        Type commandType)
    {
        // validators
        serviceCollection.AddImplementationCollection(
            assemblies,
            commandType,
            typeof(ICommandPostProcessor<>));

        // PostProcessingRunner
        serviceCollection.AddImplementation(typeof(VoidPostProcessingRunner<>), commandType);
    }

    private static void AddCommandRunners(
        this IServiceCollection serviceCollection,
        Type commandType,
        Type resultType)
    {
        // command runner
        serviceCollection.AddScopedGenericType(
            typeof(CommandRunner<,>),
            commandType,
            resultType);

        // scheduled command runner
        var scheduledCommandRunnerType = typeof(ScheduledCommandRunner<,>).MakeGenericType(commandType, resultType);
        serviceCollection.AddScoped(scheduledCommandRunnerType);
        serviceCollection.AddScoped(
            typeof(IScheduledCommandRunner<>).MakeGenericType(commandType),
            scheduledCommandRunnerType);

        // recurring command runner
        serviceCollection.AddScopedGenericType(
            typeof(RecurringCommandRunner<,>),
            commandType,
            resultType);
    }

    private static void AddCommandRunners(
        this IServiceCollection serviceCollection,
        Type commandType)
    {
        // command runner
        serviceCollection.AddScopedGenericType(
            typeof(VoidCommandRunner<>),
            commandType);

        // scheduled command runner
        var scheduledCommandRunnerType = typeof(VoidScheduledCommandRunner<>).MakeGenericType(commandType);
        serviceCollection.AddScoped(scheduledCommandRunnerType);
        serviceCollection.AddScoped(
            typeof(IScheduledCommandRunner<>).MakeGenericType(commandType),
            scheduledCommandRunnerType);

        // recurring command runner
        serviceCollection.AddScopedGenericType(
            typeof(VoidRecurringCommandRunner<>),
            commandType);
    }

    private static void AddImplementationCollection(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies,
        Type commandType,
        Type genericInterfaceType)
    {
        var interfaceType = genericInterfaceType.MakeGenericType(commandType);
        var implementationTypes = assemblies.GetClassTypesWhichImplementInterface(interfaceType);
        var implementationCollectionType = typeof(List<>).MakeGenericType(interfaceType);
        serviceCollection.AddScoped(
            implementationCollectionType,
            serviceProvider =>
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
        List<Assembly> assemblies,
        Type commandType,
        Type resultType,
        Type genericInterfaceType)
    {
        var interfaceType = genericInterfaceType.MakeGenericType(commandType, resultType);
        var implementationTypes = assemblies.GetClassTypesWhichImplementInterface(interfaceType);
        var implementationCollectionType = typeof(List<>).MakeGenericType(interfaceType);
        serviceCollection.AddScoped(
            implementationCollectionType,
            serviceProvider =>
        {
            var implementationInstances =
                implementationTypes
                    .Select(x => ActivatorUtilities.CreateInstance(serviceProvider, x))
                    .ToListOfType(implementationCollectionType);
            return implementationInstances;
        });
    }

    /// <summary>
    /// Adds the ActivitySource and Meter of the CQRS library to the monitoring environment
    /// </summary>
    public static MonitoringEnvironment WithCQRS(this MonitoringEnvironment monitoringEnvironment)
    {
        return monitoringEnvironment
            .WithActivitySource(Observability.DefaultActivities.Name)
            .WithMeter(Observability.Meter.Name);
    }

    public static IHealthChecksBuilder AddDelayedProcessorCheck<TCommand>(this IHealthChecksBuilder healthChecksBuilder)
        where TCommand : ICommandBase
    {
        return healthChecksBuilder.AddCheck<DelayedCommandProcessorHealthCheck<TCommand>>(
            $"{typeof(TCommand).Name}DelayedProcessorHealthCheck");
    }
}
