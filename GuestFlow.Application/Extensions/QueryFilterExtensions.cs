using GuestFlow.Application.Models;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using System;
using System.Linq;

namespace GuestFlow.Application.Extensions
{
    /// <summary>
    /// Query filtreleme için extension metodları
    /// </summary>
    public static class QueryFilterExtensions
    {
        /// <summary>
        /// Misafir sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<GuestEntity> ApplyGuestFilters(
            this IQueryable<GuestEntity> query,
            GuestFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Arama terimi (isim, e-posta, telefon numarası)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(g =>
                    g.FullName.ToLower().Contains(searchTerm) ||
                    (g.Email != null && g.Email.ToLower().Contains(searchTerm)) ||
                    (g.PhoneNumber != null && g.PhoneNumber.Contains(searchTerm)) ||
                    (g.GuestCode != null && g.GuestCode.ToLower().Contains(searchTerm))
                );
            }

            // Uyruk filtresi
            if (!string.IsNullOrWhiteSpace(filters.Nationality))
            {
                query = query.Where(g => g.Nationality == filters.Nationality);
            }

            // Özel misafir filtresi
            if (filters.IsSpecialGuest.HasValue)
            {
                query = query.Where(g => g.IsSpecialGuest == filters.IsSpecialGuest.Value);
            }

            // E-posta filtresi
            if (!string.IsNullOrWhiteSpace(filters.Email))
            {
                query = query.Where(g => g.Email != null && g.Email.ToLower().Contains(filters.Email.ToLower()));
            }

            // Telefon numarası filtresi
            if (!string.IsNullOrWhiteSpace(filters.PhoneNumber))
            {
                query = query.Where(g => g.PhoneNumber != null && g.PhoneNumber.Contains(filters.PhoneNumber));
            }

