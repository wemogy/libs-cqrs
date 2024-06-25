namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;

public class TestContext
{
    public string UserId { get; set; }

    public TestContext()
    {
        UserId = string.Empty;
    }
}
