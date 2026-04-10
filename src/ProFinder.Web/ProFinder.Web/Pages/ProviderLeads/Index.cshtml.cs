using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ProFinder.Application.Common;
using ProFinder.Application.DTOs.ProviderLeads;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.ProviderLeads;

public class IndexModel : PageModel
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private static readonly (string Header, Func<ProviderLeadExportItemDto, object?> ValueFactory)[] ExportColumns =
    [
        ("Lead ID", item => item.Id),
        ("Lote", item => item.LeadCaptureRunId),
        ("Origem", item => item.SourceName),
        ("Lead Source ID", item => item.LeadSourceId),
        ("Site Key", item => item.SiteKey),
        ("Nome", item => item.Name),
        ("Telefone", item => item.Phone),
        ("WhatsApp", item => item.WhatsApp),
        ("Telefone Normalizado", item => item.NormalizedPhone),
        ("Website", item => item.Website),
        ("Busca", item => item.SearchQuery),
        ("Endereco", item => item.Address),
        ("Bairro", item => item.Neighborhood),
        ("Cidade", item => item.City),
        ("Estado", item => item.State),
        ("Localidade", item => item.LocalityDisplay),
        ("Regiao Alvo", item => item.RegionDisplayName),
        ("Profissao Principal", item => item.ProfessionName),
        ("Profissoes", item => item.ProfessionNamesDisplay),
        ("Status", item => item.ImportStatus),
        ("Profissional Importado ID", item => item.ImportedProfessionalId),
        ("Profissional Importado", item => item.ImportedProfessionalName),
        ("Latitude", item => item.Latitude),
        ("Longitude", item => item.Longitude),
        ("Qtd Fontes", item => item.SourceCount),
        ("URL Listing", item => item.SourceListingUrl),
        ("URL Detalhe", item => item.SourceDetailsUrl),
        ("External ID", item => item.ExternalId),
        ("Rating", item => item.Rating),
        ("Reviews", item => item.ReviewCount),
        ("Captado Em", item => item.ScrapedAt.ToLocalTime()),
        ("Criado Em", item => item.CreatedAt.ToLocalTime()),
        ("Atualizado Em", item => item.UpdatedAt.ToLocalTime())
    ];

    private readonly IProviderLeadService _providerLeadService;
    private readonly ILookupService _lookupService;

    public IndexModel(IProviderLeadService providerLeadService, ILookupService lookupService)
    {
        _providerLeadService = providerLeadService;
        _lookupService = lookupService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LeadSourceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ProfessionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? RegionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LeadCaptureRunId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SiteKey { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ImportStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    public PagedResult<ProviderLeadListItemDto> Result { get; private set; } = new();

    public IReadOnlyList<SelectListItem> SourceOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> ProfessionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> RegionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> ImportStatusOptions { get; private set; } = [];

    public string CurrentListUrl =>
        Url.Page("/ProviderLeads/Index", new
        {
            SearchTerm,
            LeadSourceId,
            ProfessionId,
            RegionId,
            LeadCaptureRunId,
            SiteKey,
            City,
            ImportStatus,
            PageNumber,
            PageSize
        }) ?? "/ProviderLeads/Index";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadFilterOptionsAsync(cancellationToken);
        await LoadResultsAsync(cancellationToken);
    }

    public async Task<PartialViewResult> OnGetResultsAsync(CancellationToken cancellationToken)
    {
        await LoadResultsAsync(cancellationToken);

        return new PartialViewResult
        {
            ViewName = "_Results",
            ViewData = new ViewDataDictionary<IndexModel>(ViewData, this)
        };
    }

    public async Task<JsonResult> OnGetMapAsync(CancellationToken cancellationToken)
    {
        var result = await _providerLeadService.GetMapItemsAsync(BuildFilter(), cancellationToken);
        return new JsonResult(result);
    }

    public async Task<FileContentResult> OnGetExportAsync(CancellationToken cancellationToken)
    {
        var items = await _providerLeadService.GetExportItemsAsync(BuildFilter(), cancellationToken);
        using var workbook = BuildExportWorkbook(items);
        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return File(stream.ToArray(), ExcelContentType, BuildExportFileName());
    }

    private async Task LoadFilterOptionsAsync(CancellationToken cancellationToken)
    {
        var sources = await _lookupService.GetSourcesAsync(cancellationToken: cancellationToken);
        var professions = await _lookupService.GetProfessionsAsync(cancellationToken: cancellationToken);
        var regions = await _lookupService.GetRegionsAsync(includeInactive: true, cancellationToken: cancellationToken);

        SourceOptions = sources.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        ProfessionOptions = professions.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        RegionOptions = regions.OrderBy(x => x.DisplayName).Select(x => new SelectListItem(x.DisplayName, x.Id.ToString())).ToList();

        ImportStatusOptions =
        [
            new SelectListItem("Captured", "Captured"),
            new SelectListItem("Imported", "Imported"),
            new SelectListItem("Ignored", "Ignored"),
            new SelectListItem("Errored", "Errored")
        ];
    }

    private async Task LoadResultsAsync(CancellationToken cancellationToken)
    {
        Result = await _providerLeadService.GetPagedAsync(BuildFilter(), cancellationToken);
    }

    private ProviderLeadQueryFilter BuildFilter()
    {
        return new ProviderLeadQueryFilter
        {
            SearchTerm = SearchTerm,
            LeadSourceId = LeadSourceId,
            ProfessionId = ProfessionId,
            RegionId = RegionId,
            LeadCaptureRunId = LeadCaptureRunId,
            SiteKey = SiteKey,
            City = City,
            ImportStatus = ImportStatus,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }

    private XLWorkbook BuildExportWorkbook(IReadOnlyList<ProviderLeadExportItemDto> items)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Leads");

        for (var columnIndex = 0; columnIndex < ExportColumns.Length; columnIndex++)
        {
            var cell = worksheet.Cell(1, columnIndex + 1);
            cell.Value = ExportColumns[columnIndex].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");
        }

        for (var rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            var item = items[rowIndex];

            for (var columnIndex = 0; columnIndex < ExportColumns.Length; columnIndex++)
            {
                SetCellValue(worksheet.Cell(rowIndex + 2, columnIndex + 1), ExportColumns[columnIndex].ValueFactory(item));
            }
        }

        var lastRow = Math.Max(items.Count + 1, 1);
        var lastColumn = ExportColumns.Length;
        var usedRange = worksheet.Range(1, 1, lastRow, lastColumn);
        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.SheetView.FreezeRows(1);
        usedRange.SetAutoFilter();
        worksheet.Columns(1, lastColumn).AdjustToContents();

        return workbook;
    }

    private string BuildExportFileName()
    {
        var timestamp = DateTime.UtcNow.ToLocalTime().ToString("yyyyMMdd-HHmm");
        return LeadCaptureRunId.HasValue
            ? $"provider-leads-lote-{LeadCaptureRunId.Value}-{timestamp}.xlsx"
            : $"provider-leads-{timestamp}.xlsx";
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case string text:
                cell.Value = text;
                break;
            case int number:
                cell.Value = number;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                break;
            case DateTime dateTime:
                cell.Value = dateTime;
                cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }
}
