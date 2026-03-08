using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.DTOs.Regions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/regions")]
public class RegionsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public RegionsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegionDto>>> GetList([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var items = await _lookupService.GetRegionsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _lookupService.GetRegionByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<RegionDto>> Create([FromBody] UpsertRegionDto dto, CancellationToken cancellationToken)
    {
        var id = await _lookupService.CreateRegionAsync(dto, cancellationToken);
        var item = await _lookupService.GetRegionByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertRegionDto dto, CancellationToken cancellationToken)
    {
        await _lookupService.UpdateRegionAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteRegionAsync(id, cancellationToken);
        return NoContent();
    }
}
