using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.DTOs.Sources;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/sources")]
public class SourcesController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public SourcesController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeadSourceDto>>> GetList([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var items = await _lookupService.GetSourcesAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeadSourceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _lookupService.GetSourceByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<LeadSourceDto>> Create([FromBody] UpsertLeadSourceDto dto, CancellationToken cancellationToken)
    {
        var id = await _lookupService.CreateSourceAsync(dto, cancellationToken);
        var item = await _lookupService.GetSourceByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertLeadSourceDto dto, CancellationToken cancellationToken)
    {
        await _lookupService.UpdateSourceAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteSourceAsync(id, cancellationToken);
        return NoContent();
    }
}
