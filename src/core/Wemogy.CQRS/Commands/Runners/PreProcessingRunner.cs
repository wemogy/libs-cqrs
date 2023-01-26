using System.Collections.Generic;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class PreProcessingRunner<TCommand>
    where TCommand : ICommandBase
{
    private readonly List<ICommandValidator<TCommand>> _commandValidators;
    private readonly List<ICommandAuthorization<TCommand>> _commandAuthorizations;
    private readonly List<ICommandPreProcessor<TCommand>> _commandPreProcessors;

    public PreProcessingRunner(
        List<ICommandValidator<TCommand>> commandValidators,
        List<ICommandAuthorization<TCommand>> commandAuthorizations,
        List<ICommandPreProcessor<TCommand>> commandPreProcessors)
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
            await commandValidator.ValidateAsync(command);
        }
    }

    private async Task RunAuthorizationProcessorsAsync(TCommand command)
    {
        foreach (var commandAuthorization in _commandAuthorizations)
        {
            await commandAuthorization.AuthorizeAsync(command);
        }
    }

    public async Task RunPreProcessorsAsync(TCommand command)
    {
        foreach (var commandPreProcessor in _commandPreProcessors)
        {
            await commandPreProcessor.ProcessAsync(command);
        }
    }
}
