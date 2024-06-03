using System.ComponentModel.DataAnnotations;

namespace CurrencyRateService.DTOs;

public class CurrencyRateWriteDTO
{
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