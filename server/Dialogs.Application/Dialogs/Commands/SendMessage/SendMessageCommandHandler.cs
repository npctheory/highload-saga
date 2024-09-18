using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Dialogs.Application.Dialogs.DTO;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Interfaces;


namespace Dialogs.Application.Dialogs.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, DialogMessageDTO>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(IDialogRepository dialogRepository, IMapper mapper)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
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

        DialogMessage message_sent = await _dialogRepository.SendMessage(message);
        return _mapper.Map<DialogMessageDTO>(message_sent);
    }
}