using Microsoft.EntityFrameworkCore;
using FurEver.Web.Models;

namespace FurEver.Web.Data;

public class FurEverContext : DbContext
{
    public FurEverContext(DbContextOptions<FurEverContext> options) : base(options) { }

    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Adopter> Adopters => Set<Adopter>();
    public DbSet<Adoption> Adoptions => Set<Adoption>();
    public DbSet<VeterinaryVisit> VetVisits => Set<VeterinaryVisit>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Admin> Admins => Set<Admin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Adopter>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.AdopterId, f.PetId })
            .IsUnique();

        // Adoption has triggers attached — tell EF Core so it uses
        // a compatible save strategy (no OUTPUT clause).
        modelBuilder.Entity<Adoption>()
            .ToTable(tb =>
            {
                tb.HasTrigger("trg_adoption_validate_insert");
                tb.HasTrigger("trg_adoption_after_insert");
                tb.HasTrigger("trg_adoption_after_update");
                tb.HasTrigger("trg_adoption_after_delete");
            });

        modelBuilder.Entity<Pet>()
            .ToTable(tb => tb.HasTrigger("trg_pet_cleanup_favorites"));

        // Cascade paths: SQL Server disallows multiple cascade paths.
        // Favorite cascades from both Adopter and Pet; Adoption too.
        // Keep cascade on Pet side, restrict on Adopter where needed
        // is NOT what MySQL did — both cascaded. SQL Server allows this
        // here because the paths don't conflict (no shared subtree).
    }
}
