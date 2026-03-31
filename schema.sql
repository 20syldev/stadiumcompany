-- StadiumCompany Database - Questionnaire Management (PostgreSQL)

-- Create database
-- CREATE USER stadiumcompany WITH PASSWORD 'stadiumcompany';
-- CREATE DATABASE stadiumcompany OWNER stadiumcompany;

-- If required, grant all privileges
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO stadiumcompany;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO stadiumcompany;

-- Answer type enum
CREATE TYPE answer_type AS ENUM ('TRUE_FALSE', 'MULTIPLE_CHOICE');

-- Users table
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(60) NOT NULL,
    last_name VARCHAR(50),
    first_name VARCHAR(50),
    is_admin BOOLEAN NOT NULL DEFAULT FALSE,
    is_archived BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Themes table
CREATE TABLE themes (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- Questionnaires table
CREATE TABLE questionnaires (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    theme_id INT NOT NULL,
    user_id INT NOT NULL,
    question_count INT NOT NULL DEFAULT 0,
    published BOOLEAN DEFAULT FALSE,
    CONSTRAINT fk_questionnaire_theme FOREIGN KEY (theme_id) REFERENCES themes(id),
    CONSTRAINT fk_questionnaire_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Questions table
CREATE TABLE questions (
    id SERIAL PRIMARY KEY,
    questionnaire_id INT NOT NULL,
    number INT NOT NULL,
    label VARCHAR(250) NOT NULL,
    answer_type answer_type NOT NULL,
    CONSTRAINT fk_question_questionnaire FOREIGN KEY (questionnaire_id) REFERENCES questionnaires(id) ON DELETE CASCADE
);

-- Answers table
CREATE TABLE answers (
    id SERIAL PRIMARY KEY,
    question_id INT NOT NULL,
    label VARCHAR(250) NOT NULL,
    is_correct BOOLEAN DEFAULT FALSE,
    weight DECIMAL(10,2) DEFAULT 1,
    CONSTRAINT fk_answer_question FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE
);

-- Migration (if database already exists):
-- ALTER TABLE answers ALTER COLUMN weight TYPE DECIMAL(10,2);

-- User preferences table
CREATE TABLE user_preferences (
    user_id INT PRIMARY KEY,
    theme VARCHAR(10) NOT NULL DEFAULT 'Light',
    language VARCHAR(5) NOT NULL DEFAULT 'fr',
    CONSTRAINT fk_preference_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Activity logs table (audit trail)
CREATE TABLE activity_logs (
    id SERIAL PRIMARY KEY,
    user_id INT,
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(50),
    entity_id INT,
    details TEXT,
    source VARCHAR(10) NOT NULL DEFAULT 'desktop',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_activity_log_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Quiz submissions table
CREATE TABLE quiz_submissions (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL,
    questionnaire_id INT NOT NULL,
    score DECIMAL(10,2) NOT NULL DEFAULT 0,
    max_score DECIMAL(10,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_submission_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_submission_questionnaire FOREIGN KEY (questionnaire_id) REFERENCES questionnaires(id) ON DELETE CASCADE
);

-- Quiz answers table
CREATE TABLE quiz_answers (
    id SERIAL PRIMARY KEY,
    quiz_submission_id INT NOT NULL,
    question_id INT NOT NULL,
    answer_id INT NOT NULL,
    CONSTRAINT fk_quiz_answer_submission FOREIGN KEY (quiz_submission_id) REFERENCES quiz_submissions(id) ON DELETE CASCADE,
    CONSTRAINT fk_quiz_answer_question FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE,
    CONSTRAINT fk_quiz_answer_answer FOREIGN KEY (answer_id) REFERENCES answers(id) ON DELETE CASCADE
);

-- Question feedbacks table
CREATE TABLE question_feedbacks (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL,
    question_id INT NOT NULL,
    rating SMALLINT NOT NULL,
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_feedback_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_feedback_question FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE
);

-- Indexes
CREATE INDEX idx_questionnaire_theme ON questionnaires(theme_id);
CREATE INDEX idx_questionnaire_user ON questionnaires(user_id);
CREATE INDEX idx_questionnaire_published ON questionnaires(published);
CREATE INDEX idx_question_questionnaire ON questions(questionnaire_id);
CREATE INDEX idx_answer_question ON answers(question_id);
CREATE INDEX idx_activity_logs_user ON activity_logs(user_id);
CREATE INDEX idx_activity_logs_action ON activity_logs(action);
CREATE INDEX idx_activity_logs_created_at ON activity_logs(created_at);
CREATE INDEX idx_quiz_submission_user ON quiz_submissions(user_id);
CREATE INDEX idx_quiz_submission_questionnaire ON quiz_submissions(questionnaire_id);
CREATE INDEX idx_quiz_answer_submission ON quiz_answers(quiz_submission_id);
CREATE INDEX idx_feedback_user ON question_feedbacks(user_id);
CREATE INDEX idx_feedback_question ON question_feedbacks(question_id);
CREATE INDEX idx_users_is_archived ON users(is_archived);

-- Stored procedure: Archive inactive participants
-- Archives non-admin users who haven't submitted a quiz in 180+ days
CREATE OR REPLACE FUNCTION archive_inactive_users()
RETURNS INTEGER AS $$
DECLARE
    archived_count INTEGER;
BEGIN
    UPDATE users
    SET is_archived = TRUE
    WHERE is_admin = FALSE
      AND is_archived = FALSE
      AND id NOT IN (
          SELECT DISTINCT user_id
          FROM quiz_submissions
          WHERE created_at >= (CURRENT_DATE - INTERVAL '180 days')
      );

    GET DIAGNOSTICS archived_count = ROW_COUNT;
    RETURN archived_count;
END;
$$ LANGUAGE plpgsql;

-- Stored procedure: Unarchive a specific user
CREATE OR REPLACE FUNCTION unarchive_user(p_user_id INTEGER)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE users
    SET is_archived = FALSE
    WHERE id = p_user_id AND is_archived = TRUE;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Trigger function: Delete related data when a user is archived
CREATE OR REPLACE FUNCTION on_user_archived()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.is_archived = TRUE AND (OLD.is_archived = FALSE OR OLD.is_archived IS NULL) THEN
        DELETE FROM quiz_answers
        WHERE quiz_submission_id IN (
            SELECT id FROM quiz_submissions WHERE user_id = NEW.id
        );

        DELETE FROM quiz_submissions WHERE user_id = NEW.id;

        DELETE FROM activity_logs WHERE user_id = NEW.id;

        DELETE FROM question_feedbacks WHERE user_id = NEW.id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_user_archived
    AFTER UPDATE OF is_archived ON users
    FOR EACH ROW
    EXECUTE FUNCTION on_user_archived();
