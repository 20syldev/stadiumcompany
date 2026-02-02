namespace StadiumCompany.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? FullName
    {
        get
        {
            var name = $"{FirstName} {LastName}".Trim();
            return string.IsNullOrEmpty(name) ? null : name;
        }
    }
}
