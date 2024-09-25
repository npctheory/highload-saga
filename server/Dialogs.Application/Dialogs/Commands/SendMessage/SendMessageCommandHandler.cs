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

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, DialogMessageDTO>
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

    public async Task<DialogMessageDTO> Handle(SendMessageCommand request, CancellationToken cancellationToken)
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

        DialogMessage sent_message = await _dialogRepository.SendMessage(message);
        _dialogRepository.GetOrInsertDialog(sent_message.SenderId, sent_message.ReceiverId);

        var dialogWithUnreadMessages = await _dialogRepository.GetOrInsertDialog(sent_message.ReceiverId, sent_message.SenderId);


        MessageSentEvent messageSentEvent = new MessageSentEvent{
            CorrelationId = dialogWithUnreadMessages.Id,
            ReceiverId = dialogWithUnreadMessages.UserId,
            SenderId = dialogWithUnreadMessages.AgentId
        };

        _eventBus.PublishAsync(messageSentEvent);
        
        return _mapper.Map<DialogMessageDTO>(sent_message);
    }
}