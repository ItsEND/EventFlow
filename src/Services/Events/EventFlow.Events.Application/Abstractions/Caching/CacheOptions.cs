namespace EventFlow.Events.Application.Abstractions.Caching;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";
    public TimeSpan EventTtl { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan TopEventsTtl { get; init; } = TimeSpan.FromMinutes(1);
}