using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyRateService.DTOs;

public class ExchangeRateAPIReadDTO
{
    [Required]
    [JsonPropertyName("time_last_update_unix")]
    public long TimeLastUpdateUnix { get; set; }

    [Required]
    [JsonPropertyName("time_next_update_unix")]
    public long TimeNextUpdateUnix { get; set; }

    [Required]
    [JsonPropertyName("base_code")]
    public required string BaseCode { get; set; }

    [Required]
    [JsonPropertyName("conversion_rates")]
    public required Dictionary<string, decimal> ConversionRates { get; set; }
}