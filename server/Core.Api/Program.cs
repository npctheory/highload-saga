using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Core.Application;
using Core.Infrastructure;
using Core.Api;
using Core.Api.Hubs;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Dialogs.Api.Grpc;
using Core.Api.Middleware;
using MediatR;
using Core.Api.Behavior;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>));
builder.Services.AddHttpContextAccessor();


var dialogsGrpcUrl = Environment.GetEnvironmentVariable("DIALOGS_GRPC_URL") ?? "http://dialogs:82";

builder.Services.AddGrpcClient<DialogService.DialogServiceClient>(options =>
{
    options.Address = new Uri(dialogsGrpcUrl);
});

builder.Services.AddControllers();

var app = builder.Build();
app.UseMiddleware<RequestIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PostHub>("/post/feed/posted");

app.Run();
