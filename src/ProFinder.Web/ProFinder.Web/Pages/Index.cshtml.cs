using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.DTOs.Dashboard;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;

    public IndexModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public DashboardSummaryDto Summary { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Summary = await _dashboardService.GetSummaryAsync(cancellationToken);
    }
}
