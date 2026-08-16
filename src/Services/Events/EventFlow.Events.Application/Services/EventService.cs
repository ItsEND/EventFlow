using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Application.Abstractions.Services;
using EventFlow.Events.Application.Contracts;
using EventFlow.Events.Application.Contracts.Events;
using EventFlow.Events.Application.Exceptions;
using EventFlow.Events.Domain.Exceptions;
using EventFlow.Events.Domain.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using EventFlow.Events.Application.Abstractions.Caching;
namespace EventFlow.Events.Application.Services;

/// <summary>
/// Сервис для работы с мероприятиями.
/// Выполняет операции создания, получения, обновления, удаления,
/// фильтрации и пагинации мероприятий.
/// </summary>
public class EventService(IEventRepository eventRepository, ICacheService cache, IOptions<CacheOptions> options) : IEventService
{
    private readonly CacheOptions cacheOptions = options.Value;
    private const int TopEventsLimit = 10;
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
        var key = CacheKeys.Event(id);
        var cached = await cache.GetAsync<EventDto>(key, ct);
        if (cached is not null)
        {
            return cached;
        }
        

        try
        {
            var ev = await eventRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Мероприятие", id);

            var dto = MapToDto(ev);
            await cache.SetAsync(key, dto, cacheOptions.EventTtl, ct);
            return dto;
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
                ?? throw new NotFoundException("Мероприятие", id);

            existingEvent.Update(
                updatedEvent.Title,
                updatedEvent.Description,
                updatedEvent.StartAt,
                updatedEvent.EndAt);

            await eventRepository.SaveChangesAsync(ct);
            await cache.RemoveAsync(CacheKeys.Event(id), ct);

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
                ?? throw new NotFoundException("Мероприятие", id);

            eventRepository.Remove(ev);
            await eventRepository.SaveChangesAsync(ct);
            await cache.RemoveAsync(CacheKeys.Event(id), ct);
        }
        catch (NotFoundException ex)
        {
            throw AppException.NotFound(ex.Message, ex);
        }
    }

    public async Task<IReadOnlyList<EventDto>> GetTopEventsAsync(CancellationToken ct = default)
    {
        var cached = await cache.GetAsync<List<EventDto>>(CacheKeys.TopEvents, ct);
        if (cached is not null)
        {
            return cached;
        }
        var events = await eventRepository.GetTopEventsAsync(TopEventsLimit, ct);
        var result = events.Select(MapToDto).ToList();

        await cache.SetAsync(CacheKeys.TopEvents, result, cacheOptions.TopEventsTtl, ct);
        
        return result;
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
