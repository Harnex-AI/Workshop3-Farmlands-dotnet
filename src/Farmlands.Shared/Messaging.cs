
namespace Farmlands.Shared.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default);
}

public sealed class ConsoleMessageBus : IMessageBus
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default)
    {
        Console.WriteLine($"[EVENT] {typeof(T).Name}: {System.Text.Json.JsonSerializer.Serialize(message)}");
        return Task.CompletedTask;
    }
}
