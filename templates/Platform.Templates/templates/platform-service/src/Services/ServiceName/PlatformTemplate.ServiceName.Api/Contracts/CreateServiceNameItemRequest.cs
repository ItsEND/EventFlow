using System.ComponentModel.DataAnnotations;

namespace PlatformTemplate.ServiceName.Api.Contracts;

public sealed record CreateServiceNameItemRequest([property: Required, MaxLength(256)] string Name);
