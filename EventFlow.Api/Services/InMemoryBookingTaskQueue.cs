using EventFlow.Api.Services.Interfaces;
using System.Threading.Channels;

namespace EventFlow.Api.Services;

/// <summary>
/// In-memory очередь задач фоновой обработки бронирований.
/// Хранит идентификаторы созданных броней и передаёт их в фоновый сервис.
/// </summary>
public class InMemoryBookingTaskQueue : IBookingTaskQueue
{
    private readonly Channel<Guid> _channel;

    /// <summary>
    /// Инициализирует ограниченную очередь бронирований.
    /// Очередь допускает несколько писателей и одного читателя.
    /// При заполнении очередь ожидает освобождения места.
    /// </summary>
    public InMemoryBookingTaskQueue()
    {
        var options = new BoundedChannelOptions(capacity: 100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<Guid>(options);
    }
    /// <summary>
    /// Добавляет идентификатор брони в очередь фоновой обработки.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Задача добавления элемента в очередь.</returns>
    public ValueTask EnqueueAsync(Guid bookingId, CancellationToken ct)
    {
        return _channel.Writer.WriteAsync(bookingId, ct);
    }

    /// <summary>
    /// Возвращает асинхронный поток идентификаторов броней для фоновой обработки.
    /// </summary>
    /// <param name="ct">Токен отмены чтения из очереди.</param>
    /// <returns>Асинхронная последовательность идентификаторов броней.</returns>
    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}

