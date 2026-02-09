using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class AnswerRepository
{
    public List<Answer> GetByQuestion(int questionId)
    {
        var answers = new List<Answer>();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM answers WHERE question_id = @questionId ORDER BY id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@questionId", questionId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            answers.Add(MapAnswer(reader));
        }

        return answers;
    }

    public int Create(Answer answer)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"INSERT INTO answers (question_id, label, is_correct, weight)
                      VALUES (@questionId, @label, @isCorrect, @weight)
                      RETURNING id";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@questionId", answer.QuestionId);
        command.Parameters.AddWithValue("@label", answer.Label);
        command.Parameters.AddWithValue("@isCorrect", answer.IsCorrect);
        command.Parameters.AddWithValue("@weight", answer.Weight);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool Update(Answer answer)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"UPDATE answers
                      SET label = @label, is_correct = @isCorrect, weight = @weight
                      WHERE id = @id";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", answer.Id);
        command.Parameters.AddWithValue("@label", answer.Label);
        command.Parameters.AddWithValue("@isCorrect", answer.IsCorrect);
        command.Parameters.AddWithValue("@weight", answer.Weight);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "DELETE FROM answers WHERE id = @id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteByQuestion(int questionId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "DELETE FROM answers WHERE question_id = @questionId";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@questionId", questionId);

        return command.ExecuteNonQuery() >= 0;
    }

    private static Answer MapAnswer(NpgsqlDataReader reader)
    {
        return new Answer
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            QuestionId = reader.GetInt32(reader.GetOrdinal("question_id")),
            Label = reader.GetString(reader.GetOrdinal("label")),
            IsCorrect = reader.GetBoolean(reader.GetOrdinal("is_correct")),
            Weight = reader.GetDecimal(reader.GetOrdinal("weight"))
        };
    }
}
