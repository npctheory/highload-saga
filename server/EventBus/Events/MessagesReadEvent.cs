namespace EventBus;

public class MessagesReadEvent
{
    public Guid CorrelationId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public DateTime Timestamp { get; set; }
}