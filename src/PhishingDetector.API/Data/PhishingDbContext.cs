using Microsoft.EntityFrameworkCore;
using PhishingDetector.Core.Models;

namespace PhishingDetector.API.Data;

public class PhishingDbContext : DbContext
{
    public PhishingDbContext(DbContextOptions<PhishingDbContext> options) : base(options) { }

    public DbSet<EmailAnalysis> EmailAnalyses => Set<EmailAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.Sender).HasMaxLength(255);
            entity.Property(e => e.OverallRisk).HasConversion<string>();
            entity.Property(e => e.Indicators)
                  .HasConversion(
                      v => string.Join("||", v),
                      v => v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.HasIndex(e => e.AnalyzedAt);
            entity.HasIndex(e => e.OverallRisk);
        });
    }
}
