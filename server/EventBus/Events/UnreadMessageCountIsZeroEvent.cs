using MediatR;

namespace EventBus.Events;
public record UnreadMessageCountIsZeroEvent(Guid CorrelationId, string UserId, string AgentId) : INotification;