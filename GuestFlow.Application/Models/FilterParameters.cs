using System;

namespace GuestFlow.Application.Models
{
    /// <summary>
    /// Misafir filtreleme parametreleri
    /// </summary>
    public class GuestFilterParameters
    {
        /// <summary>
        /// Arama terimi (isim, e-posta, telefon numarası için)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Uyruk filtresi
        /// </summary>
        public string? Nationality { get; set; }

        /// <summary>
        /// Özel misafir filtresi
        /// </summary>
        public bool? IsSpecialGuest { get; set; }

        /// <summary>
        /// E-posta filtresi
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Telefon numarası filtresi
        /// </summary>
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Transfer filtreleme parametreleri
    /// </summary>
    public class TransferFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Durum filtresi
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID filtresi
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// Araç ID filtresi
        /// </summary>
        public int? VehicleId { get; set; }

        /// <summary>
        /// Havalimanı ID filtresi
        /// </summary>
        public int? AirportId { get; set; }

        /// <summary>
        /// Havalimanından mı filtresi
        /// </summary>
        public bool? IsFromAirport { get; set; }

        /// <summary>
        /// Arama terimi (adres, not için)
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Şehir Turu filtreleme parametreleri
    /// </summary>
    public class CityTourFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Şehir ID filtresi
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID filtresi
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// Arama terimi (tur adı, not için)
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Yat Turu filtreleme parametreleri
    /// </summary>
    public class YachtTourFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Şehir ID filtresi
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID filtresi
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// Arama terimi (yat adı, not için)
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Fatura filtreleme parametreleri
    /// </summary>
    public class InvoiceFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Minimum tutar
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Maksimum tutar
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Para birimi filtresi
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Personel ID filtresi
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// PDF durumu filtresi
        /// </summary>
        public bool? HasPdf { get; set; }

        /// <summary>
        /// Hizmet tipi filtresi (Transfer, CityTour, YachtTour)
        /// </summary>
        public string? ServiceType { get; set; }

        /// <summary>
        /// Hizmet ID filtresi
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Arama terimi (fatura numarası veya misafir adı için)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Fatura numarası arama
        /// </summary>
        public int? InvoiceNumber { get; set; }
    }

    /// <summary>
    /// Personel filtreleme parametreleri
    /// </summary>
    public class PersonnelFilterParameters
    {
        /// <summary>
        /// Arama terimi (isim, e-posta)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Kullanıcı tipi filtresi
        /// </summary>
        public string? UserType { get; set; }

        /// <summary>
        /// Başlangıç tarihi (oluşturulma tarihi)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (oluşturulma tarihi)
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Rezervasyon filtreleme parametreleri
    /// </summary>
    public class ReservationFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi (rezervasyon tarihi)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (rezervasyon tarihi)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID filtresi
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// Rezervasyon durumu filtresi
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Servis tipi filtresi (Transfer, CityTour, YachtTour)
        /// </summary>
        public string? ServiceType { get; set; }

        /// <summary>
        /// Servis ID filtresi
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Arama terimi (rezervasyon numarası, misafir adı için)
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Ödeme filtreleme parametreleri
    /// </summary>
    public class PaymentFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi (ödeme tarihi)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (ödeme tarihi)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Fatura ID filtresi
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Ödeme durumu filtresi
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Ödeme yöntemi filtresi
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Minimum tutar
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Maksimum tutar
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Para birimi filtresi
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Arama terimi (ödeme numarası, transaction ID, misafir adı için)
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// SMS filtreleme parametreleri
    /// </summary>
    public class SmsFilterParameters
    {
        /// <summary>
        /// Başlangıç tarihi (gönderim tarihi)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (gönderim tarihi)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Misafir ID filtresi
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID filtresi
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// SMS durumu filtresi
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// SMS tipi filtresi
        /// </summary>
        public string? SmsType { get; set; }

        /// <summary>
        /// İlişkili entity tipi filtresi
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID filtresi
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Provider filtresi
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// Telefon numarası filtresi
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Arama terimi (mesaj içeriği, telefon numarası için)
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
