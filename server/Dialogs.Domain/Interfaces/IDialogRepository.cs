using Dialogs.Domain.Entities;
using Dialogs.Domain.Aggregates;

namespace Dialogs.Domain.Interfaces;
public interface IDialogRepository
{
    Task<DialogMessage> SendMessage(DialogMessage message);
    Task<List<DialogMessage>> ListMessages(string userId, string agentId);
    Task<List<Dialog>> ListDialogs(string user);
}