namespace StadiumCompany.Models;

public class UserPreferences
{
    public int UserId { get; set; }
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "fr";
}
