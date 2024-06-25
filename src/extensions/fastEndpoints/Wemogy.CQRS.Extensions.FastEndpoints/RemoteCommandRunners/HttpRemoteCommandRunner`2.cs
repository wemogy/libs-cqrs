using System.Text.Json;
using RestSharp;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Extensions.FastEndpoints.RemoteCommandRunners;

public class HttpRemoteCommandRunner<TCommand, TResult> : IRemoteCommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly RestClient _restClient;

    /// <summary>
    /// This is the sub-path of the client base path
    /// </summary>
    private readonly string _urlPath;

    public HttpRemoteCommandRunner(RestClient restClient, string urlPath)
    {
        _restClient = restClient;
        _urlPath = urlPath;
    }

    public async Task<TResult> RunAsync(CommandRequest<TCommand> command)
    {
        // ToDo: Get configuration for the TCommand

        // ToDo: Http call to the remote service
        var request = new RestRequest(_urlPath)
            .AddJsonBody(command);

        var response = await _restClient.PostAsync(request);

        if (!response.IsSuccessful)
        {
            throw new Exception($"Failed to run command {command.Command.GetType().Name}");
        }

        if (response.Content == null)
        {
            return default!;
        }

        return JsonSerializer.Deserialize<TResult>(response.Content) !;
    }
}
