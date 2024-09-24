using MediatR;
using System;
using Dialogs.Application.Dialogs.DTO;

namespace Dialogs.Application.Dialogs.Commands.SendMessage;

public record SendMessageCommand(string SenderId, string ReceiverId, string Text) : IRequest<DialogDTO>;