using EventFlow.Domain.Models;

namespace EventFlow.Application.Abstractions.Repositories;

public sealed record EventPage(IReadOnlyList<Event> Items, int TotalItems);
