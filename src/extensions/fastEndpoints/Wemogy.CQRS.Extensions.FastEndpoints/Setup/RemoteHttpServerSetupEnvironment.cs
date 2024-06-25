using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.FastEndpoints.RemoteCommandRunners;
using Wemogy.CQRS.Extensions.FastEndpoints.RemoteQueryRunners;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Setup;

public class RemoteHttpServerSetupEnvironment : CQRSSetupEnvironment
{
    private readonly RestClient _restClient;

    public RemoteHttpServerSetupEnvironment(
        CQRSSetupEnvironment cqrsSetupEnvironment,
        Uri baseUrl)
        : base(cqrsSetupEnvironment)
    {
        _restClient = new RestClient(baseUrl);
    }

    public RemoteHttpServerSetupEnvironment(
        CQRSSetupEnvironment cqrsSetupEnvironment,
        HttpClient httpClient)
        : base(cqrsSetupEnvironment)
    {
        _restClient = new RestClient(httpClient);
    }

    public RemoteHttpServerSetupEnvironment ConfigureRemoteCommandProcessing<TCommand>(string urlPath)
        where TCommand : ICommandBase
    {
        // check if TCommand is ICommand or ICommand<TResult>
        if (typeof(ICommand).IsAssignableFrom(typeof(TCommand)))
        {
            ServiceCollection.AddSingleton<IRemoteCommandRunner<TCommand>>(
                new HttpRemoteCommandRunner<TCommand>(_restClient, urlPath));
        }
        else
        {
            // invoke the RegisterRemoteCommandRunner method
            var method = GetType().GetMethod(nameof(RegisterRemoteCommandRunner), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericTypeArgument = typeof(TCommand).GetInterfaces().FirstOrDefault(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                ?.GetGenericArguments().FirstOrDefault();

            if (genericTypeArgument == null)
            {
                throw Error.Unexpected(
                    "GenericTypeArgumentNotFound",
                    "Could not determine the generic type argument of the ICommand<T> interface implemented by the command.");
            }

            var genericMethod = method!.MakeGenericMethod(typeof(TCommand), genericTypeArgument);
            genericMethod.Invoke(this, new object[] { urlPath });
        }

        return this;
    }

    public RemoteHttpServerSetupEnvironment ConfigureRemoteQueryProcessing<TQuery>(string urlPath)
        where TQuery : IQueryBase
    {
        // invoke the RegisterRemoteQueryRunner method
        var method = GetType().GetMethod(nameof(RegisterRemoteQueryRunner), BindingFlags.Instance | BindingFlags.NonPublic);
        var genericTypeArgument = typeof(TQuery).GetInterfaces().FirstOrDefault(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
            ?.GetGenericArguments().FirstOrDefault();

        if (genericTypeArgument == null)
        {
            throw Error.Unexpected(
                "GenericTypeArgumentNotFound",
                "Could not determine the generic type argument of the IQuery<T> interface implemented by the query.");
        }

        var genericMethod = method!.MakeGenericMethod(typeof(TQuery), genericTypeArgument);
        genericMethod.Invoke(this, new object[] { urlPath });

        return this;
    }

    private void RegisterRemoteCommandRunner<TCommand, TResult>(string urlPath)
        where TCommand : ICommand<TResult>
    {
        ServiceCollection.AddSingleton<IRemoteCommandRunner<TCommand, TResult>>(
            new HttpRemoteCommandRunner<TCommand, TResult>(_restClient, urlPath));
    }

    private void RegisterRemoteQueryRunner<TQuery, TResult>(string urlPath)
        where TQuery : IQuery<TResult>
    {
        ServiceCollection.AddSingleton<IRemoteQueryRunner<TQuery, TResult>>(
            new HttpRemoteQueryRunner<TQuery, TResult>(_restClient, urlPath));
    }
}
