using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Sources;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Sources;

public class CreateModel : PageModel
{
    private readonly ILookupService _lookupService;

    public CreateModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertLeadSourceDto Input { get; set; } = new();

    public void OnGet()
    {
        Input.IsActive = true;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _lookupService.CreateSourceAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Origem cadastrada com sucesso.";
            return RedirectToPage("/Sources/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
