using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/professionals")]
public class ProfessionalsController : ControllerBase
{
    private readonly IProfessionalService _professionalService;

    public ProfessionalsController(IProfessionalService professionalService)
    {
        _professionalService = professionalService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProfessionalListItemDto>>> GetList([FromQuery] ProfessionalQueryFilter filter, CancellationToken cancellationToken)
    {
        var items = await _professionalService.GetPagedAsync(filter, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProfessionalDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _professionalService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ProfessionalDetailsDto>> Create([FromBody] UpsertProfessionalDto dto, CancellationToken cancellationToken)
    {
        var id = await _professionalService.CreateAsync(dto, cancellationToken);
        var item = await _professionalService.GetByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertProfessionalDto dto, CancellationToken cancellationToken)
    {
        await _professionalService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _professionalService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
