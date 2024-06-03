using CurrencyRateService.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateService.Data;

public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
{
    public DbSet<CurrencyRate> CurrencyRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CurrencyRate>()
            .Property(c => c.RateToUSD)
            .HasColumnType("decimal(18,6)");
    }
}