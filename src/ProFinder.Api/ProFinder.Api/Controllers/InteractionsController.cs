using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.DTOs.Interactions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/interactions")]
public class InteractionsController : ControllerBase
{
    private readonly IInteractionService _interactionService;

    public InteractionsController(IInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InteractionDto>>> GetList([FromQuery] int? professionalId = null, CancellationToken cancellationToken = default)
    {
        var items = await _interactionService.GetListAsync(professionalId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InteractionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _interactionService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<InteractionDto>> Create([FromBody] UpsertInteractionDto dto, CancellationToken cancellationToken)
    {
        var id = await _interactionService.CreateAsync(dto, cancellationToken);
        var item = await _interactionService.GetByIdAsync(id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertInteractionDto dto, CancellationToken cancellationToken)
    {
        await _interactionService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _interactionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
