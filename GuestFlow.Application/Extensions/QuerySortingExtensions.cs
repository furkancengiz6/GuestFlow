using GuestFlow.Application.Models;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using System;
using System.Linq;

namespace GuestFlow.Application.Extensions
{
    /// <summary>
    /// Query sıralama için extension metodları
    /// </summary>
    public static class QuerySortingExtensions
    {
        /// <summary>
        /// Misafir sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<GuestEntity> ApplyGuestSorting(
            this IQueryable<GuestEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: CreatedDate desc (en yeni önce)
                return query.OrderByDescending(g => g.CreatedDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(g => g.Id) 
                    : query.OrderByDescending(g => g.Id),
                "fullname" or "name" => direction == SortDirection.Ascending 
                    ? query.OrderBy(g => g.FullName) 
                    : query.OrderByDescending(g => g.FullName),
                "email" => direction == SortDirection.Ascending 
                    ? query.OrderBy(g => g.Email) 
                    : query.OrderByDescending(g => g.Email),
                "nationality" => direction == SortDirection.Ascending 
                    ? query.OrderBy(g => g.Nationality) 
                    : query.OrderByDescending(g => g.Nationality),
                "guestcode" or "code" => direction == SortDirection.Ascending 
                    ? query.OrderBy(g => g.GuestCode) 
                    : query.OrderByDescending(g => g.GuestCode),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(g => g.CreatedDate) 
                    : query.OrderByDescending(g => g.CreatedDate),
                _ => query.OrderByDescending(g => g.CreatedDate) // Varsayılan
            };
        }

