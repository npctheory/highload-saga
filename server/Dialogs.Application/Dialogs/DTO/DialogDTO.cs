using System;
using System.ComponentModel.DataAnnotations;

namespace Dialogs.Application.Dialogs.DTO;

public record DialogDTO(
    string AgentId,
    string MessageCount
);