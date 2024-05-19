using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateService.Models;

[Index(nameof(CurrencyCode), Name = "IX_CurrencyCode")]
public class CurrencyRate
{
    [Key]
    public uint Id { get; set; }

    [Required]
    public required string CurrencyCode { get; set; }

    [Required]
    public required string CurrencyName { get; set; }

    [Required]
    public decimal RateToUSD { get; set; }

    [Required]
    public long UpdatedAt { get; set; }

    [Required]
    public long NextUpdateAt { get; set; }
}