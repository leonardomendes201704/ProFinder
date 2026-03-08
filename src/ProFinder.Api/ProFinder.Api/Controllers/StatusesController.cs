using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.DTOs.Statuses;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/statuses")]
public class StatusesController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public StatusesController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeadStatusDto>>> GetList([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var items = await _lookupService.GetStatusesAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeadStatusDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _lookupService.GetStatusByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<LeadStatusDto>> Create([FromBody] UpsertLeadStatusDto dto, CancellationToken cancellationToken)
    {
        var id = await _lookupService.CreateStatusAsync(dto, cancellationToken);
        var item = await _lookupService.GetStatusByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertLeadStatusDto dto, CancellationToken cancellationToken)
    {
        await _lookupService.UpdateStatusAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteStatusAsync(id, cancellationToken);
        return NoContent();
    }
}
