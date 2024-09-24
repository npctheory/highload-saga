using MassTransit;

namespace Dialogs.Infrastructure.SagaStates;

public class DialogMessageSagaData : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; }

    public string UserId { get; set; }

    public string AgentId { get; set; }

    public int UnreadMessageCount { get; set; }
}