            return query;
        }

        /// <summary>
        /// Transfer sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<TransferEntity> ApplyTransferFilters(
            this IQueryable<TransferEntity> query,
            TransferFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(t => t.TransferDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(t => t.TransferDate <= filters.EndDate.Value);
            }

            // Durum filtresi
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                query = query.Where(t => t.Status == filters.Status);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(t => t.GuestId == filters.GuestId.Value);
            }

            // Personel ID filtresi
            if (filters.PersonnelId.HasValue)
            {
                query = query.Where(t => t.PersonnelId == filters.PersonnelId.Value);
            }

            // Araç ID filtresi
            if (filters.VehicleId.HasValue)
            {
                query = query.Where(t => t.VehicleId == filters.VehicleId.Value);
            }

            // Havalimanı ID filtresi
            if (filters.AirportId.HasValue)
            {
                query = query.Where(t => t.AirportId == filters.AirportId.Value);
            }

            // Havalimanından mı filtresi
            if (filters.IsFromAirport.HasValue)
            {
                query = query.Where(t => t.IsFromAirport == filters.IsFromAirport.Value);
            }

            // Arama terimi (adres, not için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(t =>
                    (t.PickupAddress != null && t.PickupAddress.ToLower().Contains(searchTerm)) ||
                    (t.DropoffAddress != null && t.DropoffAddress.ToLower().Contains(searchTerm)) ||
                    (t.Note != null && t.Note.ToLower().Contains(searchTerm))
                );
            }

            return query;
        }

        /// <summary>
        /// Şehir Turu sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<CityTourEntity> ApplyCityTourFilters(
            this IQueryable<CityTourEntity> query,
            CityTourFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(ct => ct.TourDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(ct => ct.TourDate <= filters.EndDate.Value);
            }

            // Şehir ID filtresi
            if (filters.CityId.HasValue)
            {
                query = query.Where(ct => ct.CityId == filters.CityId.Value);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(ct => ct.OwnerGuestId == filters.GuestId.Value);
            }

            // Personel ID filtresi
            if (filters.PersonnelId.HasValue)
            {
                query = query.Where(ct => ct.PersonnelId == filters.PersonnelId.Value);
            }

            // Arama terimi (dil için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(ct =>
                    (ct.Language != null && ct.Language.ToLower().Contains(searchTerm))
                );
            }

            return query;
        }

        /// <summary>
        /// Yat Turu sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<YachtTourEntity> ApplyYachtTourFilters(
            this IQueryable<YachtTourEntity> query,
            YachtTourFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(yt => yt.TourDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(yt => yt.TourDate <= filters.EndDate.Value);
            }

            // Şehir ID filtresi
            if (filters.CityId.HasValue)
            {
                query = query.Where(yt => yt.CityId == filters.CityId.Value);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(yt => yt.OwnerGuestId == filters.GuestId.Value);
            }

            // Personel ID filtresi
            if (filters.PersonnelId.HasValue)
            {
                query = query.Where(yt => yt.PersonnelId == filters.PersonnelId.Value);
            }

            // Arama terimi (yat adı, özel istek için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(yt =>
                    (yt.YachtName != null && yt.YachtName.ToLower().Contains(searchTerm)) ||
                    (yt.SpecialRequest != null && yt.SpecialRequest.ToLower().Contains(searchTerm))
                );
            }

            return query;
        }

        /// <summary>
        /// Fatura sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<InvoicesEntity> ApplyInvoiceFilters(
            this IQueryable<InvoicesEntity> query,
            InvoiceFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(i => i.IssueDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(i => i.IssueDate <= filters.EndDate.Value);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(i => i.GuestId == filters.GuestId.Value);
            }

            // Tutar aralığı filtresi
            if (filters.MinAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount >= filters.MinAmount.Value);
            }

            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount <= filters.MaxAmount.Value);
            }

            // Para birimi filtresi
            if (!string.IsNullOrWhiteSpace(filters.Currency))
            {
                query = query.Where(i => i.Currency == filters.Currency);
            }

            // Personel ID filtresi
            if (filters.PersonnelId.HasValue)
            {
                query = query.Where(i => i.PersonnelId == filters.PersonnelId.Value);
            }

            // PDF durumu filtresi
            if (filters.HasPdf.HasValue)
            {
                if (filters.HasPdf.Value)
                {
                    query = query.Where(i => !string.IsNullOrEmpty(i.PdfUrl));
                }
                else
                {
                    query = query.Where(i => string.IsNullOrEmpty(i.PdfUrl));
                }
            }

            // Hizmet tipi filtresi
            if (!string.IsNullOrWhiteSpace(filters.ServiceType))
            {
                var serviceType = filters.ServiceType.ToLower();
                query = query.Where(i => i.InvoiceItems.Any(item => item.ServiceType.ToLower() == serviceType));
            }

            // Hizmet ID filtresi
            if (filters.ServiceId.HasValue)
            {
                query = query.Where(i => i.InvoiceItems.Any(item => item.ServiceId == filters.ServiceId.Value));
            }

            // Arama terimi (fatura numarası veya misafir adı için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(i =>
                    i.InvoiceNumber.ToString().Contains(searchTerm) ||
                    (i.Guest != null && i.Guest.FullName.ToLower().Contains(searchTerm))
                );
            }

            // Tutar aralığı filtresi
            if (filters.MinAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount >= filters.MinAmount.Value);
            }

            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount <= filters.MaxAmount.Value);
            }

            return query;
        }

        /// <summary>
        /// Personel sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<PersonnelEntity> ApplyPersonnelFilters(
            this IQueryable<PersonnelEntity> query,
            PersonnelFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Arama terimi (isim, e-posta)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(searchTerm) ||
                    (p.Email != null && p.Email.ToLower().Contains(searchTerm)));
            }

            // Kullanıcı tipi filtresi
            if (!string.IsNullOrWhiteSpace(filters.UserType))
            {
                if (Enum.TryParse<UserType>(filters.UserType, true, out var userType))
                {
                    query = query.Where(p => p.UserType == userType);
                }
            }

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate <= filters.EndDate.Value);
            }

            return query;
        }

        /// <summary>
        /// Rezervasyon sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<ReservationEntity> ApplyReservationFilters(
            this IQueryable<ReservationEntity> query,
            ReservationFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate <= filters.EndDate.Value);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(r => r.GuestId == filters.GuestId.Value);
            }

            // Personel ID filtresi
            if (filters.PersonnelId.HasValue)
            {
                query = query.Where(r => r.PersonnelId == filters.PersonnelId.Value);
            }

            // Durum filtresi
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                var status = ReservationStatusHelper.FromString(filters.Status);
                query = query.Where(r => r.Status == status);
            }

            // Servis tipi filtresi
            if (!string.IsNullOrWhiteSpace(filters.ServiceType))
            {
                var serviceType = filters.ServiceType.ToLower();
                query = query.Where(r => r.ServiceType.ToLower() == serviceType);
            }

            // Servis ID filtresi
            if (filters.ServiceId.HasValue)
            {
                query = query.Where(r => r.ServiceId == filters.ServiceId.Value);
            }

            // Arama terimi (rezervasyon numarası, misafir adı için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.ReservationNumber.ToLower().Contains(searchTerm) ||
                    (r.Guest != null && r.Guest.FullName.ToLower().Contains(searchTerm)) ||
                    (r.Guest != null && r.Guest.GuestCode != null && r.Guest.GuestCode.ToLower().Contains(searchTerm))
                );
            }

            return query;
        }

        /// <summary>
        /// Ödeme sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<PaymentEntity> ApplyPaymentFilters(
            this IQueryable<PaymentEntity> query,
            PaymentFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= filters.EndDate.Value);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(p => p.GuestId == filters.GuestId.Value);
            }

            // Fatura ID filtresi
            if (filters.InvoiceId.HasValue)
            {
                query = query.Where(p => p.InvoiceId == filters.InvoiceId.Value);
            }

            // Durum filtresi
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                var status = PaymentStatusHelper.FromString(filters.Status);
                query = query.Where(p => p.Status == status);
            }

            // Ödeme yöntemi filtresi
            if (!string.IsNullOrWhiteSpace(filters.PaymentMethod))
            {
                var method = PaymentMethodHelper.FromString(filters.PaymentMethod);
                query = query.Where(p => p.PaymentMethod == method);
            }

            // Minimum tutar filtresi
            if (filters.MinAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= filters.MinAmount.Value);
            }

            // Maksimum tutar filtresi
            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= filters.MaxAmount.Value);
            }

            // Para birimi filtresi
            if (!string.IsNullOrWhiteSpace(filters.Currency))
            {
                query = query.Where(p => p.Currency == filters.Currency);
            }

            // Arama terimi (ödeme numarası, transaction ID, misafir adı için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.PaymentNumber.ToLower().Contains(searchTerm) ||
                    (p.TransactionId != null && p.TransactionId.ToLower().Contains(searchTerm)) ||
                    (p.Guest != null && p.Guest.FullName.ToLower().Contains(searchTerm)) ||
                    (p.Guest != null && p.Guest.GuestCode != null && p.Guest.GuestCode.ToLower().Contains(searchTerm))
                );
            }

            return query;
        }

        /// <summary>
        /// SMS sorgusuna filtreleme uygular
        /// </summary>
        public static IQueryable<SmsHistoryEntity> ApplySmsFilters(
            this IQueryable<SmsHistoryEntity> query,
            SmsFilterParameters? filters)
        {
            if (filters == null)
                return query;

            // Tarih aralığı filtresi
            if (filters.StartDate.HasValue)
            {
                query = query.Where(s => s.SentDate >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(s => s.SentDate <= filters.EndDate.Value);
            }

            // Misafir ID filtresi
            if (filters.GuestId.HasValue)
            {
                query = query.Where(s => s.GuestId == filters.GuestId.Value);
            }

            // Personel ID filtresi
            if (filters.PersonnelId.HasValue)
            {
                query = query.Where(s => s.PersonnelId == filters.PersonnelId.Value);
            }

            // Durum filtresi
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                var status = SmsStatusHelper.FromString(filters.Status);
                query = query.Where(s => s.Status == status);
            }

            // SMS tipi filtresi
            if (!string.IsNullOrWhiteSpace(filters.SmsType))
            {
                query = query.Where(s => s.SmsType == filters.SmsType);
            }

            // İlişkili entity tipi filtresi
            if (!string.IsNullOrWhiteSpace(filters.RelatedEntityType))
            {
                query = query.Where(s => s.RelatedEntityType == filters.RelatedEntityType);
            }

            // İlişkili entity ID filtresi
            if (filters.RelatedEntityId.HasValue)
            {
                query = query.Where(s => s.RelatedEntityId == filters.RelatedEntityId.Value);
            }

            // Provider filtresi
            if (!string.IsNullOrWhiteSpace(filters.Provider))
            {
                query = query.Where(s => s.Provider == filters.Provider);
            }

            // Telefon numarası filtresi
            if (!string.IsNullOrWhiteSpace(filters.PhoneNumber))
            {
                query = query.Where(s => s.PhoneNumber.Contains(filters.PhoneNumber));
            }

            // Arama terimi (mesaj içeriği, telefon numarası için)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.Message.ToLower().Contains(searchTerm) ||
                    s.PhoneNumber.Contains(searchTerm) ||
                    (s.Guest != null && s.Guest.FullName.ToLower().Contains(searchTerm)) ||
                    (s.Guest != null && s.Guest.GuestCode != null && s.Guest.GuestCode.ToLower().Contains(searchTerm))
                );
            }

            return query;
        }
    }
}

