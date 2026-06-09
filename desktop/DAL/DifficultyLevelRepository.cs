using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class DifficultyLevelRepository
{
    public List<DifficultyLevel> GetAll()
    {
        var levels = new List<DifficultyLevel>();

        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "SELECT * FROM difficulty_levels ORDER BY position, label", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
            levels.Add(MapLevel(reader));

        return levels;
    }

    public DifficultyLevel? GetById(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "SELECT * FROM difficulty_levels WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapLevel(reader) : null;
    }

    public DifficultyLevel Create(string label, int position)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "INSERT INTO difficulty_levels (label, position) VALUES (@label, @position) RETURNING id",
            connection);
        command.Parameters.AddWithValue("@label", label);
        command.Parameters.AddWithValue("@position", position);

        var id = Convert.ToInt32(command.ExecuteScalar());
        return new DifficultyLevel { Id = id, Label = label, Position = position };
    }

    public bool Update(DifficultyLevel level)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "UPDATE difficulty_levels SET label = @label, position = @position WHERE id = @id",
            connection);
        command.Parameters.AddWithValue("@id", level.Id);
        command.Parameters.AddWithValue("@label", level.Label);
        command.Parameters.AddWithValue("@position", level.Position);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "DELETE FROM difficulty_levels WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    // Returns the number of questionnaires using this difficulty level (used before deletion)
    public int CountUsage(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM questionnaires WHERE difficulty_level_id = @id", connection);
        command.Parameters.AddWithValue("@id", id);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static DifficultyLevel MapLevel(NpgsqlDataReader reader) => new()
    {
        Id       = reader.GetInt32(reader.GetOrdinal("id")),
        Label    = reader.GetString(reader.GetOrdinal("label")),
        Position = reader.GetInt32(reader.GetOrdinal("position"))
    };
}
