using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class UserPreferencesRepository
{
    private static bool _tableChecked;

    private static void EnsureTableExists()
    {
        if (_tableChecked) return;

        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"CREATE TABLE IF NOT EXISTS user_preferences (
                          user_id INT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
                          theme VARCHAR(10) NOT NULL DEFAULT 'Light',
                          language VARCHAR(5) NOT NULL DEFAULT 'fr'
                      )";
        using var command = new NpgsqlCommand(query, connection);
        command.ExecuteNonQuery();

        _tableChecked = true;
    }

    public UserPreferences? GetByUserId(int userId)
    {
        EnsureTableExists();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT user_id, theme, language FROM user_preferences WHERE user_id = @userId";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new UserPreferences
            {
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                Theme = reader.GetString(reader.GetOrdinal("theme")),
                Language = reader.GetString(reader.GetOrdinal("language"))
            };
        }
        return null;
    }

    public void Save(UserPreferences prefs)
    {
        EnsureTableExists();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"INSERT INTO user_preferences (user_id, theme, language)
                      VALUES (@userId, @theme, @language)
                      ON CONFLICT (user_id) DO UPDATE SET theme = @theme, language = @language";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", prefs.UserId);
        command.Parameters.AddWithValue("@theme", prefs.Theme);
        command.Parameters.AddWithValue("@language", prefs.Language);

        command.ExecuteNonQuery();
    }
}
