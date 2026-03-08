using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Statuses;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Statuses;

public class CreateModel : PageModel
{
    private readonly ILookupService _lookupService;

    public CreateModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertLeadStatusDto Input { get; set; } = new();

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
            await _lookupService.CreateStatusAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Status cadastrado com sucesso.";
            return RedirectToPage("/Statuses/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
