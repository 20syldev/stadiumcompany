namespace StadiumCompany.Models;

// DTO for the statistics view: questionnaire count per theme
public class ThemeStat
{
    public int ThemeId { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public int QuestionnaireCount { get; set; }
}
