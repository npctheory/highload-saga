using AutoMapper;
using Dialogs.Domain.Interfaces;
using EventBus;
using EventBus.Events;
using MassTransit;

namespace Dialogs.Application.Dialogs.Commands.MarkDialogMessagesAsRead;

public class MarkDialogMessagesAsReadCommandHandler : IConsumer<AllMessagesWereReadEvent>
{
    private readonly IDialogRepository _dialogRepository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public MarkDialogMessagesAsReadCommandHandler(IDialogRepository dialogRepository, IMapper mapper, IEventBus eventBus)
    {
        _dialogRepository = dialogRepository;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task Consume(ConsumeContext<AllMessagesWereReadEvent> context)
    {
        var notification = context.Message;
        await _dialogRepository.MarkUnreadMessagesAsRead(notification.UserId, notification.AgentId);
        UnreadMessageCountIsZeroEvent countIsZero = new UnreadMessageCountIsZeroEvent(notification.CorrelationId, notification.UserId, notification.AgentId);
        _eventBus.PublishAsync(countIsZero);
    }
}