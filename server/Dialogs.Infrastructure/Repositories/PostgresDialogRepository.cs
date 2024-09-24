using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Dialogs.Domain.Aggregates;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Interfaces;
using Npgsql;

namespace Dialogs.Infrastructure.Repositories;

public class PostgresDialogRepository : IDialogRepository
{
    private readonly string _connectionString;
    private readonly IMapper _mapper;

    public PostgresDialogRepository(string connectionString, IMapper mapper)
    {
        _connectionString = connectionString;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Dialog> SendMessage(DialogMessage message)
    {
        const string sql = @"
            INSERT INTO dialog_messages (id, sender_id, receiver_id, text, is_read, timestamp)
            VALUES (@Id, @SenderId, @ReceiverId, @Text, @IsRead, @Timestamp);

            INSERT INTO dialogs (user_id, agent_id, message_count)
            VALUES (@ReceiverId, @SenderId, 0)
            ON CONFLICT (user_id, agent_id) DO NOTHING;

            INSERT INTO dialogs (user_id, agent_id, message_count)
            VALUES (@SenderId, @ReceiverId, 0)
            ON CONFLICT (user_id, agent_id) DO NOTHING;

            SELECT *
            FROM dialogs
            WHERE user_id = @ReceiverId AND agent_id = @SenderId;";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", message.Id);
                command.Parameters.AddWithValue("@SenderId", message.SenderId);
                command.Parameters.AddWithValue("@ReceiverId", message.ReceiverId);
                command.Parameters.AddWithValue("@Text", message.Text);
                command.Parameters.AddWithValue("@IsRead", message.IsRead);
                command.Parameters.AddWithValue("@Timestamp", message.Timestamp);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Map the selected row to a Dialog object
                        return new Dialog
                        {
                            Id = reader.GetGuid(0),
                            UserId = reader.GetString(1),
                            AgentId = reader.GetString(2),
                            MessageCount = reader.GetInt32(3)
                        };
                    }
                }
            }
        }

        return null; // Handle the case when no dialog is found
    }

    public async Task<List<DialogMessage>> ListMessages(string userId, string agentId)
    {
        const string sql = @"
            SELECT id, sender_id, receiver_id, text, is_read, timestamp
            FROM dialog_messages
            WHERE (sender_id = @SenderId AND receiver_id = @ReceiverId)
                OR (sender_id = @ReceiverId AND receiver_id = @SenderId)
            ORDER BY timestamp ASC";

        var messages = new List<DialogMessage>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SenderId", userId);
                command.Parameters.AddWithValue("@ReceiverId", agentId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var message = new DialogMessage
                        {
                            Id = reader.GetGuid(0),
                            SenderId = reader.GetString(1),
                            ReceiverId = reader.GetString(2),
                            Text = reader.GetString(3),
                            IsRead = reader.GetBoolean(4),
                            Timestamp = reader.GetDateTime(5)
                        };
                        messages.Add(message);
                    }
                }
            }
        }

        return messages;
    }

    public async Task<List<Dialog>> ListDialogs(string user)
    {
        const string sql = @"
            SELECT id, user_id, agent_id, message_count
            FROM dialogs
            WHERE user_id = @UserId";

        var dialogs = new List<Dialog>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", user);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var dialog = new Dialog
                        {
                            Id = reader.GetGuid(0),
                            UserId = reader.GetString(1),
                            AgentId = reader.GetString(2),
                            MessageCount = reader.GetInt32(3)
                        };
                        dialogs.Add(dialog);
                    }
                }
            }
        }

        return dialogs;
    }

    public async Task IncrementMessageCount(Guid dialogId)
    {
        const string sql = @"
            UPDATE dialogs
            SET message_count = message_count + 1
            WHERE id = @DialogId";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DialogId", dialogId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task ResetMessageCount(Guid dialogId)
    {
        const string sql = @"
            UPDATE dialogs
            SET message_count = 0
            WHERE id = @DialogId";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DialogId", dialogId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}