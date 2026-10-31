using CampusDormitory.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusDormitory.Api.Data;

public class CampusDbContext : DbContext
{
    public CampusDbContext(DbContextOptions<CampusDbContext> options) : base(options)
    {
    }

    public DbSet<Dormitory> Dormitories => Set<Dormitory>();
    public DbSet<Building> Buildings => Set<Building>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dormitory>().ToTable("Dormitory");
        modelBuilder.Entity<Building>().ToTable("Buildings");

        modelBuilder.Entity<Dormitory>()
            .HasOne(dormitory => dormitory.Building)
            .WithMany()
            .HasForeignKey(dormitory => dormitory.BuildingId);
    }
}
