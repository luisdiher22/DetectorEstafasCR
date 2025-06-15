using Microsoft.EntityFrameworkCore;
using DetectorEstafaCR.Models;

namespace DetectorEstafaCR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ReportedEntry> ReportedEntries { get; set; }

        // If you need to add model configurations (e.g., using Fluent API),
        // you can override OnModelCreating method here.
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        //     // Example: modelBuilder.Entity<ReportedEntry>().ToTable("ReportedScams");
        // }
    }
}
