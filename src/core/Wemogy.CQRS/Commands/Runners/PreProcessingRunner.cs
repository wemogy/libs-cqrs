using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class PreProcessingRunner<TCommand>
    where TCommand : ICommandBase
{
    private readonly IEnumerable<ICommandValidator<TCommand>> _commandValidators;
    private readonly IEnumerable<ICommandAuthorization<TCommand>> _commandAuthorizations;
    private readonly IEnumerable<ICommandPreProcessor<TCommand>> _commandPreProcessors;

    public PreProcessingRunner(
        IEnumerable<ICommandValidator<TCommand>> commandValidators,
        IEnumerable<ICommandAuthorization<TCommand>> commandAuthorizations,
        IEnumerable<ICommandPreProcessor<TCommand>> commandPreProcessors)
    {
        _commandValidators = commandValidators;
        _commandAuthorizations = commandAuthorizations;
        _commandPreProcessors = commandPreProcessors;
    }

    public async Task RunAsync(TCommand command)
    {
        await RunPreChecksAsync(command);
        await RunPreProcessorsAsync(command);
    }

    public async Task RunPreChecksAsync(TCommand command)
    {
        await RunValidationProcessorsAsync(command);
        await RunAuthorizationProcessorsAsync(command);
    }

    private async Task RunValidationProcessorsAsync(TCommand command)
    {
        foreach (var commandValidator in _commandValidators)
        {
            using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Validation");
            await commandValidator.ValidateAsync(command);
        }
    }

    private async Task RunAuthorizationProcessorsAsync(TCommand command)
    {
        foreach (var commandAuthorization in _commandAuthorizations)
        {
            using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Authorization");
            await commandAuthorization.AuthorizeAsync(command);
        }
    }

    public async Task RunPreProcessorsAsync(TCommand command)
    {
        foreach (var commandPreProcessor in _commandPreProcessors)
        {
            using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} PreProcessor");
            await commandPreProcessor.ProcessAsync(command);
        }
    }
}
