using Dialogs.Infrastructure.SagaStates;
using EventBus;
using EventBus.Events;
using MassTransit;
using MediatR;

namespace Dialogs.Api.Sagas;

public class DialogMessageSaga : MassTransitStateMachine<DialogMessageSagaData>
{
    private readonly ILogger<DialogMessageSaga> _logger;
    private readonly IMediator _mediator;

    public State Idle { get; private set; }
    public State CountingUnreadMessages { get; private set; }
    public State MarkingUnreadMessagesAsRead { get; private set; }
    public State ResettingUnreadMessageCount { get; private set; }

    public Event<MessageSentEvent> MessageSent { get; private set; }
    public Event<UnreadMessageCountUpToDateEvent> UnreadMessageCountUpToDate { get; private set; }
    public Event<MessagesListedEvent> MessagesListed { get; private set; }
    public Event<AllMessagesWereReadEvent> AllMessagesWereRead { get; private set; }
    public Event<UnreadMessageCountIsZeroEvent> UnreadMessageCountIsZero { get; private set; }

    public DialogMessageSaga(ILogger<DialogMessageSaga> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;

        InstanceState(x => x.CurrentState);

        Event(() => MessageSent, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));


        Event(() => MessagesListed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));


        Initially(
            When(MessageSent)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.ReceiverId;
                context.Saga.AgentId = context.Message.SenderId;
                _logger.LogInformation("Сообщение получено. Начат подсчет сообщений.");
            })
            .TransitionTo(CountingUnreadMessages)
            .Publish(context => new UnreadMessageCountWentStaleEvent(context.Saga.CorrelationId, context.Saga.UserId, context.Saga.AgentId))
        );

        During(Idle,
            When(MessageSent)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.ReceiverId;
                context.Saga.AgentId = context.Message.SenderId;
                _logger.LogInformation("Сообщение получено. Начат подсчет сообщений.");
            })
            .TransitionTo(CountingUnreadMessages)
            .Publish(context => new UnreadMessageCountWentStaleEvent(context.Saga.CorrelationId, context.Saga.UserId, context.Saga.AgentId))
        );

        During(CountingUnreadMessages,
            When(UnreadMessageCountUpToDate)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.UserId;
                context.Saga.AgentId = context.Message.AgentId;
                _logger.LogInformation("Счетчик пересчитан.");
            })
            .TransitionTo(Idle)
        );

        During(Idle,
            When(MessagesListed)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.UserId;
                context.Saga.AgentId = context.Message.AgentId;
                _logger.LogInformation("Сообщения запрошены. Начато обновление столбца is_read");
            })
            .TransitionTo(MarkingUnreadMessagesAsRead)
            .Publish(context => new AllMessagesWereReadEvent(context.Saga.CorrelationId, context.Saga.UserId, context.Saga.AgentId))
        );

        During(MarkingUnreadMessagesAsRead,
            When(MessagesListed)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.UserId;
                context.Saga.AgentId = context.Message.AgentId;
                _logger.LogInformation("Сообщения прочитаны. Начат сброс счетчика.");
            })
            .TransitionTo(ResettingUnreadMessageCount)
            .Publish(context => new UnreadMessagesMarkedAsReadEvent(context.Saga.CorrelationId, context.Saga.UserId, context.Saga.AgentId))
        );

        During(ResettingUnreadMessageCount,
            When(UnreadMessageCountUpToDate)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.UserId;
                context.Saga.AgentId = context.Message.AgentId;
                _logger.LogInformation("Cчетчик сброшен.");
            })
            .TransitionTo(Idle)
        );
    }
}
