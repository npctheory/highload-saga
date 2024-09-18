using Grpc.Net.Client;
using Dialogs.Api.Grpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Core.Api.DTO;
using Grpc.Core;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Core.Api.Controllers;

[ApiController]
public class DialogController : ControllerBase
{
    private readonly DialogService.DialogServiceClient _dialogServiceClient;
    private readonly ILogger<DialogController> _logger;

    public DialogController(DialogService.DialogServiceClient dialogServiceClient, ILogger<DialogController> logger)
    {
        _dialogServiceClient = dialogServiceClient;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("dialog/list")]
    public async Task<IActionResult> ListDialogs()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestId = HttpContext.Request.Headers["x-request-id"].FirstOrDefault();

        _logger.LogInformation("Обработка реквеста ListDialogs(x-request-id: {RequestId}).", requestId);

        try
        {
            var metadata = new Metadata();
            if (!string.IsNullOrEmpty(requestId))
            {
                metadata.Add("x-request-id", requestId);
            }

            var response = await _dialogServiceClient.ListDialogsAsync(new ListDialogsRequest { UserId = userId }, metadata);
            var dialogs = response.Agents.Select(agent => new AgentDTO(agent.AgentId)).ToList();
            _logger.LogInformation("Реквест ListDialogs успех. Ответ: {@Response}", response);
            return Ok(dialogs);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            _logger.LogWarning("Реквест ListDialogs ошибка. Текст ошибки: {Message}", ex.Message);
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpGet("dialog/{agentId}/list")]
    public async Task<IActionResult> ListMessages([FromRoute] string agentId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestId = HttpContext.Request.Headers["x-request-id"].FirstOrDefault();

        _logger.LogInformation("Обработка реквеста ListMessages(x-request-id: {RequestId}).", requestId);

        try
        {
            var metadata = new Metadata();
            if (!string.IsNullOrEmpty(requestId))
            {
                metadata.Add("x-request-id", requestId);
            }

            var response = await _dialogServiceClient.ListMessagesAsync(new ListMessagesRequest { UserId = userId, AgentId = agentId }, metadata);
            var messages = response.Messages.Select(message => new DialogMessageDTO
            {
                Id = Guid.Parse(message.Id),
                Text = message.Text,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                IsRead = message.IsRead,
                Timestamp = DateTime.Parse(message.Timestamp)
            }).ToList();
            _logger.LogInformation("ListMessages успех. Ответ: {RequestId}).", response);
            return Ok(messages);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            _logger.LogWarning("ListMessages ошибка. Текст ошибки: {Message}", ex.Message);
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpPost("dialog/{receiverId}/send")]
    public async Task<IActionResult> SendMessage([FromRoute] string receiverId, [FromBody] JsonElement jsonElement)
    {
        var senderId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string text = jsonElement.GetProperty("text").GetString();
        var requestId = HttpContext.Request.Headers["x-request-id"].FirstOrDefault();

        _logger.LogInformation("Обработка реквеста SendMessage(x-request-id: {RequestId})", requestId);

        try
        {
            var metadata = new Metadata();
            if (!string.IsNullOrEmpty(requestId))
            {
                metadata.Add("x-request-id", requestId);
            }

            var response = await _dialogServiceClient.SendMessageAsync(new SendMessageRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Text = text
            }, metadata);
            
            var message = response.Message;
            var dialogMessageDto = new DialogMessageDTO
            {
                Id = Guid.Parse(message.Id),
                Text = message.Text,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                IsRead = message.IsRead,
                Timestamp = DateTime.Parse(message.Timestamp)
            };

            _logger.LogInformation("SendMessage успех. Ответ: {@Response}", response);
            return Ok(dialogMessageDto);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            _logger.LogWarning("SendMessage ошибка. Текст ошибки: {Message}", ex.Message);
            return Unauthorized();
        }
    }
}
