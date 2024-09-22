namespace EventBus.Events;

public class MessageSentEvent
{
    public string DialogId { get; set; }
    public string UserId {get; set;}
    public string AgentId {get; set;}
}
