namespace Wemogy.CQRS.Extensions.Hangfire.UnitTests.Testing.Models
{
    public class MyTestingContext
    {
        public string Name { get; set; }

        public MyTestingContext()
        {
            Name = string.Empty;
        }
    }
}
