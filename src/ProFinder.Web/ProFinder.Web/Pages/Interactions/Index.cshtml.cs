using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Interactions;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Enums;

namespace ProFinder.Web.Pages.Interactions;

public class IndexModel : PageModel
{
    private readonly IInteractionService _interactionService;
    private readonly IProfessionalService _professionalService;

    public IndexModel(IInteractionService interactionService, IProfessionalService professionalService)
    {
        _interactionService = interactionService;
        _professionalService = professionalService;
    }

    [BindProperty(SupportsGet = true)]
    public int? ProfessionalId { get; set; }

    [BindProperty]
    public UpsertInteractionDto Input { get; set; } = new()
    {
        InteractionDate = DateTime.Now,
        CreatedBy = "Admin"
    };

    public IReadOnlyList<InteractionDto> Items { get; private set; } = [];

    public IReadOnlyList<SelectListItem> ProfessionalOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> InteractionTypeOptions { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadPageAsync(cancellationToken);
        if (ProfessionalId.HasValue && Input.ProfessionalId == 0)
        {
            Input.ProfessionalId = ProfessionalId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        try
        {
            await _interactionService.CreateAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Interação registrada com sucesso.";
            return RedirectToPage(new { professionalId = Input.ProfessionalId });
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadPageAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        Items = await _interactionService.GetListAsync(ProfessionalId, cancellationToken);

        var professionals = await _professionalService.GetLookupAsync(cancellationToken);
        ProfessionalOptions = professionals
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        InteractionTypeOptions = Enum.GetValues<InteractionType>()
            .Select(x => new SelectListItem(GetInteractionTypeLabel(x), ((int)x).ToString()))
            .ToList();
    }

    private static string GetInteractionTypeLabel(InteractionType type) =>
        type switch
        {
            InteractionType.PhoneCall => "Ligação",
            InteractionType.WhatsApp => "WhatsApp",
            InteractionType.Email => "E-mail",
            InteractionType.Note => "Observação",
            InteractionType.Visit => "Visita",
            _ => "Outro"
        };
}
