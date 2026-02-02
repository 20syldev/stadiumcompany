namespace StadiumCompany.Models;

public class Theme
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;

    public override string ToString() => Label;
}
