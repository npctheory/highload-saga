using MediatR;

namespace EventBus.Events;
public record UnreadMessageCountWentStaleEvent(Guid CorrelationId, string UserId, string AgentId) : INotification;