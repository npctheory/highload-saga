namespace Dialogs.Domain.Aggregates;

public class Dialog
{
    public Guid Id { get; set; }
    public string UserId {get; set;}
    public string AgentId { get; set; }
    public int MessageCount {get; set;}
}