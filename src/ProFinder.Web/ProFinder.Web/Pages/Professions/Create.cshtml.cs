using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Professions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professions;

public class CreateModel : PageModel
{
    private readonly ILookupService _lookupService;

    public CreateModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertProfessionDto Input { get; set; } = new();

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
            await _lookupService.CreateProfessionAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Profissão cadastrada com sucesso.";
            return RedirectToPage("/Professions/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
