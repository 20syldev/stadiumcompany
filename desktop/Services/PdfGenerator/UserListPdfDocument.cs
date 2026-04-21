using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StadiumCompany.Models;

namespace StadiumCompany.Services.PdfGenerator;

public class UserListPdfDocument : IDocument
{
    private readonly IList<User> _users;
    private readonly LocalizationManager _loc = LocalizationManager.Instance;

    private const string PrimaryColor = "#667eea";
    private const string TextColor = "#1e293b";
    private const string MutedColor = "#64748b";
    private const string ArchivedColor = "#ef4444";
    private const string ActiveColor = "#22c55e";

    public UserListPdfDocument(IList<User> users)
    {
        _users = users;
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
            column.Item().Text(_loc.T("pdf.users_header"))
                .FontSize(10)
                .FontColor(MutedColor)
                .LetterSpacing(0.1f);

            column.Item().PaddingTop(5).Text(_loc.T("pdf.header"))
                .FontSize(18)
                .Bold()
                .FontColor(PrimaryColor);

            column.Item().PaddingTop(6).Text(string.Format(_loc.T("pdf.users_date"), DateTime.Now.ToString("dd/MM/yyyy")))
                .FontSize(11)
                .FontColor(MutedColor);

            column.Item().PaddingTop(4).PaddingBottom(12)
                .BorderBottom(1)
                .BorderColor("#e2e8f0");
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);   // ID
                columns.RelativeColumn(3);    // Email
                columns.RelativeColumn(2);    // Name
                columns.RelativeColumn(1);    // Status
                columns.RelativeColumn(1.5f); // Created
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Background("#f1f5f9").Padding(8)
                    .Text(_loc.T("pdf.users_col_id")).Bold().FontSize(10).FontColor(MutedColor);
                header.Cell().Background("#f1f5f9").Padding(8)
                    .Text(_loc.T("pdf.users_col_email")).Bold().FontSize(10).FontColor(MutedColor);
                header.Cell().Background("#f1f5f9").Padding(8)
                    .Text(_loc.T("pdf.users_col_name")).Bold().FontSize(10).FontColor(MutedColor);
                header.Cell().Background("#f1f5f9").Padding(8)
                    .Text(_loc.T("pdf.users_col_status")).Bold().FontSize(10).FontColor(MutedColor);
                header.Cell().Background("#f1f5f9").Padding(8)
                    .Text(_loc.T("pdf.users_col_created")).Bold().FontSize(10).FontColor(MutedColor);
            });

            // Data rows
            for (int i = 0; i < _users.Count; i++)
            {
                var user = _users[i];
                var rowBg = i % 2 == 0 ? "#ffffff" : "#f8fafc";
                var statusText = user.IsArchived ? _loc.T("pdf.users_status_archived") : _loc.T("pdf.users_status_active");
                var statusColor = user.IsArchived ? ArchivedColor : ActiveColor;

                table.Cell().Background(rowBg).BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                    .Text(user.Id.ToString()).FontSize(10);
                table.Cell().Background(rowBg).BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                    .Text(user.Email).FontSize(10);
                table.Cell().Background(rowBg).BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                    .Text(user.FullName ?? "—").FontSize(10);
                table.Cell().Background(rowBg).BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                    .Text(statusText).FontSize(10).FontColor(statusColor);
                table.Cell().Background(rowBg).BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                    .Text(user.CreatedAt.ToString("dd/MM/yyyy")).FontSize(10);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("— ").FontColor(MutedColor).FontSize(9);
            text.CurrentPageNumber().FontColor(MutedColor).FontSize(9);
            text.Span(" / ").FontColor(MutedColor).FontSize(9);
            text.TotalPages().FontColor(MutedColor).FontSize(9);
            text.Span(" —").FontColor(MutedColor).FontSize(9);
        });
    }
}
