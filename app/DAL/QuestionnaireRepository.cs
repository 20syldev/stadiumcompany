using Npgsql;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.DAL;

public class QuestionnaireRepository
{
    private readonly ThemeRepository _themeRepository = new();

    public List<Questionnaire> GetByUser(int userId)
    {
        var questionnaires = new List<Questionnaire>();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"SELECT q.*, t.name as theme_label,
                      (SELECT COUNT(*) FROM questions WHERE questionnaire_id = q.id) as question_count
                      FROM questionnaires q
                      JOIN themes t ON q.theme_id = t.id
                      WHERE q.user_id = @userId
                      ORDER BY q.id DESC";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var questionnaire = MapQuestionnaire(reader);
            questionnaire.Theme = new Theme
            {
                Id = questionnaire.ThemeId,
                Label = reader.GetString(reader.GetOrdinal("theme_label"))
            };
            questionnaires.Add(questionnaire);
        }

        return questionnaires;
    }

    public List<Questionnaire> GetPublishedByOthers(int currentUserId)
    {
        var questionnaires = new List<Questionnaire>();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"SELECT q.*, t.name as theme_label,
                      u.first_name, u.last_name, u.email,
                      (SELECT COUNT(*) FROM questions WHERE questionnaire_id = q.id) as question_count
                      FROM questionnaires q
                      JOIN themes t ON q.theme_id = t.id
                      JOIN users u ON q.user_id = u.id
                      WHERE q.published = TRUE AND q.user_id != @userId
                      ORDER BY q.id DESC";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", currentUserId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var questionnaire = MapQuestionnaire(reader);
            questionnaire.Theme = new Theme
            {
                Id = questionnaire.ThemeId,
                Label = reader.GetString(reader.GetOrdinal("theme_label"))
            };
            questionnaire.Owner = new User
            {
                Id = questionnaire.UserId,
                FirstName = reader.IsDBNull(reader.GetOrdinal("first_name")) ? null : reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.IsDBNull(reader.GetOrdinal("last_name")) ? null : reader.GetString(reader.GetOrdinal("last_name")),
                Email = reader.GetString(reader.GetOrdinal("email"))
            };
            questionnaires.Add(questionnaire);
        }

        return questionnaires;
    }

    public List<Questionnaire> GetAll()
    {
        var questionnaires = new List<Questionnaire>();

        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"SELECT q.*, t.name as theme_label,
                      (SELECT COUNT(*) FROM questions WHERE questionnaire_id = q.id) as question_count
                      FROM questionnaires q
                      JOIN themes t ON q.theme_id = t.id";

        using var command = new NpgsqlCommand(query, connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var questionnaire = MapQuestionnaire(reader);
            questionnaire.Theme = new Theme
            {
                Id = questionnaire.ThemeId,
                Label = reader.GetString(reader.GetOrdinal("theme_label"))
            };
            questionnaires.Add(questionnaire);
        }

        return questionnaires;
    }

    public Questionnaire? GetById(int id)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"SELECT q.*, t.name as theme_label,
                      (SELECT COUNT(*) FROM questions WHERE questionnaire_id = q.id) as question_count
                      FROM questionnaires q
                      JOIN themes t ON q.theme_id = t.id
                      WHERE q.id = @id";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var questionnaire = MapQuestionnaire(reader);
            questionnaire.Theme = new Theme
            {
                Id = questionnaire.ThemeId,
                Label = reader.GetString(reader.GetOrdinal("theme_label"))
            };
            return questionnaire;
        }
        return null;
    }

    public int Create(Questionnaire questionnaire)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"INSERT INTO questionnaires (name, theme_id, user_id, published)
                      VALUES (@name, @themeId, @userId, @published)
                      RETURNING id";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@name", questionnaire.Name);
        command.Parameters.AddWithValue("@themeId", questionnaire.ThemeId);
        command.Parameters.AddWithValue("@userId", questionnaire.UserId);
        command.Parameters.AddWithValue("@published", questionnaire.Published);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool Update(Questionnaire questionnaire, int requestingUserId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = @"UPDATE questionnaires
                      SET name = @name, theme_id = @themeId, published = @published
                      WHERE id = @id AND user_id = @userId";

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", questionnaire.Id);
        command.Parameters.AddWithValue("@name", questionnaire.Name);
        command.Parameters.AddWithValue("@themeId", questionnaire.ThemeId);
        command.Parameters.AddWithValue("@published", questionnaire.Published);
        command.Parameters.AddWithValue("@userId", requestingUserId);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id, int requestingUserId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "DELETE FROM questionnaires WHERE id = @id AND user_id = @userId";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@userId", requestingUserId);

        return command.ExecuteNonQuery() > 0;
    }

    public bool IsOwner(int questionnaireId, int userId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "SELECT COUNT(*) FROM questionnaires WHERE id = @id AND user_id = @userId";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", questionnaireId);
        command.Parameters.AddWithValue("@userId", userId);

        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    public bool SetPublished(int id, bool published, int requestingUserId)
    {
        using var connection = Database.GetConnection();
        connection.Open();

        var query = "UPDATE questionnaires SET published = @published WHERE id = @id AND user_id = @userId";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@published", published);
        command.Parameters.AddWithValue("@userId", requestingUserId);

        return command.ExecuteNonQuery() > 0;
    }

    public Questionnaire? GetFullById(int id)
    {
        var questionnaire = GetById(id);
        if (questionnaire == null) return null;

        var questionRepo = new QuestionRepository();
        var answerRepo = new AnswerRepository();

        questionnaire.Questions = questionRepo.GetByQuestionnaire(id);
        foreach (var question in questionnaire.Questions)
        {
            question.Answers = answerRepo.GetByQuestion(question.Id);
        }

        return questionnaire;
    }

    public int Fork(int sourceId, int newOwnerId)
    {
        using var connection = Database.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Récupérer le questionnaire source
            var sourceQuery = @"SELECT name, theme_id FROM questionnaires WHERE id = @id";
            using var sourceCmd = new NpgsqlCommand(sourceQuery, connection, transaction);
            sourceCmd.Parameters.AddWithValue("@id", sourceId);

            string sourceName;
            int themeId;
            using (var reader = sourceCmd.ExecuteReader())
            {
                if (!reader.Read())
                    throw new Exception(LocalizationManager.Instance.T("error.source_not_found"));
                sourceName = reader.GetString(0);
                themeId = reader.GetInt32(1);
            }

            // 2. Créer la copie du questionnaire
            var insertQuery = @"INSERT INTO questionnaires (name, theme_id, user_id, published)
                               VALUES (@name, @themeId, @userId, FALSE)
                               RETURNING id";
            using var insertCmd = new NpgsqlCommand(insertQuery, connection, transaction);
            insertCmd.Parameters.AddWithValue("@name", $"{sourceName} {LocalizationManager.Instance.T("main.fork_suffix")}");
            insertCmd.Parameters.AddWithValue("@themeId", themeId);
            insertCmd.Parameters.AddWithValue("@userId", newOwnerId);
            var newQuestionnaireId = Convert.ToInt32(insertCmd.ExecuteScalar());

            // 3. Copier les questions
            var questionsQuery = @"SELECT id, number, label, answer_type FROM questions
                                  WHERE questionnaire_id = @sourceId ORDER BY number";
            using var questionsCmd = new NpgsqlCommand(questionsQuery, connection, transaction);
            questionsCmd.Parameters.AddWithValue("@sourceId", sourceId);

            var questionMapping = new Dictionary<int, int>();
            var questionsToInsert = new List<(int oldId, int number, string label, string answerType)>();

            using (var reader = questionsCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    questionsToInsert.Add((
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetString(2),
                        reader.GetString(3)
                    ));
                }
            }

            foreach (var q in questionsToInsert)
            {
                var insertQQuery = @"INSERT INTO questions (questionnaire_id, number, label, answer_type)
                                    VALUES (@questionnaireId, @number, @label, @answerType::answer_type)
                                    RETURNING id";
                using var insertQCmd = new NpgsqlCommand(insertQQuery, connection, transaction);
                insertQCmd.Parameters.AddWithValue("@questionnaireId", newQuestionnaireId);
                insertQCmd.Parameters.AddWithValue("@number", q.number);
                insertQCmd.Parameters.AddWithValue("@label", q.label);
                insertQCmd.Parameters.AddWithValue("@answerType", q.answerType);
                var newQuestionId = Convert.ToInt32(insertQCmd.ExecuteScalar());
                questionMapping[q.oldId] = newQuestionId;
            }

            // 4. Copier les réponses
            foreach (var (oldQuestionId, newQuestionId) in questionMapping)
            {
                var answersQuery = @"SELECT label, is_correct, weight FROM answers WHERE question_id = @questionId";
                using var answersCmd = new NpgsqlCommand(answersQuery, connection, transaction);
                answersCmd.Parameters.AddWithValue("@questionId", oldQuestionId);

                var answersToInsert = new List<(string label, bool isCorrect, int weight)>();
                using (var reader = answersCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        answersToInsert.Add((reader.GetString(0), reader.GetBoolean(1), reader.GetInt32(2)));
                    }
                }

                foreach (var a in answersToInsert)
                {
                    var insertAQuery = @"INSERT INTO answers (question_id, label, is_correct, weight)
                                        VALUES (@questionId, @label, @isCorrect, @weight)";
                    using var insertACmd = new NpgsqlCommand(insertAQuery, connection, transaction);
                    insertACmd.Parameters.AddWithValue("@questionId", newQuestionId);
                    insertACmd.Parameters.AddWithValue("@label", a.label);
                    insertACmd.Parameters.AddWithValue("@isCorrect", a.isCorrect);
                    insertACmd.Parameters.AddWithValue("@weight", a.weight);
                    insertACmd.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            return newQuestionnaireId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Questionnaire MapQuestionnaire(NpgsqlDataReader reader)
    {
        return new Questionnaire
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            ThemeId = reader.GetInt32(reader.GetOrdinal("theme_id")),
            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
            Published = reader.GetBoolean(reader.GetOrdinal("published")),
            QuestionCount = reader.GetInt32(reader.GetOrdinal("question_count"))
        };
    }
}
