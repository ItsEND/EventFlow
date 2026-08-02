using EventFlow.Events.Domain.Models;

namespace EventFlow.Events.Application.Abstractions.Repositories;

public sealed record EventPage(IReadOnlyList<Event> Items, int TotalItems);
