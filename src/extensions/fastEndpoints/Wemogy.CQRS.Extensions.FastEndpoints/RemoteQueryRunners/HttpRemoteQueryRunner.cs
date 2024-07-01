using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.Common;
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
        // ToDo: Get configuration for the TCommand

        // ToDo: Http call to the remote service
        var request = new RestRequest(_urlPath)
            .AddJsonBody(query);

        var response = await _restClient.PostAsync(request, cancellationToken: cancellationToken);

        if (!response.IsSuccessful)
        {
            throw new Exception($"Failed to run query {query.Query.GetType().Name}");
        }

        if (response.Content == null)
        {
            return default!;
        }

        return JsonSerializer.Deserialize<TResult>(response.Content, JsonOptions.JsonSerializerOptions) !;
    }
}
