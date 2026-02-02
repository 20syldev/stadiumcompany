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
    theme_id INT NOT NULL REFERENCES themes(id),
    user_id INT NOT NULL REFERENCES users(id),
    question_count INT NOT NULL DEFAULT 0,
    published BOOLEAN DEFAULT FALSE
);

-- Questions table
CREATE TABLE questions (
    id SERIAL PRIMARY KEY,
    questionnaire_id INT NOT NULL REFERENCES questionnaires(id) ON DELETE CASCADE,
    number INT NOT NULL,
    label VARCHAR(250) NOT NULL,
    answer_type answer_type NOT NULL
);

-- Answers table
CREATE TABLE answers (
    id SERIAL PRIMARY KEY,
    question_id INT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    label VARCHAR(250) NOT NULL,
    is_correct BOOLEAN DEFAULT FALSE,
    weight DECIMAL(10,2) DEFAULT 1
);

-- Migration (if database already exists):
-- ALTER TABLE answers ALTER COLUMN weight TYPE DECIMAL(10,2);

-- User preferences table
CREATE TABLE user_preferences (
    user_id INT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    theme VARCHAR(10) NOT NULL DEFAULT 'Light',
    language VARCHAR(5) NOT NULL DEFAULT 'fr'
);

-- Indexes
CREATE INDEX idx_questionnaire_theme ON questionnaires(theme_id);
CREATE INDEX idx_questionnaire_user ON questionnaires(user_id);
CREATE INDEX idx_questionnaire_published ON questionnaires(published);
CREATE INDEX idx_question_questionnaire ON questions(questionnaire_id);
CREATE INDEX idx_answer_question ON answers(question_id);
