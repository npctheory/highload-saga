using MediatR;

namespace Core.Api.Behavior;
public class RequestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<RequestLoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestLoggingBehavior(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestId = _httpContextAccessor.HttpContext?.Request.Headers["x-request-id"].FirstOrDefault();

        _logger.LogInformation("Обработка реквеста {RequestName}(x-request-id: {RequestId}).", typeof(TRequest).Name, requestId);

        var response = await next();

        _logger.LogInformation("Обработка реквеста {RequestName}(x-request-id: {RequestId}).", typeof(TRequest).Name, requestId);

        return response;
    }
}
