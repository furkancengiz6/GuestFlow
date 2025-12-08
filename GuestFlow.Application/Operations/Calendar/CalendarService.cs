using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using IcalCalendar = Ical.Net.Calendar;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Calendar
{
    public class CalendarService : ICalendarService
    {
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<ReservationEntity> _reservationRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CalendarService> _logger;
        private readonly string _appName;

        public CalendarService(
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<ReservationEntity> reservationRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IConfiguration configuration,
            ILogger<CalendarService> logger)
        {
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _reservationRepository = reservationRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _configuration = configuration;
            _logger = logger;
            _appName = _configuration["AppSettings:Name"] ?? "GuestFlow";
        }

        public async Task<CalendarExportResult> GenerateTransferCalendarAsync(int transferId)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Airport)
                    .FirstOrDefaultAsync(t => t.Id == transferId && !t.IsDeleted);

                if (transfer == null)
                {
                    return new CalendarExportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Transfer bulunamadı."
                    };
                }

                var calendar = new IcalCalendar();
                var calendarEvent = CreateTransferEvent(transfer);
                calendar.Events.Add(calendarEvent);

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytes = Encoding.UTF8.GetBytes(serializedCalendar);

                return new CalendarExportResult
                {
                    IsSuccess = true,
                    FileContent = bytes,
                    FileName = $"Transfer_{transferId}_{transfer.TransferDate:yyyyMMdd}.ics",
                    ContentType = "text/calendar; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer takvim event'i oluşturulurken hata: {ex.Message}");
                return new CalendarExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Takvim event'i oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<CalendarExportResult> GenerateCityTourCalendarAsync(int cityTourId)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .FirstOrDefaultAsync(ct => ct.Id == cityTourId && !ct.IsDeleted);

                if (cityTour == null)
                {
                    return new CalendarExportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Şehir turu bulunamadı."
                    };
                }

                var calendar = new IcalCalendar();
                var calendarEvent = CreateCityTourEvent(cityTour);
                calendar.Events.Add(calendarEvent);

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytes = Encoding.UTF8.GetBytes(serializedCalendar);

                return new CalendarExportResult
                {
                    IsSuccess = true,
                    FileContent = bytes,
                    FileName = $"CityTour_{cityTourId}_{cityTour.TourDate:yyyyMMdd}.ics",
                    ContentType = "text/calendar; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turu takvim event'i oluşturulurken hata: {ex.Message}");
                return new CalendarExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Takvim event'i oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<CalendarExportResult> GenerateYachtTourCalendarAsync(int yachtTourId)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .FirstOrDefaultAsync(yt => yt.Id == yachtTourId && !yt.IsDeleted);

                if (yachtTour == null)
                {
                    return new CalendarExportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Yat turu bulunamadı."
                    };
                }

                var calendar = new IcalCalendar();
                var calendarEvent = CreateYachtTourEvent(yachtTour);
                calendar.Events.Add(calendarEvent);

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytes = Encoding.UTF8.GetBytes(serializedCalendar);

                return new CalendarExportResult
                {
                    IsSuccess = true,
                    FileContent = bytes,
                    FileName = $"YachtTour_{yachtTourId}_{yachtTour.TourDate:yyyyMMdd}.ics",
                    ContentType = "text/calendar; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turu takvim event'i oluşturulurken hata: {ex.Message}");
                return new CalendarExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Takvim event'i oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<CalendarExportResult> GenerateReservationCalendarAsync(int reservationId)
        {
            try
            {
                var reservation = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

                if (reservation == null)
                {
                    return new CalendarExportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Rezervasyon bulunamadı."
                    };
                }

                var calendar = new IcalCalendar();
                var calendarEvent = CreateReservationEvent(reservation);
                calendar.Events.Add(calendarEvent);

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytes = Encoding.UTF8.GetBytes(serializedCalendar);

                return new CalendarExportResult
                {
                    IsSuccess = true,
                    FileContent = bytes,
                    FileName = $"Reservation_{reservationId}_{reservation.ReservationDate:yyyyMMdd}.ics",
                    ContentType = "text/calendar; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon takvim event'i oluşturulurken hata: {ex.Message}");
                return new CalendarExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Takvim event'i oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<CalendarExportResult> GenerateBulkTransferCalendarAsync(List<int> transferIds, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Airport)
                    .Where(t => !t.IsDeleted);

                if (transferIds != null && transferIds.Count > 0)
                {
                    query = query.Where(t => transferIds.Contains(t.Id));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.TransferDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.TransferDate <= endDate.Value);
                }

                var transfers = await query.OrderBy(t => t.TransferDate).ToListAsync();

                if (transfers.Count == 0)
                {
                    return new CalendarExportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Transfer bulunamadı."
                    };
                }

                var calendar = new IcalCalendar();

                foreach (var transfer in transfers)
                {
                    var calendarEvent = CreateTransferEvent(transfer);
                    calendar.Events.Add(calendarEvent);
                }

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytes = Encoding.UTF8.GetBytes(serializedCalendar);

                var fileName = startDate.HasValue && endDate.HasValue
                    ? $"Transfers_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.ics"
                    : $"Transfers_{DateTime.Now:yyyyMMdd}.ics";

                return new CalendarExportResult
                {
                    IsSuccess = true,
                    FileContent = bytes,
                    FileName = fileName,
                    ContentType = "text/calendar; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Toplu transfer takvim event'i oluşturulurken hata: {ex.Message}");
                return new CalendarExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Takvim event'i oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<CalendarExportResult> GenerateBulkTourCalendarAsync(List<int> cityTourIds, List<int> yachtTourIds, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var calendar = new IcalCalendar();
                int eventCount = 0;

                // City Tours
                if (cityTourIds != null && cityTourIds.Count > 0)
                {
                    var cityTours = await _cityTourRepository.GetAll()
                        .Include(ct => ct.OwnerGuest)
                        .Include(ct => ct.Personnel)
                        .Include(ct => ct.City)
                        .Where(ct => !ct.IsDeleted && cityTourIds.Contains(ct.Id))
                        .ToListAsync();

                    foreach (var cityTour in cityTours)
                    {
                        if (!startDate.HasValue || cityTour.TourDate >= startDate.Value)
                        {
                            if (!endDate.HasValue || cityTour.TourDate <= endDate.Value)
                            {
                                var calendarEvent = CreateCityTourEvent(cityTour);
                                calendar.Events.Add(calendarEvent);
                                eventCount++;
                            }
                        }
                    }
                }

                // Yacht Tours
                if (yachtTourIds != null && yachtTourIds.Count > 0)
                {
                    var yachtTours = await _yachtTourRepository.GetAll()
                        .Include(yt => yt.OwnerGuest)
                        .Include(yt => yt.Personnel)
                        .Include(yt => yt.City)
                        .Where(yt => !yt.IsDeleted && yachtTourIds.Contains(yt.Id))
                        .ToListAsync();

                    foreach (var yachtTour in yachtTours)
                    {
                        if (!startDate.HasValue || yachtTour.TourDate >= startDate.Value)
                        {
                            if (!endDate.HasValue || yachtTour.TourDate <= endDate.Value)
                            {
                                var calendarEvent = CreateYachtTourEvent(yachtTour);
                                calendar.Events.Add(calendarEvent);
                                eventCount++;
                            }
                        }
                    }
                }

                if (eventCount == 0)
                {
                    return new CalendarExportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Tur bulunamadı."
                    };
                }

                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytes = Encoding.UTF8.GetBytes(serializedCalendar);

                var fileName = startDate.HasValue && endDate.HasValue
                    ? $"Tours_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.ics"
                    : $"Tours_{DateTime.Now:yyyyMMdd}.ics";

                return new CalendarExportResult
                {
                    IsSuccess = true,
                    FileContent = bytes,
                    FileName = fileName,
                    ContentType = "text/calendar; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Toplu tur takvim event'i oluşturulurken hata: {ex.Message}");
                return new CalendarExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Takvim event'i oluşturulurken hata: {ex.Message}"
                };
            }
        }

        #region Private Methods

        private CalendarEvent CreateTransferEvent(TransferEntity transfer)
        {
            var calendarEvent = new CalendarEvent
            {
                Uid = $"transfer-{transfer.Id}@{_appName.ToLower()}.com",
                Summary = $"Transfer - {transfer.Guest?.FullName ?? "Misafir"}",
                Description = BuildTransferDescription(transfer),
                Start = new CalDateTime(transfer.TransferDate),
                End = new CalDateTime(transfer.TransferDate.AddHours(1)), // Varsayılan 1 saat
                Location = $"{transfer.PickupAddress} → {transfer.DropoffAddress}",
                IsAllDay = false
            };

            calendarEvent.Organizer = new Organizer($"MAILTO:{_appName.ToLower()}@guestflow.com");
            calendarEvent.Status = "CONFIRMED";

            return calendarEvent;
        }

        private CalendarEvent CreateCityTourEvent(CityTourEntity cityTour)
        {
            var calendarEvent = new CalendarEvent
            {
                Uid = $"citytour-{cityTour.Id}@{_appName.ToLower()}.com",
                Summary = $"Şehir Turu - {cityTour.City?.CityName ?? "Şehir"}",
                Description = BuildCityTourDescription(cityTour),
                Start = new CalDateTime(cityTour.TourDate),
                End = new CalDateTime(cityTour.TourDate.AddHours(cityTour.DurationHours > 0 ? cityTour.DurationHours : 4)), // Varsayılan 4 saat
                Location = cityTour.City?.CityName ?? "Belirtilmemiş",
                IsAllDay = false
            };

            calendarEvent.Organizer = new Organizer($"MAILTO:{_appName.ToLower()}@guestflow.com");
            calendarEvent.Status = "CONFIRMED";

            return calendarEvent;
        }

        private CalendarEvent CreateYachtTourEvent(YachtTourEntity yachtTour)
        {
            var calendarEvent = new CalendarEvent
            {
                Uid = $"yachttour-{yachtTour.Id}@{_appName.ToLower()}.com",
                Summary = $"Yat Turu - {yachtTour.YachtName ?? "Yat"}",
                Description = BuildYachtTourDescription(yachtTour),
                Start = new CalDateTime(yachtTour.TourDate),
                End = new CalDateTime(yachtTour.TourDate.AddHours(6)), // Varsayılan 6 saat
                Location = yachtTour.City?.CityName ?? "Belirtilmemiş",
                IsAllDay = false
            };

            calendarEvent.Organizer = new Organizer($"MAILTO:{_appName.ToLower()}@guestflow.com");
            calendarEvent.Status = "CONFIRMED";

            return calendarEvent;
        }

        private CalendarEvent CreateReservationEvent(ReservationEntity reservation)
        {
            var calendarEvent = new CalendarEvent
            {
                Uid = $"reservation-{reservation.Id}@{_appName.ToLower()}.com",
                Summary = $"Rezervasyon - {reservation.Guest?.FullName ?? "Misafir"}",
                Description = BuildReservationDescription(reservation),
                Start = new CalDateTime(reservation.ReservationDate),
                End = new CalDateTime(reservation.ReservationDate.AddHours(1)),
                Location = "Rezervasyon",
                IsAllDay = false
            };

            calendarEvent.Organizer = new Organizer($"MAILTO:{_appName.ToLower()}@guestflow.com");
            calendarEvent.Status = "CONFIRMED";

            return calendarEvent;
        }

        private string BuildTransferDescription(TransferEntity transfer)
        {
            var description = new StringBuilder();
            description.AppendLine($"Transfer Detayları:");
            description.AppendLine($"Misafir: {transfer.Guest?.FullName ?? "Bilinmiyor"}");
            description.AppendLine($"Kalkış: {transfer.PickupAddress}");
            description.AppendLine($"Varış: {transfer.DropoffAddress}");
            description.AppendLine($"Tarih: {transfer.TransferDate:dd.MM.yyyy HH:mm}");
            
            if (transfer.Personnel != null)
                description.AppendLine($"Personel: {transfer.Personnel.FullName}");
            
            if (transfer.Vehicle != null)
                description.AppendLine($"Araç: {transfer.Vehicle.PlateNumber} - {transfer.Vehicle.Type}");
            
            if (transfer.FinalPrice > 0)
                description.AppendLine($"Fiyat: {transfer.FinalPrice} {transfer.Currency}");
            
            if (!string.IsNullOrEmpty(transfer.Note))
                description.AppendLine($"Notlar: {transfer.Note}");

            return description.ToString();
        }

        private string BuildCityTourDescription(CityTourEntity cityTour)
        {
            var description = new StringBuilder();
            description.AppendLine($"Şehir Turu Detayları:");
            description.AppendLine($"Misafir: {cityTour.OwnerGuest?.FullName ?? "Bilinmiyor"}");
            description.AppendLine($"Şehir: {cityTour.City?.CityName ?? "Belirtilmemiş"}");
            description.AppendLine($"Tarih: {cityTour.TourDate:dd.MM.yyyy HH:mm}");
            description.AppendLine($"Dil: {cityTour.Language ?? "Belirtilmemiş"}");
            
            if (cityTour.Personnel != null)
                description.AppendLine($"Personel: {cityTour.Personnel.FullName}");
            
            // NumberOfPeople property'si CityTourEntity'de yok, bu yüzden kaldırıyoruz
            
            if (cityTour.FinalPrice > 0)
                description.AppendLine($"Fiyat: {cityTour.FinalPrice} {cityTour.Currency}");

            return description.ToString();
        }

        private string BuildYachtTourDescription(YachtTourEntity yachtTour)
        {
            var description = new StringBuilder();
            description.AppendLine($"Yat Turu Detayları:");
            description.AppendLine($"Misafir: {yachtTour.OwnerGuest?.FullName ?? "Bilinmiyor"}");
            description.AppendLine($"Yat Adı: {yachtTour.YachtName ?? "Belirtilmemiş"}");
            description.AppendLine($"Şehir: {yachtTour.City?.CityName ?? "Belirtilmemiş"}");
            description.AppendLine($"Tarih: {yachtTour.TourDate:dd.MM.yyyy HH:mm}");
            
            if (yachtTour.Personnel != null)
                description.AppendLine($"Personel: {yachtTour.Personnel.FullName}");
            
            if (yachtTour.NumberOfPeople > 0)
                description.AppendLine($"Kişi Sayısı: {yachtTour.NumberOfPeople}");
            
            if (yachtTour.FinalPrice > 0)
                description.AppendLine($"Fiyat: {yachtTour.FinalPrice} {yachtTour.Currency}");
            
            if (!string.IsNullOrEmpty(yachtTour.SpecialRequest))
                description.AppendLine($"Özel İstekler: {yachtTour.SpecialRequest}");

            return description.ToString();
        }

        private string BuildReservationDescription(ReservationEntity reservation)
        {
            var description = new StringBuilder();
            description.AppendLine($"Rezervasyon Detayları:");
            description.AppendLine($"Misafir: {reservation.Guest?.FullName ?? "Bilinmiyor"}");
            description.AppendLine($"Servis Tipi: {reservation.ServiceType}");
            description.AppendLine($"Tarih: {reservation.ReservationDate:dd.MM.yyyy HH:mm}");
            description.AppendLine($"Durum: {reservation.Status}");
            
            if (reservation.Personnel != null)
                description.AppendLine($"Personel: {reservation.Personnel.FullName}");
            
            if (!string.IsNullOrEmpty(reservation.Notes))
                description.AppendLine($"Notlar: {reservation.Notes}");

            return description.ToString();
        }

        #endregion
    }
}

