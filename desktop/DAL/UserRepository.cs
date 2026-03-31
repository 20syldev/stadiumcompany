using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class UserRepository
{
    public User? Authenticate(string email, string password)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM users WHERE email = @email";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var user = MapUser(reader);
            if (user.IsArchived) return null;
            if (BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
        }
        return null;
    }

    public bool Register(User user)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password)
            .Replace("$2a$", "$2y$");

        var query = @"INSERT INTO users (email, password, last_name, first_name)
                      VALUES (@email, @password, @lastName, @firstName)";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@password", hashedPassword);
        command.Parameters.AddWithValue("@lastName", user.LastName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@firstName", user.FirstName ?? (object)DBNull.Value);

        return command.ExecuteNonQuery() > 0;
    }

    public bool EmailExists(string email)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT COUNT(*) FROM users WHERE email = @email";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@email", email);

        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    public List<User> GetAllNonAdmin()
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM users WHERE is_admin = FALSE ORDER BY last_name, first_name, email";
        using var command = new NpgsqlCommand(query, connection);

        var users = new List<User>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public int ArchiveInactiveUsers()
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand("SELECT archive_inactive_users()", connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UnarchiveUser(int userId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand("SELECT unarchive_user(@userId)", connection);
        command.Parameters.AddWithValue("@userId", userId);
        return Convert.ToBoolean(command.ExecuteScalar());
    }

    private static User MapUser(NpgsqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Email = reader.GetString(reader.GetOrdinal("email")),
            Password = reader.GetString(reader.GetOrdinal("password")),
            LastName = reader.IsDBNull(reader.GetOrdinal("last_name")) ? null : reader.GetString(reader.GetOrdinal("last_name")),
            FirstName = reader.IsDBNull(reader.GetOrdinal("first_name")) ? null : reader.GetString(reader.GetOrdinal("first_name")),
            IsAdmin = !reader.IsDBNull(reader.GetOrdinal("is_admin")) && reader.GetBoolean(reader.GetOrdinal("is_admin")),
            IsArchived = !reader.IsDBNull(reader.GetOrdinal("is_archived")) && reader.GetBoolean(reader.GetOrdinal("is_archived")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}
