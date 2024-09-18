using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Dialogs.Application.Dialogs.DTO;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Interfaces;


namespace Dialogs.Application.Dialogs.Queries.ListMessages;
public class ListMessagesQueryHandler : IRequestHandler<ListMessagesQuery, List<DialogMessageDTO>>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;

    public ListMessagesQueryHandler(IDialogRepository dialogRepository, IMapper mapper)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
    }

    public async Task<List<DialogMessageDTO>> Handle(ListMessagesQuery request, CancellationToken cancellationToken)
    {
        List<DialogMessage> messages = await _dialogRepository.ListMessages(request.userId, request.agentId);
        return _mapper.Map<List<DialogMessageDTO>>(messages);
    }
}