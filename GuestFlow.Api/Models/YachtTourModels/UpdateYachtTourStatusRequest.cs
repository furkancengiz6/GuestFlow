using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.YachtTourModels;

/// <summary>
/// Request model for updating yacht tour status
/// </summary>
public class UpdateYachtTourStatusRequest
{
    /// <summary>
    /// New status for the yacht tour
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
}
