using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using RestSharp;
using Wemogy.Core.Errors;
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
    private readonly IRestClient _restClient;

    /// <summary>
    /// This is the sub-path of the client base path
    /// </summary>
    private readonly string _urlPath;
    private readonly IAsyncPolicy<RestResponse> _retryPolicy;

    public HttpRemoteCommandRunner(IRestClient restClient, string urlPath)
    {
        _restClient = restClient;
        _urlPath = urlPath;
        var retryCount = 3;
        var delay = Backoff.ExponentialBackoff(
            TimeSpan.FromMilliseconds(100),
            retryCount);
        _retryPolicy = Policy
            .HandleResult<RestResponse>(x => !x.IsSuccessful)
            .WaitAndRetryAsync(delay);
    }

    public async Task RunAsync(CommandRequest<TCommand> command)
    {
        // Http call to the remote service
        var request = new RestRequest(_urlPath)
            .AddJsonBody(command);

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() => _restClient.ExecutePostAsync(request));

            if (!response.IsSuccessful)
            {
                if (response.Headers == null || !response.Headers.HasJsonTypeHeader<ExceptionInformation>())
                {
                    throw response.ErrorException ?? new Exception(response.Content);
                }

                var exceptionInformation = response.Content?.FromJson<ExceptionInformation>();

                if (exceptionInformation == null)
                {
                    throw Error.Unexpected(
                        "ExceptionInformationMissing",
                        "The response from the remote service did not contain any exception information.");
                }

                var exception = exceptionInformation.ToException();
                throw exception;
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
