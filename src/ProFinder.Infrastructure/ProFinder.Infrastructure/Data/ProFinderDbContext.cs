using Microsoft.EntityFrameworkCore;
using ProFinder.Domain.Common;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data.Seed;

namespace ProFinder.Infrastructure.Data;

public class ProFinderDbContext : DbContext
{
    public ProFinderDbContext(DbContextOptions<ProFinderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Professional> Professionals => Set<Professional>();

    public DbSet<ProfessionalProfession> ProfessionalProfessions => Set<ProfessionalProfession>();

    public DbSet<Profession> Professions => Set<Profession>();

    public DbSet<Region> Regions => Set<Region>();

    public DbSet<ProfessionalRegion> ProfessionalRegions => Set<ProfessionalRegion>();

    public DbSet<LeadSource> LeadSources => Set<LeadSource>();

    public DbSet<LeadStatus> LeadStatuses => Set<LeadStatus>();

    public DbSet<Interaction> Interactions => Set<Interaction>();

    public DbSet<Evidence> Evidences => Set<Evidence>();

    public DbSet<LeadCaptureRun> LeadCaptureRuns => Set<LeadCaptureRun>();

    public DbSet<LeadCaptureLog> LeadCaptureLogs => Set<LeadCaptureLog>();

    public DbSet<GoogleMapsLead> GoogleMapsLeads => Set<GoogleMapsLead>();

    public DbSet<ProviderLead> ProviderLeads => Set<ProviderLead>();

    public DbSet<ProviderLeadProfession> ProviderLeadProfessions => Set<ProviderLeadProfession>();

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProFinderDbContext).Assembly);
        SeedData.Apply(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    private void ApplyAuditInformation()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
