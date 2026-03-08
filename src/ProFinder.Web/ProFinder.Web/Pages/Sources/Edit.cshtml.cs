using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Sources;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Sources;

public class EditModel : PageModel
{
    private readonly ILookupService _lookupService;

    public EditModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertLeadSourceDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _lookupService.GetSourceByIdAsync(id, cancellationToken);
            Input = new()
            {
                Name = item.Name,
                Description = item.Description,
                Url = item.Url,
                IsActive = item.IsActive
            };

            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Sources/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _lookupService.UpdateSourceAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Origem atualizada com sucesso.";
            return RedirectToPage("/Sources/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
