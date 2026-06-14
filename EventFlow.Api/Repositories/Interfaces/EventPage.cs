using EventFlow.Api.Models;
namespace EventFlow.Api.Repositories.Interfaces;

public sealed record EventPage(IReadOnlyList<Event> Items, int TotalItems);