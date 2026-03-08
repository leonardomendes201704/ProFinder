using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common;
using ProFinder.Application.DTOs.GoogleMapsLeads;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Api.Controllers;

[ApiController]
[Route("api/google-maps-leads")]
public class GoogleMapsLeadsController : ControllerBase
{
    private readonly IGoogleMapsLeadService _googleMapsLeadService;

    public GoogleMapsLeadsController(IGoogleMapsLeadService googleMapsLeadService)
    {
        _googleMapsLeadService = googleMapsLeadService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GoogleMapsLeadListItemDto>>> GetList([FromQuery] GoogleMapsLeadQueryFilter filter, CancellationToken cancellationToken)
    {
        var items = await _googleMapsLeadService.GetPagedAsync(filter, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GoogleMapsLeadDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _googleMapsLeadService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }
}
