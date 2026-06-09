using QuestPDF.Fluent;
using StadiumCompany.Models;

namespace StadiumCompany.Services.PdfGenerator;

public static class QuestionnairePdfGenerator
{
    public static void Generate(Questionnaire questionnaire, string filePath)
    {
        var document = new QuestionnairePdfDocument(questionnaire);
        document.GeneratePdf(filePath);
    }

    public static byte[] GenerateBytes(Questionnaire questionnaire)
    {
        var document = new QuestionnairePdfDocument(questionnaire);
        return document.GeneratePdf();
    }
}
