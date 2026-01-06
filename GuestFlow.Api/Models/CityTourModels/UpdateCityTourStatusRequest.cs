using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.CityTourModels;

/// <summary>
/// Request model for updating city tour status
/// </summary>
public class UpdateCityTourStatusRequest
{
    /// <summary>
    /// New status for the city tour
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
}
