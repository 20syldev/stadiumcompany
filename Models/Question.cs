namespace StadiumCompany.Models;

public enum AnswerType
{
    TRUE_FALSE,
    MULTIPLE_CHOICE
}

public class Question
{
    public int Id { get; set; }
    public int QuestionnaireId { get; set; }
    public int Number { get; set; }
    public string Label { get; set; } = string.Empty;
    public AnswerType AnswerType { get; set; }

    // Navigation properties
    public Questionnaire? Questionnaire { get; set; }
    public List<Answer> Answers { get; set; } = [];
}
