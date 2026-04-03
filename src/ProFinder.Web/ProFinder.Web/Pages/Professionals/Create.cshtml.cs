using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.ProviderLeads;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professionals;

public class CreateModel : ProfessionalFormPageModel
{
    private const string DefaultStatusName = "Novo";

    private readonly IProfessionalService _professionalService;
    private readonly IProviderLeadService _providerLeadService;

    public CreateModel(
        IProfessionalService professionalService,
        IProviderLeadService providerLeadService,
        ILookupService lookupService)
        : base(lookupService)
    {
        _professionalService = professionalService;
        _providerLeadService = providerLeadService;
    }

    [BindProperty(SupportsGet = true)]
    public int? LeadId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool IsConversionMode => LeadId.HasValue;

    public string PageTitle => IsConversionMode ? "Converter lead" : "Novo profissional";

    public string PageDescription => IsConversionMode
        ? "Revise os dados capturados e conclua a conversao do lead em profissional."
        : "Cadastre um profissional e vincule sua atuacao por regiao.";

    public string SubmitLabel => IsConversionMode ? "Converter e cadastrar" : "Salvar";

    public string CancelUrl =>
        GetSafeLocalReturnUrl()
        ?? (IsConversionMode
            ? Url.Page("/ProviderLeads/Index")
            : Url.Page("/Professionals/Index"))
        ?? "/";

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            await LoadOptionsAsync(cancellationToken);
            await InitializeInputAsync(cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToCancelFallback();
        }
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
            if (IsConversionMode)
            {
                var professionalId = await _providerLeadService.ConvertToProfessionalAsync(LeadId!.Value, Input, cancellationToken);
                TempData["SuccessMessage"] = "Lead convertido em profissional com sucesso.";
                return RedirectToSuccessDestination(professionalId);
            }

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

    private async Task InitializeInputAsync(CancellationToken cancellationToken)
    {
        Input.IsAutonomous = true;
        Input.IsActive = true;
        Input.ProfessionIds = [];

        if (!IsConversionMode)
        {
            Input.StatusId = await ResolveDefaultStatusIdAsync(cancellationToken);
            return;
        }

        var lead = await _providerLeadService.GetByIdAsync(LeadId!.Value, cancellationToken);

        if (lead.ImportedProfessionalId.HasValue || string.Equals(lead.ImportStatus, "Imported", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Este lead ja foi convertido em profissional.");
        }

        var professionIds = lead.Professions
            .Select(x => x.Id)
            .Where(x => x > 0)
            .ToList();

        if (lead.ProfessionId.HasValue && lead.ProfessionId.Value > 0 && !professionIds.Contains(lead.ProfessionId.Value))
        {
            professionIds.Insert(0, lead.ProfessionId.Value);
        }

        var primaryProfessionId = lead.ProfessionId
            ?? professionIds.FirstOrDefault();

        var regionIds = lead.RegionId.HasValue
            ? new List<int> { lead.RegionId.Value }
            : [];

        Input = new()
        {
            FullName = lead.Name,
            Phone = lead.Phone,
            WhatsApp = lead.WhatsApp,
            Website = lead.Website,
            ProfessionId = primaryProfessionId,
            ProfessionIds = professionIds,
            SourceId = lead.LeadSourceId,
            StatusId = await ResolveDefaultStatusIdAsync(cancellationToken),
            IsAutonomous = true,
            IsActive = true,
            RegionIds = regionIds,
            PrimaryRegionId = lead.RegionId
        };
    }

    private async Task<int> ResolveDefaultStatusIdAsync(CancellationToken cancellationToken)
    {
        var statuses = await LookupService.GetStatusesAsync(cancellationToken: cancellationToken);
        return statuses.FirstOrDefault(x => string.Equals(x.Name, DefaultStatusName, StringComparison.OrdinalIgnoreCase))?.Id
            ?? statuses.FirstOrDefault()?.Id
            ?? 0;
    }

    private IActionResult RedirectToSuccessDestination(int professionalId)
    {
        var safeReturnUrl = GetSafeLocalReturnUrl();
        if (!string.IsNullOrWhiteSpace(safeReturnUrl))
        {
            return LocalRedirect(safeReturnUrl);
        }

        return RedirectToPage("/Professionals/Details", new { id = professionalId });
    }

    private IActionResult RedirectToCancelFallback()
    {
        var safeReturnUrl = GetSafeLocalReturnUrl();
        if (!string.IsNullOrWhiteSpace(safeReturnUrl))
        {
            return LocalRedirect(safeReturnUrl);
        }

        return RedirectToPage(IsConversionMode ? "/ProviderLeads/Index" : "/Professionals/Index");
    }

    private string? GetSafeLocalReturnUrl()
    {
        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : null;
    }
}
