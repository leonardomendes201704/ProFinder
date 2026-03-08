using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Interactions;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Enums;

namespace ProFinder.Web.Pages.Professionals;

public class DetailsModel : PageModel
{
    private readonly IProfessionalService _professionalService;
    private readonly IInteractionService _interactionService;

    public DetailsModel(IProfessionalService professionalService, IInteractionService interactionService)
    {
        _professionalService = professionalService;
        _interactionService = interactionService;
    }

    public ProfessionalDetailsDto Professional { get; private set; } = new();

    [BindProperty]
    public UpsertInteractionDto NewInteraction { get; set; } = new();

    public IReadOnlyList<SelectListItem> InteractionTypeOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await LoadPageAsync(id, cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Professionals/Index");
        }
    }

    public async Task<IActionResult> OnPostAddInteractionAsync(int id, CancellationToken cancellationToken)
    {
        NewInteraction.ProfessionalId = id;

        if (!ModelState.IsValid)
        {
            await LoadPageAsync(id, cancellationToken);
            return Page();
        }

        try
        {
            await _interactionService.CreateAsync(NewInteraction, cancellationToken);
            TempData["SuccessMessage"] = "Interação registrada com sucesso.";
            return RedirectToPage(new { id });
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadPageAsync(id, cancellationToken);
            return Page();
        }
    }

    private async Task LoadPageAsync(int id, CancellationToken cancellationToken)
    {
        Professional = await _professionalService.GetByIdAsync(id, cancellationToken);
        InteractionTypeOptions = Enum.GetValues<InteractionType>()
            .Select(x => new SelectListItem(GetInteractionTypeLabel(x), ((int)x).ToString()))
            .ToList();

        NewInteraction ??= new UpsertInteractionDto();
        NewInteraction.ProfessionalId = id;
        if (NewInteraction.InteractionDate == default)
        {
            NewInteraction.InteractionDate = DateTime.Now;
        }
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
