using Dialogs.Domain.Entities;
using Dialogs.Domain.Aggregates;

namespace Dialogs.Domain.Interfaces;
public interface IDialogRepository
{
        Task<DialogMessage> SendMessage(DialogMessage message);
        Task<List<DialogMessage>> ListMessages(string userId, string agentId);
        Task<Dialog> GetOrInsertDialog(string user, string agent);
        Task<List<Dialog>> ListDialogs(string user);
        Task<Dialog> UpdateUnreadMessageCount(Guid dialogId, long newUnreadCount);
        Task<long> CountUnreadMessages(string userId, string agentId);
        Task<long> MarkUnreadMessagesAsRead(string userId, string agentId);
}