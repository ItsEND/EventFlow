using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformTemplate.ServiceName.Api.Contracts;
using PlatformTemplate.ServiceName.Application.Abstractions.Services;
using PlatformTemplate.ServiceName.Application.Contracts.Items;

namespace PlatformTemplate.ServiceName.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ServiceName/items")]
public sealed class ServiceNameItemsController(IServiceNameItemService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ServiceNameItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceNameItemDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<ServiceNameItemDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ServiceNameItemDto>> Create(CreateServiceNameItemRequest request, CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(request.Name, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }
}
