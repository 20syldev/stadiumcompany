using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class ThemeRepository
{
    public List<Theme> GetAll()
    {
        var themes = new List<Theme>();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM themes ORDER BY name";
        using var command = new NpgsqlCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            themes.Add(new Theme
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Label = reader.GetString(reader.GetOrdinal("name"))
            });
        }

        return themes;
    }

    public Theme? GetById(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM themes WHERE id = @id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Theme
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Label = reader.GetString(reader.GetOrdinal("name"))
            };
        }
        return null;
    }

    public Theme Create(string name)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "INSERT INTO themes (name) VALUES (@name) RETURNING id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@name", name);

        var id = Convert.ToInt32(command.ExecuteScalar());
        return new Theme { Id = id, Label = name };
    }
}
