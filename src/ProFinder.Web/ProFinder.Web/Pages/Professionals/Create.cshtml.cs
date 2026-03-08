using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professionals;

public class CreateModel : ProfessionalFormPageModel
{
    private readonly IProfessionalService _professionalService;

    public CreateModel(IProfessionalService professionalService, ILookupService lookupService)
        : base(lookupService)
    {
        _professionalService = professionalService;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadOptionsAsync(cancellationToken);
        Input.IsAutonomous = true;
        Input.IsActive = true;
        Input.ProfessionIds = [];
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(cancellationToken);
            return Page();
        }

        try
        {
            await _professionalService.CreateAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Profissional cadastrado com sucesso.";
            return RedirectToPage("/Professionals/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadOptionsAsync(cancellationToken);
            return Page();
        }
    }
}
