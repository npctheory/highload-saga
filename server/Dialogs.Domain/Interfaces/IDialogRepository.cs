using Dialogs.Domain.Entities;
using Dialogs.Domain.Aggregates;

namespace Dialogs.Domain.Interfaces;
public interface IDialogRepository
{
        Task<Dialog> SendMessage(DialogMessage message);
        Task<List<DialogMessage>> ListMessages(string userId, string agentId);
        Task<List<Dialog>> ListDialogs(string user);
        Task IncrementMessageCount(Guid dialogId);
        Task ResetMessageCount(Guid dialogId);
}