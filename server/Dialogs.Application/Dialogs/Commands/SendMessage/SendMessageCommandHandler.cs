using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Dialogs.Application.Dialogs.DTO;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using Dialogs.Domain.Aggregates;


namespace Dialogs.Application.Dialogs.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, DialogDTO>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public SendMessageCommandHandler(IDialogRepository dialogRepository, IMapper mapper, IEventBus eventBus)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task<DialogDTO> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        if(request.ReceiverId == request.SenderId)
        {
            throw new Exception();
        }

        DialogMessage message = new DialogMessage{
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            Text = request.Text
        };

        Dialog receiver_dialog = await _dialogRepository.SendMessage(message);

        MessageSentEvent messageSentEvent = new MessageSentEvent{
            CorrelationId = receiver_dialog.Id,
            ReceiverId = receiver_dialog.UserId,
            SenderId = receiver_dialog.AgentId
        };



        await _eventBus.PublishAsync(messageSentEvent);
        return _mapper.Map<DialogDTO>(receiver_dialog);
    }
}