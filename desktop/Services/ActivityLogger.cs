using StadiumCompany.DAL;

namespace StadiumCompany.Services;

public static class ActivityLogger
{
    private static readonly ActivityLogRepository _repository = new();

    public static void Log(int? userId, string action, string? entityType = null, int? entityId = null, string? details = null)
    {
        try
        {
            _repository.Log(userId, action, entityType, entityId, details);
        }
        catch
        {
            // Never crash the app for logging failures
        }
    }
}
