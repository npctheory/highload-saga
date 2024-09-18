using Grpc.Core;
using MediatR;
using Dialogs.Application.Dialogs.Commands.SendMessage;
using Dialogs.Application.Dialogs.DTO;
using Dialogs.Application.Dialogs.Queries.ListDialogs;
using Dialogs.Application.Dialogs.Queries.ListMessages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Dialogs.Api.Grpc;

public class DialogServiceImpl : DialogService.DialogServiceBase
{
    private readonly ISender _mediator;
    private readonly ILogger<DialogServiceImpl> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DialogServiceImpl(ISender mediator, ILogger<DialogServiceImpl> logger, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<ListDialogsResponse> ListDialogs(ListDialogsRequest request, ServerCallContext context)
    {
        var requestId = _httpContextAccessor.HttpContext?.Request.Headers["x-request-id"].FirstOrDefault();
        _logger.LogInformation("Handling ListDialogs request with x-request-id {RequestId}", requestId);

        var userId = request.UserId;

        try
        {
            var dialogs = await _mediator.Send(new ListDialogsQuery(userId));
            var response = new ListDialogsResponse
            {
                Agents = { dialogs.Select(d => new AgentResponse { AgentId = d.AgentId }) }
            };
            return response;
        }
        catch (UnauthorizedAccessException)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized"));
        }
    }

    public override async Task<ListMessagesResponse> ListMessages(ListMessagesRequest request, ServerCallContext context)
    {
        var requestId = _httpContextAccessor.HttpContext?.Request.Headers["x-request-id"].FirstOrDefault();
        _logger.LogInformation("Handling ListMessages request with x-request-id {RequestId}", requestId);

        var userId = request.UserId;
        var agentId = request.AgentId;

        try
        {
            var messages = await _mediator.Send(new ListMessagesQuery(userId, agentId));
            var response = new ListMessagesResponse
            {
                Messages = { messages.Select(m => new DialogMessageResponse
                {
                    Id = m.Id.ToString(),
                    Text = m.Text,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    IsRead = m.IsRead,
                    Timestamp = m.Timestamp.ToString("o")
                }) }
            };
            return response;
        }
        catch (UnauthorizedAccessException)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized"));
        }
    }

    public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        var requestId = _httpContextAccessor.HttpContext?.Request.Headers["x-request-id"].FirstOrDefault();
        _logger.LogInformation("Handling SendMessage request with x-request-id {RequestId}", requestId);

        var senderId = request.SenderId;
        var receiverId = request.ReceiverId;
        var text = request.Text;

        try
        {
            var message = await _mediator.Send(new SendMessageCommand(senderId, receiverId, text));
            var response = new SendMessageResponse
            {
                Message = new DialogMessageResponse
                {
                    Id = message.Id.ToString(),
                    Text = message.Text,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    IsRead = message.IsRead,
                    Timestamp = message.Timestamp.ToString("o")
                }
            };
            return response;
        }
        catch (UnauthorizedAccessException)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized"));
        }
    }
}