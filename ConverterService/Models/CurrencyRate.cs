using System.ComponentModel.DataAnnotations;

namespace CurrencyRateService.Models;

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