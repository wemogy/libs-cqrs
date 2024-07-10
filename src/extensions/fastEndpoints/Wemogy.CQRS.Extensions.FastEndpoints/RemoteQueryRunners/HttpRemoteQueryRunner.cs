using System.Text.Json;
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
    private readonly RestClient _restClient;

    /// <summary>
    /// This is the sub-path of the client base path
    /// </summary>
    private readonly string _urlPath;

    public HttpRemoteQueryRunner(RestClient restClient, string urlPath)
    {
        _restClient = restClient;
        _urlPath = urlPath;
    }

    public async Task<TResult> QueryAsync(QueryRequest<TQuery> query, CancellationToken cancellationToken)
    {
        var request = new RestRequest(_urlPath)
            .AddJsonBody(query);

        var response = await _restClient.PostAsync(request, cancellationToken: cancellationToken);

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
