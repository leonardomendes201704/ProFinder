using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professionals;

public class EditModel : ProfessionalFormPageModel
{
    private readonly IProfessionalService _professionalService;

    public EditModel(IProfessionalService professionalService, ILookupService lookupService)
        : base(lookupService)
    {
        _professionalService = professionalService;
    }

    public int Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var professional = await _professionalService.GetByIdAsync(id, cancellationToken);
            Id = id;
            Input = new()
            {
                FullName = professional.FullName,
                BusinessName = professional.BusinessName,
                Phone = professional.Phone,
                WhatsApp = professional.WhatsApp,
                Email = professional.Email,
                DocumentNumber = professional.DocumentNumber,
                ProfessionId = professional.ProfessionId,
                ProfessionIds = professional.Professions.Select(x => x.Id).ToList(),
                SourceId = professional.SourceId,
                StatusId = professional.StatusId,
                Notes = professional.Notes,
                Website = professional.Website,
                Instagram = professional.Instagram,
                IsAutonomous = professional.IsAutonomous,
                IsActive = professional.IsActive,
                RegionIds = professional.Regions.Select(x => x.RegionId).ToList(),
                PrimaryRegionId = professional.Regions.FirstOrDefault(x => x.IsPrimaryRegion)?.RegionId
            };

            await LoadOptionsAsync(cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Professionals/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        Id = id;

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(cancellationToken);
            return Page();
        }

        try
        {
            await _professionalService.UpdateAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Profissional atualizado com sucesso.";
            return RedirectToPage("/Professionals/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadOptionsAsync(cancellationToken);
            return Page();
        }
    }
}
