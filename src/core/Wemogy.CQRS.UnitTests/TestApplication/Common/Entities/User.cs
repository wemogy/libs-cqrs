namespace Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

public class User
{
    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public User()
    {
        Firstname = string.Empty;
        Lastname = string.Empty;
    }
}
