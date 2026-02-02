namespace StadiumCompany.Models;

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public decimal Weight { get; set; }

    // Navigation property
    public Question? Question { get; set; }
}
