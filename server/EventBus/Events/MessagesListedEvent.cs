namespace EventBus;

public record MessagesListedEvent(Guid CorrelationId, string UserId, string AgentId);