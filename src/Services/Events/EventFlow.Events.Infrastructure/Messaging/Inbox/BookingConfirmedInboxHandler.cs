using EventFlow.Contracts;
using EventFlow.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EventFlow.Events.Infrastructure.Messaging.Inbox;

public sealed class BookingConfirmedInboxHandler(EventDbContext dbContext, ILogger<BookingConfirmedInboxHandler> logger)
{
    public async Task HandleAsync(BookingConfirmed message, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var alreadyProcessed = await dbContext.InboxMessages.AnyAsync(
                    inbox => inbox.MessageId == message.BookingId,
                    cancellationToken);

            if (alreadyProcessed)
            {
                logger.LogInformation(
                    "Сообщение BookingConfirmed {MessageId} " +
                    "уже обработано. Повтор пропущен.",
                    message.BookingId);

                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var inboxMessage = InboxMessage.Create(message.BookingId, DateTime.UtcNow);

            dbContext.InboxMessages.Add(inboxMessage);

            var ev = await dbContext.Events.FirstOrDefaultAsync(
                currentEvent => currentEvent.Id == message.EventId,
                cancellationToken);

            if (ev is null)
            {
                inboxMessage.MarkIgnored(InboxMessageStatus.IgnoredEventNotFound, $"Мероприятие {message.EventId} не найдено.", DateTime.UtcNow);

                await SaveAndCommitAsync(transaction, cancellationToken);

                logger.LogWarning(
                    "Сообщение {MessageId} пропущено: " +
                    "мероприятие {EventId} не найдено.",
                    message.BookingId,
                    message.EventId);

                return;
            }

            if (!ev.TryReserveSeats(message.SeatCount))
            {
                inboxMessage.MarkIgnored(
                    InboxMessageStatus.IgnoredNotEnoughSeats,
                    $"Для мероприятия {message.EventId} " +
                    $"недостаточно свободных мест.",
                    DateTime.UtcNow);

                await SaveAndCommitAsync(transaction, cancellationToken);

                logger.LogWarning(
                    "Сообщение {MessageId} пропущено: " +
                    "для мероприятия {EventId} недостаточно мест.",
                    message.BookingId,
                    message.EventId);

                return;
            }

            inboxMessage.MarkProcessed(DateTime.UtcNow);

            await SaveAndCommitAsync(transaction, cancellationToken);

            logger.LogInformation(
                "Сообщение {MessageId} обработано. " +
                "Для мероприятия {EventId} зарезервировано мест: {SeatCount}.",
                message.BookingId,
                message.EventId,
                message.SeatCount);
        }
        catch (DbUpdateException exception) when (IsUniqueInboxMessage(exception))
        {
            await transaction.RollbackAsync(cancellationToken);

            logger.LogInformation(
                "Сообщение BookingConfirmed {MessageId} " +
                "уже было обработано другим экземпляром сервиса.",
                message.BookingId);
        }
    }

    private async Task SaveAndCommitAsync(IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static bool IsUniqueInboxMessage(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
    }
}