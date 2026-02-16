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

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

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

    private static User MapUser(NpgsqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Email = reader.GetString(reader.GetOrdinal("email")),
            Password = reader.GetString(reader.GetOrdinal("password")),
            LastName = reader.IsDBNull(reader.GetOrdinal("last_name")) ? null : reader.GetString(reader.GetOrdinal("last_name")),
            FirstName = reader.IsDBNull(reader.GetOrdinal("first_name")) ? null : reader.GetString(reader.GetOrdinal("first_name")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}
