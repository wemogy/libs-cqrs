using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Common;

public static class JsonOptions
{
    public static JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}
