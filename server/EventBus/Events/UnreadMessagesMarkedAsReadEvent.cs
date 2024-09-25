using MediatR;

namespace EventBus.Events;
public record UnreadMessagesMarkedAsReadEvent(Guid CorrelationId, string UserId, string AgentId) : INotification;