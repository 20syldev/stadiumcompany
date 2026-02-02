using Npgsql;
using StadiumCompany.Models;

namespace StadiumCompany.DAL;

public class QuestionRepository
{
    public List<Question> GetByQuestionnaire(int questionnaireId)
    {
        var questions = new List<Question>();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM questions WHERE questionnaire_id = @questionnaireId ORDER BY number";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@questionnaireId", questionnaireId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            questions.Add(MapQuestion(reader));
        }

        return questions;
    }

    public Question? GetById(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT * FROM questions WHERE id = @id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapQuestion(reader);
        }
        return null;
    }

    public int Create(Question question)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"INSERT INTO questions (questionnaire_id, number, label, answer_type)
                      VALUES (@questionnaireId, @number, @label, @answerType::answer_type)
                      RETURNING id";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@questionnaireId", question.QuestionnaireId);
        command.Parameters.AddWithValue("@number", question.Number);
        command.Parameters.AddWithValue("@label", question.Label);
        command.Parameters.AddWithValue("@answerType", question.AnswerType.ToString());

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool Update(Question question)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"UPDATE questions
                      SET number = @number, label = @label, answer_type = @answerType::answer_type
                      WHERE id = @id";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", question.Id);
        command.Parameters.AddWithValue("@number", question.Number);
        command.Parameters.AddWithValue("@label", question.Label);
        command.Parameters.AddWithValue("@answerType", question.AnswerType.ToString());

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "DELETE FROM questions WHERE id = @id";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public int GetNextNumber(int questionnaireId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT COALESCE(MAX(number), 0) + 1 FROM questions WHERE questionnaire_id = @questionnaireId";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@questionnaireId", questionnaireId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static Question MapQuestion(NpgsqlDataReader reader)
    {
        return new Question
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            QuestionnaireId = reader.GetInt32(reader.GetOrdinal("questionnaire_id")),
            Number = reader.GetInt32(reader.GetOrdinal("number")),
            Label = reader.GetString(reader.GetOrdinal("label")),
            AnswerType = Enum.Parse<AnswerType>(reader.GetString(reader.GetOrdinal("answer_type")))
        };
    }
}
