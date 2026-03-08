using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Professions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professions;

public class EditModel : PageModel
{
    private readonly ILookupService _lookupService;

    public EditModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertProfessionDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _lookupService.GetProfessionByIdAsync(id, cancellationToken);
            Input = new()
            {
                Name = item.Name,
                Description = item.Description,
                IsActive = item.IsActive
            };

            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Professions/Index");
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
            await _lookupService.UpdateProfessionAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Profissão atualizada com sucesso.";
            return RedirectToPage("/Professions/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
