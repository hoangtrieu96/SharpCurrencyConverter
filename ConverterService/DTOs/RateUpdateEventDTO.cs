using System.ComponentModel.DataAnnotations;

namespace ConverterService.DTOs;

public class RateUpdateEventDTO
{
    [Required]
    public required string MessageId { get; set; }
    [Required]
    public required string Event { get; set; }
    [Required]
    public long Timestamp { get; set; }
    [Required]
    public required string Source { get; set; }
}