using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.DTOs.Settings;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Settings;

public class IndexModel : PageModel
{
    private readonly IAppSettingService _appSettingService;

    public IndexModel(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    public IReadOnlyList<AppSettingGroupDto> Groups { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Groups = await _appSettingService.GetGroupedAsync(cancellationToken);
    }

    public static string FormatValue(AppSettingDto item)
    {
        if (item.IsSensitive)
        {
            return "********";
        }

        return item.Value.Length <= 100 ? item.Value : $"{item.Value[..100]}...";
    }
}