        /// <summary>
        /// Transfer sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<TransferEntity> ApplyTransferSorting(
            this IQueryable<TransferEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: TransferDate desc (en yeni önce)
                return query.OrderByDescending(t => t.TransferDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(t => t.Id) 
                    : query.OrderByDescending(t => t.Id),
                "transferdate" or "date" => direction == SortDirection.Ascending 
                    ? query.OrderBy(t => t.TransferDate) 
                    : query.OrderByDescending(t => t.TransferDate),
                "price" => direction == SortDirection.Ascending 
                    ? query.OrderBy(t => t.Price) 
                    : query.OrderByDescending(t => t.Price),
                "status" => direction == SortDirection.Ascending 
                    ? query.OrderBy(t => t.Status) 
                    : query.OrderByDescending(t => t.Status),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(t => t.CreatedDate) 
                    : query.OrderByDescending(t => t.CreatedDate),
                _ => query.OrderByDescending(t => t.TransferDate) // Varsayılan
            };
        }

        /// <summary>
        /// Şehir Turu sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<CityTourEntity> ApplyCityTourSorting(
            this IQueryable<CityTourEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: TourDate desc (en yeni önce)
                return query.OrderByDescending(ct => ct.TourDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(ct => ct.Id) 
                    : query.OrderByDescending(ct => ct.Id),
                "tourdate" or "date" => direction == SortDirection.Ascending 
                    ? query.OrderBy(ct => ct.TourDate) 
                    : query.OrderByDescending(ct => ct.TourDate),
                "price" => direction == SortDirection.Ascending 
                    ? query.OrderBy(ct => ct.Price) 
                    : query.OrderByDescending(ct => ct.Price),
                "finalprice" => direction == SortDirection.Ascending 
                    ? query.OrderBy(ct => ct.FinalPrice) 
                    : query.OrderByDescending(ct => ct.FinalPrice),
                "durationhours" or "duration" => direction == SortDirection.Ascending 
                    ? query.OrderBy(ct => ct.DurationHours) 
                    : query.OrderByDescending(ct => ct.DurationHours),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(ct => ct.CreatedDate) 
                    : query.OrderByDescending(ct => ct.CreatedDate),
                _ => query.OrderByDescending(ct => ct.TourDate) // Varsayılan
            };
        }

        /// <summary>
        /// Yat Turu sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<YachtTourEntity> ApplyYachtTourSorting(
            this IQueryable<YachtTourEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: TourDate desc (en yeni önce)
                return query.OrderByDescending(yt => yt.TourDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.Id) 
                    : query.OrderByDescending(yt => yt.Id),
                "tourdate" or "date" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.TourDate) 
                    : query.OrderByDescending(yt => yt.TourDate),
                "price" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.Price) 
                    : query.OrderByDescending(yt => yt.Price),
                "finalprice" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.FinalPrice) 
                    : query.OrderByDescending(yt => yt.FinalPrice),
                "numberofpeople" or "people" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.NumberOfPeople) 
                    : query.OrderByDescending(yt => yt.NumberOfPeople),
                "yachtname" or "yacht" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.YachtName) 
                    : query.OrderByDescending(yt => yt.YachtName),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(yt => yt.CreatedDate) 
                    : query.OrderByDescending(yt => yt.CreatedDate),
                _ => query.OrderByDescending(yt => yt.TourDate) // Varsayılan
            };
        }

        /// <summary>
        /// Fatura sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<InvoicesEntity> ApplyInvoiceSorting(
            this IQueryable<InvoicesEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: IssueDate desc (en yeni önce)
                return query.OrderByDescending(i => i.IssueDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(i => i.Id) 
                    : query.OrderByDescending(i => i.Id),
                "invoicenumber" or "number" => direction == SortDirection.Ascending 
                    ? query.OrderBy(i => i.InvoiceNumber) 
                    : query.OrderByDescending(i => i.InvoiceNumber),
                "issuedate" or "date" => direction == SortDirection.Ascending 
                    ? query.OrderBy(i => i.IssueDate) 
                    : query.OrderByDescending(i => i.IssueDate),
                "totalamount" or "amount" => direction == SortDirection.Ascending 
                    ? query.OrderBy(i => i.TotalAmount) 
                    : query.OrderByDescending(i => i.TotalAmount),
                "currency" => direction == SortDirection.Ascending 
                    ? query.OrderBy(i => i.Currency) 
                    : query.OrderByDescending(i => i.Currency),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(i => i.CreatedDate) 
                    : query.OrderByDescending(i => i.CreatedDate),
                _ => query.OrderByDescending(i => i.IssueDate) // Varsayılan
            };
        }

        /// <summary>
        /// Personel sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<PersonnelEntity> ApplyPersonnelSorting(
            this IQueryable<PersonnelEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: FullName asc
                return query.OrderBy(p => p.FullName);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(p => p.Id) 
                    : query.OrderByDescending(p => p.Id),
                "fullname" or "name" => direction == SortDirection.Ascending 
                    ? query.OrderBy(p => p.FullName) 
                    : query.OrderByDescending(p => p.FullName),
                "email" => direction == SortDirection.Ascending 
                    ? query.OrderBy(p => p.Email) 
                    : query.OrderByDescending(p => p.Email),
                "usertype" or "role" => direction == SortDirection.Ascending 
                    ? query.OrderBy(p => p.UserType) 
                    : query.OrderByDescending(p => p.UserType),
                "createddate" or "date" => direction == SortDirection.Ascending 
                    ? query.OrderBy(p => p.CreatedDate) 
                    : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderBy(p => p.FullName)
            };
        }

        /// <summary>
        /// Araç sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<VehicleEntity> ApplyVehicleSorting(
            this IQueryable<VehicleEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: CreatedDate desc
                return query.OrderByDescending(v => v.CreatedDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(v => v.Id) 
                    : query.OrderByDescending(v => v.Id),
                "type" => direction == SortDirection.Ascending 
                    ? query.OrderBy(v => v.Type) 
                    : query.OrderByDescending(v => v.Type),
                "platenumber" or "plate" => direction == SortDirection.Ascending 
                    ? query.OrderBy(v => v.PlateNumber) 
                    : query.OrderByDescending(v => v.PlateNumber),
                "capacity" => direction == SortDirection.Ascending 
                    ? query.OrderBy(v => v.Capacity) 
                    : query.OrderByDescending(v => v.Capacity),
                "dailyprice" or "price" => direction == SortDirection.Ascending 
                    ? query.OrderBy(v => v.DailyPrice) 
                    : query.OrderByDescending(v => v.DailyPrice),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(v => v.CreatedDate) 
                    : query.OrderByDescending(v => v.CreatedDate),
                _ => query.OrderByDescending(v => v.CreatedDate)
            };
        }

        /// <summary>
        /// Havalimanı sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<AirportEntity> ApplyAirportSorting(
            this IQueryable<AirportEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: Name asc
                return query.OrderBy(a => a.Name);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(a => a.Id) 
                    : query.OrderByDescending(a => a.Id),
                "name" => direction == SortDirection.Ascending 
                    ? query.OrderBy(a => a.Name) 
                    : query.OrderByDescending(a => a.Name),
                "code" => direction == SortDirection.Ascending 
                    ? query.OrderBy(a => a.Code) 
                    : query.OrderByDescending(a => a.Code),
                "cityid" or "city" => direction == SortDirection.Ascending 
                    ? query.OrderBy(a => a.CityId) 
                    : query.OrderByDescending(a => a.CityId),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(a => a.CreatedDate) 
                    : query.OrderByDescending(a => a.CreatedDate),
                _ => query.OrderBy(a => a.Name)
            };
        }

        /// <summary>
        /// Şehir sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<CityEntity> ApplyCitySorting(
            this IQueryable<CityEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: CityName asc
                return query.OrderBy(c => c.CityName);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(c => c.Id) 
                    : query.OrderByDescending(c => c.Id),
                "cityname" or "name" => direction == SortDirection.Ascending 
                    ? query.OrderBy(c => c.CityName) 
                    : query.OrderByDescending(c => c.CityName),
                "country" => direction == SortDirection.Ascending 
                    ? query.OrderBy(c => c.Country) 
                    : query.OrderByDescending(c => c.Country),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(c => c.CreatedDate) 
                    : query.OrderByDescending(c => c.CreatedDate),
                _ => query.OrderBy(c => c.CityName)
            };
        }

        /// <summary>
        /// Rezervasyon sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<ReservationEntity> ApplyReservationSorting(
            this IQueryable<ReservationEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: ReservationDate desc (en yeni önce)
                return query.OrderByDescending(r => r.ReservationDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.Id) 
                    : query.OrderByDescending(r => r.Id),
                "reservationnumber" or "number" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.ReservationNumber) 
                    : query.OrderByDescending(r => r.ReservationNumber),
                "reservationdate" or "date" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.ReservationDate) 
                    : query.OrderByDescending(r => r.ReservationDate),
                "status" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.Status) 
                    : query.OrderByDescending(r => r.Status),
                "totalamount" or "amount" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.TotalAmount) 
                    : query.OrderByDescending(r => r.TotalAmount),
                "servicetype" or "type" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.ServiceType) 
                    : query.OrderByDescending(r => r.ServiceType),
                "createddate" or "created" => direction == SortDirection.Ascending 
                    ? query.OrderBy(r => r.CreatedDate) 
                    : query.OrderByDescending(r => r.CreatedDate),
                _ => query.OrderByDescending(r => r.ReservationDate)
            };
        }

        /// <summary>
        /// Ödeme sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<PaymentEntity> ApplyPaymentSorting(
            this IQueryable<PaymentEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: PaymentDate desc (en yeni önce)
                return query.OrderByDescending(p => p.PaymentDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.Id)
                    : query.OrderByDescending(p => p.Id),
                "paymentnumber" or "number" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.PaymentNumber)
                    : query.OrderByDescending(p => p.PaymentNumber),
                "paymentdate" or "date" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.PaymentDate)
                    : query.OrderByDescending(p => p.PaymentDate),
                "amount" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.Amount)
                    : query.OrderByDescending(p => p.Amount),
                "status" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.Status)
                    : query.OrderByDescending(p => p.Status),
                "paymentmethod" or "method" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.PaymentMethod)
                    : query.OrderByDescending(p => p.PaymentMethod),
                "currency" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.Currency)
                    : query.OrderByDescending(p => p.Currency),
                "createddate" or "created" => direction == SortDirection.Ascending
                    ? query.OrderBy(p => p.CreatedDate)
                    : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.PaymentDate)
            };
        }

        /// <summary>
        /// SMS sorgusuna sıralama uygular
        /// </summary>
        public static IQueryable<SmsHistoryEntity> ApplySmsSorting(
            this IQueryable<SmsHistoryEntity> query,
            SortingParameters? sorting)
        {
            if (sorting == null || string.IsNullOrWhiteSpace(sorting.SortBy))
            {
                // Varsayılan sıralama: SentDate desc (en yeni önce)
                return query.OrderByDescending(s => s.SentDate);
            }

            var sortBy = sorting.SortBy.ToLower();
            var direction = sorting.Direction;

            return sortBy switch
            {
                "id" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.Id)
                    : query.OrderByDescending(s => s.Id),
                "sentdate" or "date" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.SentDate)
                    : query.OrderByDescending(s => s.SentDate),
                "status" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.Status)
                    : query.OrderByDescending(s => s.Status),
                "phonenumber" or "phone" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.PhoneNumber)
                    : query.OrderByDescending(s => s.PhoneNumber),
                "smstype" or "type" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.SmsType)
                    : query.OrderByDescending(s => s.SmsType),
                "provider" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.Provider)
                    : query.OrderByDescending(s => s.Provider),
                "createddate" or "created" => direction == SortDirection.Ascending
                    ? query.OrderBy(s => s.CreatedDate)
                    : query.OrderByDescending(s => s.CreatedDate),
                _ => query.OrderByDescending(s => s.SentDate)
            };
        }
    }
}

