using System.Text.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using RestSharp;
using Wemogy.Core.Errors;
using Wemogy.Core.Extensions;
using Wemogy.Core.Json.ExceptionInformation;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.Common;
using Wemogy.CQRS.Extensions.FastEndpoints.Extensions;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.RemoteQueryRunners;

public class HttpRemoteQueryRunner<TQuery, TResult> : IRemoteQueryRunner<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IRestClient _restClient;

    /// <summary>
    /// This is the sub-path of the client base path
    /// </summary>
    private readonly string _urlPath;
    private readonly IAsyncPolicy<RestResponse> _retryPolicy;

    public HttpRemoteQueryRunner(IRestClient restClient, string urlPath)
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

    public async Task<TResult> QueryAsync(QueryRequest<TQuery> query, CancellationToken cancellationToken)
    {
        var request = new RestRequest(_urlPath)
            .AddJsonBody(query);

        var response = await _retryPolicy.ExecuteAsync(
            ct => _restClient.PostAsync(request, cancellationToken: ct),
            cancellationToken);

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

        if (response.Content == null)
        {
            return default!;
        }

        return JsonSerializer.Deserialize<TResult>(response.Content, JsonOptions.JsonSerializerOptions) !;
    }
}
