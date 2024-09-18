using System.ComponentModel.Design;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dialogs.Api;
using Dialogs.Infrastructure;
using Dialogs.Application;
using Dialogs.Api.Grpc;
using MediatR;
using Dialogs.Api.Behavior;
using Dialogs.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>));
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.UseMiddleware<RequestIdMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapGrpcService<DialogServiceImpl>();
app.MapGrpcReflectionService();
app.Run();