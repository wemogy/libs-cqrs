using Microsoft.AspNetCore.Http;
using RestSharp;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

public static class JsonTypeHeaderExtensions
{
    private const string JsonTypeHeaderName = "X-JSON-TYPE";

    public static void AppendJsonTypeHeader<TJsonType>(this IHeaderDictionary headers)
    {
        headers.Append(JsonTypeHeaderName, nameof(TJsonType).ToLower());
    }

    public static bool HasJsonTypeHeader<TJsonType>(this IReadOnlyCollection<HeaderParameter> headers)
    {
        return headers.Any(h => h.Name == JsonTypeHeaderName && h.Value?.ToString() == nameof(TJsonType).ToLower());
    }
}
