using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StadiumCompany.Models;

namespace StadiumCompany.Services.PdfGenerator;

public class QuestionnairePdfDocument : IDocument
{
    private readonly Questionnaire _questionnaire;
    private readonly LocalizationManager _loc = LocalizationManager.Instance;

    private const string PrimaryColor = "#667eea";
    private const string CorrectColor = "#22c55e";
    private const string CorrectBgColor = "#f0fdf4";
    private const string TextColor = "#1e293b";
    private const string MutedColor = "#64748b";

    public QuestionnairePdfDocument(Questionnaire questionnaire)
    {
        _questionnaire = questionnaire;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(11).FontColor(TextColor));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text(_loc.T("pdf.header"))
                .FontSize(10)
                .FontColor(MutedColor)
                .LetterSpacing(0.1f);

            column.Item().PaddingTop(5).Text(_questionnaire.Name)
                .FontSize(22)
                .Bold()
                .FontColor(PrimaryColor);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(_loc.T("pdf.theme", _questionnaire.Theme?.Label ?? "N/A"))
                        .FontSize(12);
                    c.Item().Text(_loc.T("pdf.question_count", _questionnaire.Questions.Count))
                        .FontSize(12);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text(_loc.T("pdf.date", DateTime.Now.ToString("dd/MM/yyyy")))
                        .FontSize(10)
                        .FontColor(MutedColor);
                    c.Item().Text(_loc.T("pdf.document_label"))
                        .FontSize(10)
                        .FontColor(MutedColor);
                });
            });

            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(20).Column(column =>
        {
            foreach (var question in _questionnaire.Questions.OrderBy(q => q.Number))
            {
                column.Item().Element(c => ComposeQuestion(c, question));
                column.Item().PaddingVertical(10);
            }
        });
    }

    private void ComposeQuestion(IContainer container, Question question)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(40).Background(PrimaryColor).Padding(8)
                    .AlignCenter().AlignMiddle()
                    .Text($"{question.Number}")
                    .FontColor(Colors.White)
                    .Bold();

                row.RelativeItem().PaddingLeft(12).Column(c =>
                {
                    c.Item().Text(question.Label)
                        .FontSize(13)
                        .SemiBold();
                    c.Item().PaddingTop(3).Text(GetAnswerTypeLabel(question.AnswerType))
                        .FontSize(10)
                        .FontColor(MutedColor);
                });
            });

            if (question.Answers.Count > 0)
            {
                column.Item().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                            .Text(_loc.T("question.col_answer")).SemiBold().FontSize(10);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                            .AlignCenter().Text(_loc.T("question.col_correct")).SemiBold().FontSize(10);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                            .AlignCenter().Text(_loc.T("question.col_weight")).SemiBold().FontSize(10);
                    });

                    foreach (var answer in question.Answers)
                    {
                        var bgColor = answer.IsCorrect ? CorrectBgColor : Colors.White.ToString();

                        table.Cell().Background(bgColor).BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2).Padding(8)
                            .Text(answer.Label).FontSize(10);

                        table.Cell().Background(bgColor).BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2).Padding(8)
                            .AlignCenter()
                            .Text(answer.IsCorrect ? _loc.T("common.yes") : _loc.T("common.no"))
                            .FontColor(answer.IsCorrect ? CorrectColor : MutedColor)
                            .FontSize(10);

                        table.Cell().Background(bgColor).BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2).Padding(8)
                            .AlignCenter()
                            .Text($"{answer.Weight}")
                            .FontSize(10);
                    }
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Page ").FontSize(9).FontColor(MutedColor);
            text.CurrentPageNumber().FontSize(9).FontColor(MutedColor);
            text.Span(" / ").FontSize(9).FontColor(MutedColor);
            text.TotalPages().FontSize(9).FontColor(MutedColor);
        });
    }

    private string GetAnswerTypeLabel(AnswerType type)
    {
        return type switch
        {
            AnswerType.TRUE_FALSE => _loc.T("pdf.type_truefalse"),
            AnswerType.MULTIPLE_CHOICE => _loc.T("pdf.type_multiple"),
            _ => _loc.T("pdf.type_unknown")
        };
    }
}
