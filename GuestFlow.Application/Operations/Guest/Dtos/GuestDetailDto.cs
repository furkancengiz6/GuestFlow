using System;
using System.Collections.Generic;
using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Application.Operations.Guest.Dtos
{
    /// <summary>
    /// Misafir detay DTO (geçmiş ile)
    /// </summary>
    public class GuestDetailDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [MaskPii(PiiType.Email)]
        public string? Email { get; set; }
        [MaskPii(PiiType.Phone)]
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public bool IsSpecialGuest { get; set; }

        // PMS Entegrasyon Bilgileri
        public int? PMSIntegrationId { get; set; }
        public string? PMSGuestId { get; set; }

        // Emergency contact information
        public string? EmergencyContactName { get; set; }
        [MaskPii(PiiType.Phone)]
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }

        public DateTime CreatedDate { get; set; }

        // İstatistikler
        public GuestStatisticsDto Statistics { get; set; } = new GuestStatisticsDto();

        // Geçmiş
        public List<GuestTransferDto> Transfers { get; set; } = new List<GuestTransferDto>();
        public List<GuestCityTourDto> CityTours { get; set; } = new List<GuestCityTourDto>();
        public List<GuestYachtTourDto> YachtTours { get; set; } = new List<GuestYachtTourDto>();
        public List<GuestInvoiceDto> Invoices { get; set; } = new List<GuestInvoiceDto>();

        // Zaman Çizelgesi (kronolojik sırada)
        public List<GuestTimelineItemDto> Timeline { get; set; } = new List<GuestTimelineItemDto>();
    }

    /// <summary>
    /// Misafir istatistikleri DTO
    /// </summary>
    public class GuestStatisticsDto
    {
        public int TotalTransfers { get; set; }
        public int TotalCityTours { get; set; }
        public int TotalYachtTours { get; set; }
        public int TotalBookings { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageBookingValue { get; set; }
        public DateTime? FirstBookingDate { get; set; }
        public DateTime? LastBookingDate { get; set; }
    }

    /// <summary>
    /// Misafir transfer DTO
    /// </summary>
    public class GuestTransferDto
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string DropoffAddress { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsFromAirport { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Misafir şehir turu DTO
    /// </summary>
    public class GuestCityTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string? CityName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Misafir yat turu DTO
    /// </summary>
    public class GuestYachtTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string YachtName { get; set; } = string.Empty;
        public string? CityName { get; set; }
        public string? SpecialRequest { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Misafir fatura DTO
    /// </summary>
    public class GuestInvoiceDto
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string PdfUrl { get; set; } = string.Empty;
        public bool HasPdf => !string.IsNullOrEmpty(PdfUrl);
        public string? ServiceType { get; set; } // Transfer, CityTour, YachtTour
        public int? ServiceId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Misafir zaman çizelgesi öğesi DTO
    /// </summary>
    public class GuestTimelineItemDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Transfer, CityTour, YachtTour, Invoice
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}

