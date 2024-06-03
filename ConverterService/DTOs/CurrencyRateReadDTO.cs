using System.ComponentModel.DataAnnotations;

namespace ConverterService.DTOs;

public class CurrencyRateReadDTO
{
    [Required]
    public required string CurrencyCode { get; set; }

    [Required]
    public required string CurrencyName { get; set; }

    [Required]
    public decimal RateToUSD { get; set; }

    [Required]
    public long UpdatedAt { get; set; }
}