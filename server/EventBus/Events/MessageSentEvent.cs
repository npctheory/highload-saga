namespace EventBus.Events;

public class MessageSentEvent
{
    public Guid CorrelationId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public DateTime Timestamp { get; set; }
}
