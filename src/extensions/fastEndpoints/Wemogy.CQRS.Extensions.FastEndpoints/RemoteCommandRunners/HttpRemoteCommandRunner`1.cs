using RestSharp;
using Wemogy.Core.Extensions;
using Wemogy.Core.Json.ExceptionInformation;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.RemoteCommandRunners;

public class HttpRemoteCommandRunner<TCommand> : IRemoteCommandRunner<TCommand>
    where TCommand : ICommandBase
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

    public async Task RunAsync(CommandRequest<TCommand> command)
    {
        // Http call to the remote service
        var request = new RestRequest(_urlPath)
            .AddJsonBody(command);

        try
        {
            var response = await _restClient.ExecutePostAsync(request);

            if (!response.IsSuccessful)
            {
                if (response.Headers == null || !response.Headers.HasJsonTypeHeader<ExceptionInformation>())
                {
                    throw response.ErrorException ?? new Exception(response.Content);
                }

                // ToDo: Handle the exception information
                var exceptionInformation = response.Content?.FromJson<ExceptionInformation>();
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
