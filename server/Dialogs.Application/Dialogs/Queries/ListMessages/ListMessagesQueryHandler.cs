using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Dialogs.Application.Dialogs.DTO;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Interfaces;
using EventBus;


namespace Dialogs.Application.Dialogs.Queries.ListMessages;
public class ListMessagesQueryHandler : IRequestHandler<ListMessagesQuery, List<DialogMessageDTO>>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    
    public ListMessagesQueryHandler(IDialogRepository dialogRepository, IMapper mapper, IEventBus eventBus)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task<List<DialogMessageDTO>> Handle(ListMessagesQuery request, CancellationToken cancellationToken)
    {
        List<DialogMessage> messages = await _dialogRepository.ListMessages(request.userId, request.agentId);

        var dialog = await _dialogRepository.GetOrInsertDialog(request.userId, request.agentId);
        
        MessagesListedEvent messagesListedEvent = new MessagesListedEvent(dialog.Id, dialog.UserId, dialog.AgentId);

        _eventBus.PublishAsync(messagesListedEvent);
        
        return _mapper.Map<List<DialogMessageDTO>>(messages);
    }
}