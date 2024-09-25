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

    public async Task<DialogMessage> SendMessage(DialogMessage message)
    {
        const string messageSql = @"
            INSERT INTO dialog_messages (id, sender_id, receiver_id, text, is_read, timestamp)
            VALUES (@Id, @SenderId, @ReceiverId, @Text, @IsRead, @Timestamp)
            RETURNING id, sender_id, receiver_id, text, is_read, timestamp;";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            DialogMessage sentMessage;
            using (var messageCommand = new NpgsqlCommand(messageSql, connection))
            {
                messageCommand.Parameters.AddWithValue("@Id", message.Id);
                messageCommand.Parameters.AddWithValue("@SenderId", message.SenderId);
                messageCommand.Parameters.AddWithValue("@ReceiverId", message.ReceiverId);
                messageCommand.Parameters.AddWithValue("@Text", message.Text);
                messageCommand.Parameters.AddWithValue("@IsRead", message.IsRead);
                messageCommand.Parameters.AddWithValue("@Timestamp", message.Timestamp);

                using (var reader = await messageCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        sentMessage = new DialogMessage
                        {
                            Id = reader.GetGuid(0),
                            SenderId = reader.GetString(1),
                            ReceiverId = reader.GetString(2),
                            Text = reader.GetString(3),
                            IsRead = reader.GetBoolean(4),
                            Timestamp = reader.GetDateTime(5)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return sentMessage;
        }
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

    public async Task<Dialog> GetOrInsertDialog(string user, string agent)
    {
        const string dialogSql = @"
            SELECT * FROM get_or_insert_dialog(@UserId, @AgentId);";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            Dialog dialog = null;
            using (var dialogCommand = new NpgsqlCommand(dialogSql, connection))
            {
                dialogCommand.Parameters.AddWithValue("@UserId", user);
                dialogCommand.Parameters.AddWithValue("@AgentId", agent);

                using (var reader = await dialogCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        dialog = new Dialog
                        {
                            Id = reader.GetGuid(0),
                            UserId = reader.GetString(1),
                            AgentId = reader.GetString(2),
                            UnreadMessageCount = reader.GetInt32(3)
                        };
                    }
                }
            }

            return dialog;
        }
    }

    public async Task<List<Dialog>> ListDialogs(string user)
    {
        const string sql = @"
            SELECT id, user_id, agent_id, unread_message_count
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
                            UnreadMessageCount = reader.GetInt32(3)
                        };
                        dialogs.Add(dialog);
                    }
                }
            }
        }

        return dialogs;
    }

    public async Task<long> CountUnreadMessages(string userId, string agentId)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM dialog_messages
            WHERE receiver_id = @UserId AND sender_id = @AgentId AND is_read = FALSE;";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@AgentId", agentId);

                var result = (long)await command.ExecuteScalarAsync();
                return result;
            }
        }
    }


    public async Task<Dialog> UpdateUnreadMessageCount(Guid dialogId, long newUnreadCount)
    {
        const string sql = @"
            UPDATE dialogs
            SET unread_message_count = @NewUnreadCount
            WHERE id = @DialogId
            RETURNING id, user_id, agent_id, unread_message_count;";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DialogId", dialogId);
                command.Parameters.AddWithValue("@NewUnreadCount", newUnreadCount);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Dialog
                        {
                            Id = reader.GetGuid(0),
                            UserId = reader.GetString(1),
                            AgentId = reader.GetString(2),
                            UnreadMessageCount = reader.GetInt32(3)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<long> MarkUnreadMessagesAsRead(string userId, string agentId)
    {
        const string sql = @"
            UPDATE dialog_messages
            SET is_read = TRUE
            WHERE receiver_id = @UserId AND sender_id = @AgentId AND is_read = FALSE;";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@AgentId", agentId);

                return await command.ExecuteNonQueryAsync();
            }
        }
    }


}