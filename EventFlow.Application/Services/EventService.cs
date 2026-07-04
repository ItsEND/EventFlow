using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Application.Abstractions.Services;
using EventFlow.Application.Dtos;
using EventFlow.Application.Dtos.Events;
using EventFlow.Application.Exceptions;
using EventFlow.Domain.Exceptions;
using EventFlow.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Application.Services;

/// <summary>
/// Сервис для работы с мероприятиями.
/// Выполняет операции создания, получения, обновления, удаления,
/// фильтрации и пагинации мероприятий.
/// </summary>
public class EventService(IEventRepository eventRepository) : IEventService
{
    public async Task<PaginatedResult<EventDto>> GetEventsAsync(GetEventsQuery pageData, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pageData);
        ValidatePagination(pageData.Page, pageData.PageSize);

        var result = await eventRepository.GetPageAsync(pageData, ct);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageData.PageSize);

        return new PaginatedResult<EventDto>(
            result.Items.Select(MapToDto),
            pageData.Page,
            pageData.PageSize,
            totalPages,
            result.TotalItems,
            result.Items.Count);
    }

    public async Task<EventDto> GetEventAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var ev = await eventRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Event", id);

            return MapToDto(ev);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
    }

    public async Task<EventDto> CreateEventAsync(CreateEventModel newEvent, CancellationToken ct)
    {
        var createdEvent = Event.Create(
            newEvent.Title,
            newEvent.Description,
            newEvent.TotalSeats,
            newEvent.StartAt,
            newEvent.EndAt);

        eventRepository.Add(createdEvent);
        await eventRepository.SaveChangesAsync(ct);

        return MapToDto(createdEvent);
    }

    public async Task<EventDto> UpdateEventAsync(Guid id, UpdateEventModel updatedEvent, CancellationToken ct)
    {
        try
        {
            var existingEvent = await eventRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Event", id);

            existingEvent.Update(
                updatedEvent.Title,
                updatedEvent.Description,
                updatedEvent.StartAt,
                updatedEvent.EndAt);

            await eventRepository.SaveChangesAsync(ct);
            return MapToDto(existingEvent);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
    }

    public async Task RemoveEventAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var ev = await eventRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Event", id);

            eventRepository.Remove(ev);
            await eventRepository.SaveChangesAsync(ct);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ValidationException("Номер страницы должен быть больше или равен 1.");
        }

        if (pageSize < 1)
        {
            throw new ValidationException("Размер страницы должен быть больше или равен 1.");
        }

        if (pageSize > 50)
        {
            throw new ValidationException("Размер страницы не может быть больше 50.");
        }
    }

    private static EventDto MapToDto(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        TotalSeats = ev.TotalSeats,
        AvailableSeats = ev.AvailableSeats,
        StartAt = ev.StartAt,
        EndAt = ev.EndAt
    };
}
