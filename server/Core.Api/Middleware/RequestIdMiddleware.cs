namespace Core.Api.Middleware;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("x-request-id", out var requestId))
        {
            requestId = Guid.NewGuid().ToString();
            context.Request.Headers.Add("x-request-id", requestId);
        }

        context.Response.Headers.Add("x-request-id", requestId);

        await _next(context);
    }
}
