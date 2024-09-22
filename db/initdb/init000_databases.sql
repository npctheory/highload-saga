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
    message_count INT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (agent_id) REFERENCES users(id)
);

CREATE OR REPLACE FUNCTION insert_dialog_if_not_exists()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM dialogs
        WHERE user_id = NEW.sender_id AND agent_id = NEW.receiver_id
    ) THEN
        INSERT INTO dialogs (user_id, agent_id, message_count)
        VALUES (NEW.sender_id, NEW.receiver_id, 0);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM dialogs
        WHERE user_id = NEW.receiver_id AND agent_id = NEW.sender_id
    ) THEN
        INSERT INTO dialogs (user_id, agent_id, message_count)
        VALUES (NEW.receiver_id, NEW.sender_id, 0);
    END IF;

    UPDATE dialogs
    SET message_count = message_count + 1
    WHERE user_id = NEW.sender_id AND agent_id = NEW.receiver_id;

    UPDATE dialogs
    SET message_count = message_count + 1
    WHERE user_id = NEW.receiver_id AND agent_id = NEW.sender_id;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER after_message_insert
AFTER INSERT ON dialog_messages
FOR EACH ROW
EXECUTE FUNCTION insert_dialog_if_not_exists();



\c postgres;
CREATE TABLE IF NOT EXISTS seed_status (
    id SERIAL PRIMARY KEY,
    seed_completed BOOLEAN NOT NULL,
    completed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
