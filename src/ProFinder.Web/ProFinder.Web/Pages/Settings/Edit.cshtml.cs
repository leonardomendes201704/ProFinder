using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Settings;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Settings;

public class EditModel : PageModel
{
    private readonly IAppSettingService _appSettingService;

    public EditModel(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    public AppSettingDto Setting { get; private set; } = new();

    [BindProperty]
    public UpdateAppSettingDto Input { get; set; } = new();

    public bool UseTextArea => string.Equals(Setting.DataType, "json", StringComparison.OrdinalIgnoreCase);

    public string InputType => Setting.IsSensitive ? "password" : "text";

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await LoadAsync(id, cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Settings/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _appSettingService.UpdateAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Configuracao atualizada com sucesso.";
            return RedirectToPage("/Settings/Index");
        }
        catch (AppException exception)
        {
            await LoadAsync(id, cancellationToken);
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }

    private async Task LoadAsync(int id, CancellationToken cancellationToken)
    {
        Setting = await _appSettingService.GetByIdAsync(id, cancellationToken);
        Input = new UpdateAppSettingDto
        {
            Value = Setting.Value
        };
    }
}
