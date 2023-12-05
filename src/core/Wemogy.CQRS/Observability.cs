using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Wemogy.CQRS;

public static class Observability
{
    // Define a default ActivitySource
    public static readonly ActivitySource DefaultActivities = new ActivitySource("Wemogy.CQRS");

    // Define a default Meter with name and version
    public static readonly Meter Meter = new ("Wemogy.CQRS", typeof(Observability).Assembly.GetName().Version?.ToString() ?? "0.0.0");
}
