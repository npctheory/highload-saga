using System;
using System.ComponentModel.DataAnnotations;

namespace Dialogs.Application.Dialogs.DTO;

public record DialogDTO(
    Guid Id, string UserId, string AgentId, int UnreadMessageCount
);