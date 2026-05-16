namespace EventFlow.Api.Services.Interfaces;

/// <summary>
/// Определяет контракт очереди задач для фоновой обработки бронирований.
/// Очередь принимает идентификаторы созданных броней и предоставляет их
/// фоновому сервису в виде асинхронного потока.
public interface IBookingTaskQueue
{
    /// <summary>
    /// Добавляет бронь в очередь фоновой обработки.
    /// </summary>
    /// <param name="bookingId">Идентификатор брони.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Задача добавления брони в очередь.</returns>
    ValueTask EnqueueAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Возвращает асинхронную последовательность броней, ожидающих обработки.
    /// </summary>
    /// <param name="ct">Токен отмены чтения из очереди.</param>
    /// <returns>Асинхронная последовательность идентификаторов броней.</returns>
    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct);
}
