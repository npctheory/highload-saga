using EventBus.Events;
using MassTransit;

namespace Dialogs.Api.Consumers;
public class MessageSentEventConsumer : IConsumer<MessageSentEvent>
{
    public Task Consume(ConsumeContext<MessageSentEvent> context)
    {
        var message = context.Message;

        Console.WriteLine($"CorrelationId: {message.CorrelationId}, Sender: {message.SenderId}, Receiver: {message.ReceiverId}");

        return Task.CompletedTask;
    }
}
