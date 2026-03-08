using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.DTOs.Professions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/professions")]
public class ProfessionsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public ProfessionsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProfessionDto>>> GetList([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var items = await _lookupService.GetProfessionsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProfessionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _lookupService.GetProfessionByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ProfessionDto>> Create([FromBody] UpsertProfessionDto dto, CancellationToken cancellationToken)
    {
        var id = await _lookupService.CreateProfessionAsync(dto, cancellationToken);
        var item = await _lookupService.GetProfessionByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertProfessionDto dto, CancellationToken cancellationToken)
    {
        await _lookupService.UpdateProfessionAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteProfessionAsync(id, cancellationToken);
        return NoContent();
    }
}
