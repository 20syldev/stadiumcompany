using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class ActivityLogRepository
{
    public List<ActivityLog> Search(string? query, string? action, DateTime? from, DateTime? to, int limit = 50, int offset = 0)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var sql = @"SELECT al.*, TRIM(CONCAT(u.first_name, ' ', u.last_name)) AS user_name
                    FROM activity_logs al
                    LEFT JOIN users u ON al.user_id = u.id
                    WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql += " AND (al.action ILIKE @query OR al.details ILIKE @query OR al.entity_type ILIKE @query)";
            parameters.Add(new NpgsqlParameter("@query", $"%{query}%"));
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            sql += " AND al.action = @action";
            parameters.Add(new NpgsqlParameter("@action", action));
        }
        if (from.HasValue)
        {
            sql += " AND al.created_at >= @from";
            parameters.Add(new NpgsqlParameter("@from", from.Value));
        }
        if (to.HasValue)
        {
            sql += " AND al.created_at <= @to";
            parameters.Add(new NpgsqlParameter("@to", to.Value));
        }

        sql += " ORDER BY al.id DESC LIMIT @limit OFFSET @offset";
        parameters.Add(new NpgsqlParameter("@limit", limit));
        parameters.Add(new NpgsqlParameter("@offset", offset));

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        var logs = new List<ActivityLog>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            logs.Add(MapActivityLog(reader));
        }
        return logs;
    }

    public int GetCount(string? query, string? action, DateTime? from, DateTime? to)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var sql = "SELECT COUNT(*) FROM activity_logs al WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql += " AND (al.action ILIKE @query OR al.details ILIKE @query OR al.entity_type ILIKE @query)";
            parameters.Add(new NpgsqlParameter("@query", $"%{query}%"));
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            sql += " AND al.action = @action";
            parameters.Add(new NpgsqlParameter("@action", action));
        }
        if (from.HasValue)
        {
            sql += " AND al.created_at >= @from";
            parameters.Add(new NpgsqlParameter("@from", from.Value));
        }
        if (to.HasValue)
        {
            sql += " AND al.created_at <= @to";
            parameters.Add(new NpgsqlParameter("@to", to.Value));
        }

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Log(int? userId, string action, string? entityType = null, int? entityId = null, string? details = null)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var sql = @"INSERT INTO activity_logs (user_id, action, entity_type, entity_id, details, source)
                    VALUES (@userId, @action, @entityType, @entityId, @details, 'desktop')";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId.HasValue ? userId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@action", action);
        command.Parameters.AddWithValue("@entityType", (object?)entityType ?? DBNull.Value);
        command.Parameters.AddWithValue("@entityId", entityId.HasValue ? entityId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@details", (object?)details ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    public List<ActivityLog> SearchLogins(DateTime? from, DateTime? to, int limit = 50, int offset = 0)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var sql = @"SELECT al.*, TRIM(CONCAT(u.first_name, ' ', u.last_name)) AS user_name,
                           u.email AS user_email
                    FROM activity_logs al
                    LEFT JOIN users u ON al.user_id = u.id
                    WHERE al.action = 'login'";
        var parameters = new List<NpgsqlParameter>();

        if (from.HasValue)
        {
            sql += " AND al.created_at >= @from";
            parameters.Add(new NpgsqlParameter("@from", from.Value));
        }
        if (to.HasValue)
        {
            sql += " AND al.created_at <= @to";
            parameters.Add(new NpgsqlParameter("@to", to.Value));
        }

        sql += " ORDER BY al.created_at DESC LIMIT @limit OFFSET @offset";
        parameters.Add(new NpgsqlParameter("@limit", limit));
        parameters.Add(new NpgsqlParameter("@offset", offset));

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        var logs = new List<ActivityLog>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var log = MapActivityLog(reader);
            log.UserEmail = reader.IsDBNull(reader.GetOrdinal("user_email"))
                ? null : reader.GetString(reader.GetOrdinal("user_email"));
            logs.Add(log);
        }
        return logs;
    }

    public int GetLoginCount(DateTime? from, DateTime? to)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var sql = "SELECT COUNT(*) FROM activity_logs al WHERE al.action = 'login'";
        var parameters = new List<NpgsqlParameter>();

        if (from.HasValue)
        {
            sql += " AND al.created_at >= @from";
            parameters.Add(new NpgsqlParameter("@from", from.Value));
        }
        if (to.HasValue)
        {
            sql += " AND al.created_at <= @to";
            parameters.Add(new NpgsqlParameter("@to", to.Value));
        }

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<string> GetDistinctActions()
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var sql = "SELECT DISTINCT action FROM activity_logs ORDER BY action";
        using var command = new NpgsqlCommand(sql, connection);

        var actions = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            actions.Add(reader.GetString(0));
        }
        return actions;
    }

    private static ActivityLog MapActivityLog(NpgsqlDataReader reader)
    {
        var userName = reader.IsDBNull(reader.GetOrdinal("user_name")) ? null : reader.GetString(reader.GetOrdinal("user_name"));
        return new ActivityLog
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? null : reader.GetInt32(reader.GetOrdinal("user_id")),
            Action = reader.GetString(reader.GetOrdinal("action")),
            EntityType = reader.IsDBNull(reader.GetOrdinal("entity_type")) ? null : reader.GetString(reader.GetOrdinal("entity_type")),
            EntityId = reader.IsDBNull(reader.GetOrdinal("entity_id")) ? null : reader.GetInt32(reader.GetOrdinal("entity_id")),
            Details = reader.IsDBNull(reader.GetOrdinal("details")) ? null : reader.GetString(reader.GetOrdinal("details")),
            Source = reader.GetString(reader.GetOrdinal("source")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UserName = string.IsNullOrWhiteSpace(userName) ? null : userName
        };
    }
}
