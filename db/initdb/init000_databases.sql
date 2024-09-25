CREATE USER docker;
CREATE DATABASE highloadsocial;
GRANT ALL PRIVILEGES ON DATABASE highloadsocial TO docker;

\c highloadsocial;

CREATE TABLE IF NOT EXISTS users (
    id text PRIMARY KEY,
    password_hash text NOT NULL,
    first_name text NOT NULL,
    second_name text NOT NULL,
    birthdate DATE NOT NULL,
    biography TEXT,
    city text
);


CREATE TABLE IF NOT EXISTS friendships (
    user_id text NOT NULL,
    friend_id text NOT NULL,
    PRIMARY KEY (user_id, friend_id),
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (friend_id) REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    text VARCHAR(1000) NOT NULL,
    user_id text NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id)
);


CREATE TABLE IF NOT EXISTS dialog_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sender_id text NOT NULL,
    receiver_id text NOT NULL,
    text text NOT NULL,
    is_read BOOLEAN DEFAULT FALSE,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (sender_id) REFERENCES users(id),
    FOREIGN KEY (receiver_id) REFERENCES users(id)
);


CREATE TABLE IF NOT EXISTS dialogs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id text NOT NULL,
    agent_id text NOT NULL,
    unread_message_count INT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (agent_id) REFERENCES users(id),
    UNIQUE (user_id, agent_id)
);

CREATE TYPE dialogs_type AS (
    id UUID,
    user_id TEXT,
    agent_id TEXT,
    unread_message_count INT
);

CREATE OR REPLACE FUNCTION get_or_insert_dialog(p_user_id TEXT, p_agent_id TEXT)
RETURNS dialogs_type AS $$
DECLARE
    dialog_record dialogs_type;
BEGIN
    INSERT INTO dialogs (user_id, agent_id)
    VALUES (p_user_id, p_agent_id)
    ON CONFLICT (user_id, agent_id) DO NOTHING
    RETURNING id, user_id, agent_id, unread_message_count INTO dialog_record;

    IF NOT FOUND THEN
        SELECT id, user_id, agent_id, unread_message_count INTO dialog_record
        FROM dialogs
        WHERE user_id = p_user_id AND agent_id = p_agent_id;
    END IF;

    RETURN dialog_record;
END;
$$ LANGUAGE plpgsql;



\c postgres;
CREATE TABLE IF NOT EXISTS seed_status (
    id SERIAL PRIMARY KEY,
    seed_completed BOOLEAN NOT NULL,
    completed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
