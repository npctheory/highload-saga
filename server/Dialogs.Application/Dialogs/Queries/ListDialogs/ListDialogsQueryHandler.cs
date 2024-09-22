using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Dialogs.Application.Dialogs.DTO;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Aggregates;
using Dialogs.Domain.Interfaces;


namespace Dialogs.Application.Dialogs.Queries.ListDialogs;

public class ListDialogsQueryHandler : IRequestHandler<ListDialogsQuery, List<DialogDTO>>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;

    public ListDialogsQueryHandler(IDialogRepository dialogRepository, IMapper mapper)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
    }

    public async Task<List<DialogDTO>> Handle(ListDialogsQuery request, CancellationToken cancellationToken)
    {
        List<Dialog> agents = await _dialogRepository.ListDialogs(request.userId);
        return _mapper.Map<List<DialogDTO>>(agents);
    }
}