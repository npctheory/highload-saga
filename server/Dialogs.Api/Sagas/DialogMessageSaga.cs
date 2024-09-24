using Dialogs.Infrastructure.SagaStates;
using EventBus;
using EventBus.Events;
using MassTransit;

namespace Dialogs.Api.Sagas;

public class DialogMessageSaga : MassTransitStateMachine<DialogMessageSagaData>
{
    private readonly ILogger<DialogMessageSaga> _logger;

    public State Idle { get; private set; }
    public State Counting { get; private set; }
    public State Resetting { get; private set; }

    public Event<MessageSentEvent> MessageSent { get; private set; }
    public Event<MessagesReadEvent> MessagesRead { get; private set; }

    public DialogMessageSaga(ILogger<DialogMessageSaga> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Event(() => MessageSent, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => MessagesRead, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));


        Initially(
            When(MessageSent)
            .Then(context => 
            {
                context.Saga.UserId = context.Message.ReceiverId;
                context.Saga.AgentId = context.Message.SenderId;
                _logger.LogInformation("Сообщение получено. Начат подсчет сообщений.");
            })
            .TransitionTo(Counting)
            .Then(context => 
            {
                context.Saga.UnreadMessageCount++;
            })
            .TransitionTo(Idle)
        );

        During(Counting,
            When(MessagesRead)
            .Then(context => 
            {
                _logger.LogInformation("Сообщения прочитаны. Сброс счетчика.");
            })
            .TransitionTo(Resetting)
            .Then(context => 
            {
                context.Saga.UnreadMessageCount = 0;
            })
            .TransitionTo(Idle)
        );

        During(Idle,
            When(MessagesRead)
            .Then(context => 
            {
                _logger.LogInformation("Сообщения прочитаны. Сброс счетчика.");
            })
            .TransitionTo(Resetting)
            .Then(context => 
            {
                context.Saga.UnreadMessageCount = 0;
            })
            .TransitionTo(Idle)
        );
    }
}
