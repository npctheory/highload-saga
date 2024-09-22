using MediatR;
using Dialogs.Application.Dialogs.DTO;

namespace Dialogs.Application.Dialogs.Queries.ListDialogs;
public record ListDialogsQuery(string userId) : IRequest<List<DialogDTO>>;