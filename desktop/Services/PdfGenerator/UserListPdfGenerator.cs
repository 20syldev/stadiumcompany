using QuestPDF.Fluent;
using StadiumCompany.Models;

namespace StadiumCompany.Services.PdfGenerator;

public static class UserListPdfGenerator
{
    public static void Generate(IList<User> users, string filePath)
    {
        var document = new UserListPdfDocument(users);
        document.GeneratePdf(filePath);
    }

    public static byte[] GenerateBytes(IList<User> users)
    {
        var document = new UserListPdfDocument(users);
        return document.GeneratePdf();
    }
}
