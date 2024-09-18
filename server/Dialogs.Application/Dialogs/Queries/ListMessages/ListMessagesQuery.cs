using MediatR;
using Dialogs.Application.Dialogs.DTO;

namespace Dialogs.Application.Dialogs.Queries.ListMessages;
public record ListMessagesQuery(string userId, string agentId) : IRequest<List<DialogMessageDTO>>;