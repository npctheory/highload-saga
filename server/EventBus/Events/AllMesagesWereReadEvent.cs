namespace EventBus;

public record AllMessagesWereReadEvent(Guid CorrelationId, string UserId, string AgentId);