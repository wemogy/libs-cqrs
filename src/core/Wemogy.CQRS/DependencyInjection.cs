using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wemogy.Core.Errors;
using Wemogy.Core.Extensions;
using Wemogy.Core.Monitoring;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Mediators;
using Wemogy.CQRS.Commands.Registries;
using Wemogy.CQRS.Commands.Runners;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions;
using Wemogy.CQRS.Health;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Queries.Mediators;
using Wemogy.CQRS.Queries.Registries;
using Wemogy.CQRS.Queries.Runners;
using Wemogy.CQRS.Resolvers;
using Wemogy.CQRS.Setup;
using Exception = System.Exception;

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
        serviceCollection.AddCommandQueryDependencies(dependencies);

        serviceCollection.AddCommands(assemblies);
        serviceCollection.AddQueries(assemblies);

        var setupEnvironment = new CQRSSetupEnvironment(serviceCollection);
        serviceCollection.TryAddSingleton(setupEnvironment);
        return setupEnvironment;
    }

    private static void AddCommandQueryDependencies(this IServiceCollection serviceCollection, Dictionary<Type, Type> dependencies)
    {
        var commandQueryDependencies = new CommandQueryDependencies(dependencies);
        serviceCollection.TryAddScoped<ICommandQueryDependencyResolver>(
            provider =>
                new CommandQueryDependencyResolver(provider, commandQueryDependencies));
    }

    private static void AddCommands(this IServiceCollection serviceCollection, List<Assembly> assemblies)
    {
        var commandTypes = assemblies.GetClassTypesWhichImplementInterface(typeof(ICommand<>));
        commandTypes.AddRange(assemblies.GetClassTypesWhichImplementInterface(typeof(ICommand)));

        foreach (var commandType in commandTypes)
        {
            if (!commandType.InheritsOrImplements(typeof(ICommand<>), out Type? genericCommandType) || genericCommandType == null)
            {
                if (!commandType.InheritsOrImplements(typeof(ICommand), out genericCommandType) || genericCommandType == null)
                {
                    throw Error.Unexpected(
                        "InvalidCommandType",
                        $"The command type {commandType.FullName} must inherit from ICommand<> or ICommand");
                }
            }

            var resultType = genericCommandType.GenericTypeArguments.ElementAtOrDefault(0);

            if (resultType == null)
            {
                // command runners
                serviceCollection.AddCommandRunners(commandType);
            }
            else
            {
                // command runners
                serviceCollection.AddCommandRunners(commandType, resultType);
            }
        }

        // PreProcessing
        serviceCollection.AddPreProcessingServices(assemblies);

        // CommandHandlers
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(ICommandHandler<>), assemblies);
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(ICommandHandler<,>), assemblies);

        // Post-Processing
        serviceCollection.AddPostProcessingServices(assemblies);

        // Add ICommands mediator
        serviceCollection.TryAddScoped<ICommands, CommandsMediator>();

        // Add Registries
        serviceCollection.TryAddSingleton<CommandRunnerRegistry>();
        serviceCollection.TryAddSingleton<ScheduledCommandRunnerRegistry>();
        serviceCollection.TryAddSingleton<RecurringCommandRunnerRegistry>();
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

            // query runner
            var queryRunnerType = typeof(QueryRunner<,>).MakeGenericType(queryType, resultType);
            serviceCollection.AddScoped(queryRunnerType);
        }

        // IQueryValidator implementations
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(IQueryValidator<>), assemblies);

        // IQueryAuthorization implementations
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(IQueryAuthorization<>), assemblies);

        // IQueryHandlers implementations
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(IQueryHandler<,>), assemblies);

        // Add IQueries mediator
        serviceCollection.TryAddScoped<IQueries, QueriesMediator>();

        // Add QueryRunnerRegistry
        serviceCollection.TryAddSingleton<QueryRunnerRegistry>();
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

    private static void AddPreProcessingServices(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies)
    {
        // validators
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(ICommandValidator<>), assemblies);

        // authorization
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(ICommandAuthorization<>), assemblies);

        // pre-processors
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(typeof(ICommandPreProcessor<>), assemblies);
    }

    private static void AddPostProcessingServices(
        this IServiceCollection serviceCollection,
        List<Assembly> assemblies)
    {
        // Post-processing of commands without result
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandPostProcessor<>),
            assemblies);

        // Post-processing of commands with result
        serviceCollection.AddImplementationsOfGenericInterfaceScoped(
            typeof(ICommandPostProcessor<,>),
            assemblies);
    }

    private static void AddCommandRunners(
        this IServiceCollection serviceCollection,
        Type commandType,
        Type resultType)
    {
        // command runner
        serviceCollection.TryAddScopedGenericType(
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
        serviceCollection.TryAddScopedGenericType(
            typeof(RecurringCommandRunner<,>),
            commandType,
            resultType);

        // PreProcessingRunner
        serviceCollection.AddImplementation(typeof(PreProcessingRunner<>), commandType);

        // PostProcessingRunner
        serviceCollection.AddImplementation(typeof(PostProcessingRunner<,>), commandType, resultType);
    }

    private static void AddCommandRunners(
        this IServiceCollection serviceCollection,
        Type commandType)
    {
        // command runner
        serviceCollection.TryAddScopedGenericType(
            typeof(VoidCommandRunner<>),
            commandType);

        // scheduled command runner
        var scheduledCommandRunnerType = typeof(VoidScheduledCommandRunner<>).MakeGenericType(commandType);
        serviceCollection.AddScoped(scheduledCommandRunnerType);
        serviceCollection.AddScoped(
            typeof(IScheduledCommandRunner<>).MakeGenericType(commandType),
            scheduledCommandRunnerType);

        // recurring command runner
        serviceCollection.TryAddScopedGenericType(
            typeof(VoidRecurringCommandRunner<>),
            commandType);

        // PreProcessingRunner
        serviceCollection.AddImplementation(typeof(PreProcessingRunner<>), commandType);

        // PostProcessingRunner
        serviceCollection.AddImplementation(typeof(VoidPostProcessingRunner<>), commandType);
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
