using AutoMapper;
using Dialogs.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MassTransit;

namespace Dialogs.Application.Dialogs.Commands.ResetUnreadMessageCount;

public class ResetUnreadMessageCountCommandHandler : IConsumer<UnreadMessageCountIsZeroEvent>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public ResetUnreadMessageCountCommandHandler(IDialogRepository dialogRepository, IMapper mapper, IEventBus eventBus)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task Consume(ConsumeContext<UnreadMessageCountIsZeroEvent> context)
    {
        var notification = context.Message;
        await _dialogRepository.UpdateUnreadMessageCount(notification.CorrelationId, 0);
        UnreadMessageCountUpToDateEvent unreadMessageCountUpToDate = new UnreadMessageCountUpToDateEvent(notification.CorrelationId, notification.UserId, notification.AgentId);
        _eventBus.PublishAsync(unreadMessageCountUpToDate);
    }
}