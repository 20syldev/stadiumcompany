namespace StadiumCompany.Models;

public class Questionnaire
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ThemeId { get; set; }
    public int UserId { get; set; }
    public bool Published { get; set; }

    // Navigation properties
    public Theme? Theme { get; set; }
    public User? Owner { get; set; }
    public List<Question> Questions { get; set; } = new();

    // Can be set from SQL query or computed from Questions
    private int? _questionCount;
    public int QuestionCount
    {
        get => _questionCount ?? Questions.Count;
        set => _questionCount = value;
    }
}
