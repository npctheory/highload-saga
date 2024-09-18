using System;

namespace Core.Api.DTO;
public class DialogMessageDTO
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}