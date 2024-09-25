using AutoMapper;
using Dialogs.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MassTransit;

namespace Dialogs.Application.Dialogs.Commands.UpdateUnreadMessageCount;

public class UpdateUnreadMessageCountCommandHandler : IConsumer<UnreadMessageCountWentStaleEvent>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public UpdateUnreadMessageCountCommandHandler(IDialogRepository dialogRepository, IMapper mapper, IEventBus eventBus)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task Consume(ConsumeContext<UnreadMessageCountWentStaleEvent> context)
    {
        var notification = context.Message;
        long count = await _dialogRepository.CountUnreadMessages(notification.UserId, notification.AgentId);
        await _dialogRepository.UpdateUnreadMessageCount(notification.CorrelationId, count);
        UnreadMessageCountUpToDateEvent unreadMessageCountUpToDate = new UnreadMessageCountUpToDateEvent(notification.CorrelationId, notification.UserId, notification.AgentId);
        _eventBus.PublishAsync(unreadMessageCountUpToDate);
    }
}