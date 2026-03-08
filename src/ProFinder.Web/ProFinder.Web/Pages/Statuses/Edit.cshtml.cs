using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Statuses;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Statuses;

public class EditModel : PageModel
{
    private readonly ILookupService _lookupService;

    public EditModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertLeadStatusDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _lookupService.GetStatusByIdAsync(id, cancellationToken);
            Input = new()
            {
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = item.DisplayOrder,
                IsActive = item.IsActive
            };

            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Statuses/Index");
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
            await _lookupService.UpdateStatusAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Status atualizado com sucesso.";
            return RedirectToPage("/Statuses/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
