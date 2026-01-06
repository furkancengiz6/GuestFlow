using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Transfer.Dtos
{
    /// <summary>
    /// Transfer detay DTO (ilgili veriler ile)
    /// </summary>
    public class TransferDetailDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public TimeSpan? PickupTime { get; set; }
        public TimeSpan? ServiceStartTime { get; set; }
        public DateTime? PickupConfirmationTime { get; set; }
        public DateTime? DropoffConfirmationTime { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string DropoffAddress { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public TransferType TransferType { get; set; } = TransferType.Custom; // Transfer tipi
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        
        // Driver information
        public string? DriverName { get; set; }

        // Guest coordination fields
        public string? ContactPersonName { get; set; }
        public string? MeetingPointDetails { get; set; }

        // Group management fields
        public int? GroupSize { get; set; }
        public int? ChildCount { get; set; }
        public int? InfantCount { get; set; }

        // Communication fields
        public string? GuestLanguage { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? PrimaryContactPhone { get; set; }
        public string? SecondaryContactPhone { get; set; }

        // External driver information
        public string? ExternalVehiclePlate { get; set; }
        public string? ExternalDriverName { get; set; }
        public string? ExternalDriverPhone { get; set; }

        // Service quality fields
        public string? AccessibilityRequirements { get; set; }
        public string? SpecialHandlingNotes { get; set; }

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; }
        public string? GuestVisibleNotes { get; set; }

        // Priority and transport fields
        public GuestFlow.Domain.Entities.Enum.TransferPriority Priority { get; set; }
        public GuestFlow.Domain.Entities.Enum.TransportMode? TransportMode { get; set; }
        public int? LuggageCount { get; set; }
        public bool IsVip { get; set; }

        public DateTime CreatedDate { get; set; }

        // İlişkili Veriler
        public TransferGuestDto? Guest { get; set; }
        public TransferPersonnelDto? Personnel { get; set; }
        public TransferVehicleDto? Vehicle { get; set; }
        public TransferAirportDto? Airport { get; set; }
        public TransferCityDto? PickupCity { get; set; }
        public TransferCityDto? DropoffCity { get; set; }

        // İstatistikler
        public TransferStatisticsDto? Statistics { get; set; }
    }

    /// <summary>
    /// Transfer misafir DTO
    /// </summary>
    public class TransferGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public bool IsSpecialGuest { get; set; }
    }

    /// <summary>
    /// Transfer personel DTO
    /// </summary>
    public class TransferPersonnelDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string UserType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transfer araç DTO
    /// </summary>
    public class TransferVehicleDto
    {
        public int Id { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? LicensePlate { get; set; }
    }

    /// <summary>
    /// Transfer havalimanı DTO
    /// </summary>
    public class TransferAirportDto
    {
        public int Id { get; set; }
        public string AirportName { get; set; } = string.Empty;
        public string? CityName { get; set; }
        public string? Country { get; set; }
    }

    /// <summary>
    /// Transfer şehir DTO
    /// </summary>
    public class TransferCityDto
    {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? Country { get; set; }
    }

    /// <summary>
    /// Transfer istatistikleri DTO
    /// </summary>
    public class TransferStatisticsDto
    {
        public int TotalTransfers { get; set; }
        public int CompletedTransfers { get; set; }
        public int PendingTransfers { get; set; }
        public int InProgressTransfers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
    }

    /// <summary>
    /// Transfer takvim öğesi DTO
    /// </summary>
    public class TransferCalendarItemDto
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public TimeSpan? PickupTime { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string DropoffAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string? PersonnelName { get; set; }
        public string? DriverName { get; set; }
        public string? VehicleName { get; set; }
        public decimal FinalPrice { get; set; }
        public TransferType TransferType { get; set; }
        public int? GroupSize { get; set; }
    }

    /// <summary>
    /// Transfer takvim görünümü DTO
    /// </summary>
    public class TransferCalendarDto
    {
        public List<TransferCalendarItemDto> Today { get; set; } = new List<TransferCalendarItemDto>();
        public List<TransferCalendarItemDto> ThisWeek { get; set; } = new List<TransferCalendarItemDto>();
        public List<TransferCalendarItemDto> ThisMonth { get; set; } = new List<TransferCalendarItemDto>();
        public int TotalUpcoming { get; set; }
    }
}

