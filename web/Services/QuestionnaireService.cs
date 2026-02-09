using System.Net.Http.Json;
using StadiumCompany.Models;

namespace StadiumCompany.Web.Services;

public class QuestionnaireService
{
    private readonly HttpClient _httpClient;

    public QuestionnaireService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Questionnaire>?> GetByUserAsync(int userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Questionnaire>>($"api/questionnaire/user/{userId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Questionnaire>?> GetPublishedByOthersAsync(int currentUserId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Questionnaire>>($"api/questionnaire/published/others/{currentUserId}");
        }
        catch
        {
            return null;
        }
    }
}