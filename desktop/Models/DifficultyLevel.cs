namespace StadiumCompany.Models;

public class DifficultyLevel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Position { get; set; }

    public override string ToString() => Label;
}
