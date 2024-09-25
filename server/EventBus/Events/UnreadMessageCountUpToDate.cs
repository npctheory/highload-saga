using MediatR;

namespace EventBus.Events;
public record UnreadMessageCountUpToDateEvent(Guid CorrelationId, string UserId, string AgentId) : INotification;