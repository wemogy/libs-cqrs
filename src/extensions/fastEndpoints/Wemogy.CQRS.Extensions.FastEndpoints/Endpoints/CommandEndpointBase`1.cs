using CaseExtensions;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Setup;
using ICommand = Wemogy.CQRS.Commands.Abstractions.ICommand;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;

public class CommandEndpointBase<TCommand> : Endpoint<CommandRequest<TCommand>>
    where TCommand : ICommand
{
    private bool _circularDependencyTolerationEnabled;

    protected void EnableCircularDependencyToleration()
    {
        _circularDependencyTolerationEnabled = true;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        var commandName = typeof(TCommand).Name.RemoveTrailingString("Command").ToKebabCase();
        Routes($"/api/commands/{commandName}");

        // ToDo: remove this
        AllowAnonymous();
    }

    public override async Task HandleAsync(CommandRequest<TCommand> req, CancellationToken ct)
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<CommandEndpointBase<TCommand>>>();
        var serviceCollection = HttpContext.RequestServices.GetRequiredService<CQRSSetupEnvironment>();
        var services = new ServiceCollection();

        foreach (var serviceDescriptor in serviceCollection.ServiceCollection)
        {
            // check, if serviceDescriptor is IRemoteCommandRunner<TCommand> to avoid circular dependency
            if (serviceDescriptor.ServiceType.IsGenericType &&
                serviceDescriptor.ServiceType.GetGenericTypeDefinition() == typeof(IRemoteCommandRunner<>) &&
                serviceDescriptor.ServiceType.GetGenericArguments()[0] == typeof(TCommand))
            {
                if (_circularDependencyTolerationEnabled)
                {
                    logger.LogWarning(
                        "Circular dependency detected. Skipping IRemoteCommandRunner<{CommandName}>",
                        typeof(TCommand).Name);
                    continue;
                }

                logger.LogError(
                    "Circular dependency detected. IRemoteCommandRunner<{CommandName}> is not allowed",
                    typeof(TCommand).Name);
            }

            services.Add(serviceDescriptor);
        }

        services.AddCommandQueryDependencies(req.Dependencies);

        var serviceProvider = services.BuildServiceProvider();
        var commands = serviceProvider.GetRequiredService<ICommands>();

        await commands.RunAsync(req.Command);

        await SendOkAsync(ct);
    }
}
