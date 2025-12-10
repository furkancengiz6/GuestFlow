using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Guest
{
    public class GuestManager : IGuestService
    {
        // Bu üç değişkeni sınıfın içinde kullanıyoruz.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyoruz örneğin, transaction başlatma, kaydetme.
        // _guestRepository: Misafirlerle ilgili veritabanı işlemlerini yapmak için kullanıyoruz.
        // _logger: Hataları veya bilgileri loglamak kaydetmek için kullanıyoruz.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly ILogger<GuestManager> _logger;
        private readonly IMapper _mapper;

        // Constructor Bu sınıf oluşturulurken dependency buradan alıyoruz.
        public GuestManager(
            IUnitOfWork unitOfWork,
            IRepository<GuestEntity> guestRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            ILogger<GuestManager> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _guestRepository = guestRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        // Bu metod, yeni bir misafir ekliyor.
        public async Task<ServiceMessage> AddGuest(AddGuestDto guest)
        {
            try
            {
                // Veritabanında transaction başlatıyoruz. Eğer bir hata olursa, tüm değişiklikleri geri alacağız.
                await _unitOfWork.BeginTransactionAsync();

                // Eğer misafir özel bir misafir değilse IsSpecialGuest false ise, bazı bilgileri kontrol etmemiz lazım.
                if (!guest.IsSpecialGuest)
                {
                    // Email boş mu ya da sadece boşluklardan mı oluşuyor? Eğer öyleyse hata döndürüyoruz.
                    if (string.IsNullOrWhiteSpace(guest.Email))
                        return new ServiceMessage { IsSuccess = false, Message = "Email alanı zorunludur." };

                    // Email'de "@" işareti var mı? Yoksa geçerli bir email değil demektir.
                    if (!guest.Email.Contains("@"))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçerli bir e-posta adresi giriniz (örneğin, user@example.com)." };

                    // Email'in uzunluğu 5'ten küçük veya 100'den büyük mü? Öyleyse hata veriyoruz.
                    if (guest.Email.Length < 5 || guest.Email.Length > 100)
                        return new ServiceMessage { IsSuccess = false, Message = "Email 5 ila 100 karakter arasında olmalıdır." };

                    // Telefon numarası boş mu ya da sadece boşluklardan mı oluşuyor? Eğer öyleyse hata döndürüyoruz.
                    if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber alanı zorunludur." };

                    // Telefon numarası sadece rakamlar ve "+" işaretinden mi oluşuyor? Değilse hata veriyoruz.
                    if (!guest.PhoneNumber.All(c => char.IsDigit(c) || c == '+'))
                        return new ServiceMessage { IsSuccess = false, Message = "Telefon numarası sadece rakamlardan oluşmalıdır (örneğin, +905551234567)." };

                    // Telefon numarasının uzunluğu 5'ten küçük veya 20'den büyük mü? Öyleyse hata veriyoruz.
                    if (guest.PhoneNumber.Length < 5 || guest.PhoneNumber.Length > 20)
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber 5 ila 20 karakter arasında olmalıdır." };
                }

                // Eğer misafir özel bir misafirse IsSpecialGuest true ise, email ve telefon numarası boşsa varsayılan değerler atıyoruz.
                if (guest.IsSpecialGuest)
                {
                    guest.Email ??= "special@guestflow.com"; // Email boşsa, bu varsayılan değeri atıyoruz.
                    guest.PhoneNumber ??= "+9000000000"; // Telefon numarası boşsa, bu varsayılan değeri atıyoruz.
                }

                // Misafire özel bir kod (GuestCode) oluşturuyoruz. Bu kod, her misafir için benzersiz olmalı.
                string guestCode = await GenerateGuestCodeAsync();

                // Bu GuestCode ile başka bir misafir var mı diye kontrol ediyoruz.IsDeleted = false ,Sadece aktif misafirleri kontrol ett.
                var hasGuest = await _guestRepository.GetAll(x => x.GuestCode == guestCode && !x.IsDeleted).AnyAsync();
                if (hasGuest)
                return new ServiceMessage { IsSuccess = false, Message = "Bu GuestCode ile bir misafir zaten mevcut." };

                // Yeni bir misafir nesnesi oluşturuyoruz ve DTO'dan gelen bilgileri buraya aktarıyoruz.
                var newGuest = new GuestEntity
                {
                    FullName = guest.FullName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Nationality = guest.Nationality,
                    GuestCode = guestCode,
                    IsSpecialGuest = guest.IsSpecialGuest
                };

                // Yeni misafiri veritabanına ekliyoruz.
                await _guestRepository.AddAsync(newGuest);
                // Değişiklikleri veritabanına kaydediyoruz.
                await _unitOfWork.SaveChangesAsync();
                // İşlem başarılıysa transaction'ı tamamlıyoruz 
                await _unitOfWork.CommitTransactionAsync();

                // Başarılı bir şekilde misafir eklendiğini logluyoru
                _logger.LogInformation($"Misafir eklendi: {guest.FullName}");
                // Başarı mesajı döndürüyoruz.
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, transaction'ı geri alıyoruz (rollback yapıyoruz).
                await _unitOfWork.RollbackTransactionAsync();
                // Hatayı logluyoruz. InnerException varsa onu da ekliyoruz ki daha fazla bilgi alabilelim.
                _logger.LogError(ex, $"Misafir eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                // Hata mesajını oluşturuyoruz. InnerException varsa onu da ekliyoruz.
                string errorMessage = $"Misafir eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                // Hata mesajıyla birlikte başarısız bir sonuç döndürüyoruz.
                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metod, mevcut bir misafiri güncelliyor.
        public async Task<ServiceMessage> UpdateGuest(UpdateGuestDto guest)
        {
            try
            {
                // Veritabanında bir işlem  başlatıyoruz.
                await _unitOfWork.BeginTransactionAsync();

                // Güncellenecek misafiri IDsine göre veritabanından çekiyoruz.
                var existing = await _guestRepository.GetAsync(x => x.Id == guest.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                // Eğer misafir özel bir misafir değilse, bilgileri kontrol ediyoruz 
                if (!guest.IsSpecialGuest)
                {
                    if (string.IsNullOrWhiteSpace(guest.Email))
                        return new ServiceMessage { IsSuccess = false, Message = "Email alanı zorunludur." };

                    if (!guest.Email.Contains("@"))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçerli bir e-posta adresi giriniz (örneğin, user@example.com)." };

                    if (guest.Email.Length < 5 || guest.Email.Length > 100)
                        return new ServiceMessage { IsSuccess = false, Message = "Email 5 ila 100 karakter arasında olmalıdır." };

                    if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber alanı zorunludur." };

                    if (!guest.PhoneNumber.All(c => char.IsDigit(c) || c == '+'))
                        return new ServiceMessage { IsSuccess = false, Message = "Telefon numarası sadece rakamlardan oluşmalıdır (örneğin, +905551234567)." };

                    if (guest.PhoneNumber.Length < 5 || guest.PhoneNumber.Length > 20)
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber 5 ila 20 karakter arasında olmalıdır." };
                }

                // Misafirin bilgilerini güncelliyoruz. Özel misafir için varsayılan değerler atıyoruz.
                existing.FullName = guest.FullName;
                existing.Email = guest.Email ?? "special@guestflow.com";
                existing.PhoneNumber = guest.PhoneNumber ?? "+9000000000";
                existing.Nationality = guest.Nationality;
                existing.IsSpecialGuest = guest.IsSpecialGuest;

                // Güncellenmiş misafiri veritabanına kaydediyoruz.
                await _guestRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Başarılı bir şekilde güncellendiğini logluyoruz.
                _logger.LogInformation($"Misafir güncellendi: {guest.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Misafir güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Misafir güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metod, bir misafiri silmek için kullanılıyor.
        public async Task<ServiceMessage> DeleteGuest(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Veritabanından misafiri silmek için ID'sini kullanıyoruz.
                await _guestRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Misafir silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Misafir silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Misafir silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metod, belirli bir misafiri ID'sine göre getiriyor.
        public async Task<GetGuestDto> GetGuestById(int id)
        {
            try
            {
                // Veritabanından misafiri ID sine göre çektik
                var guest = await _guestRepository.GetByIdAsync(id);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                // Misafiri bir DTO nesnesine çevirip geri döndürdik
                return _mapper.Map<GetGuestDto>(guest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir getirilirken hata: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metod, tüm misafirleri getiriyor.
        public async Task<List<GetGuestDto>> GetGuests()
        {
            try
            {
                // Veritabanından tüm misafirleri çekiyoruz ve her birini GetGuestDto ya çeviriyoruz.
                var guests = await _guestRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetGuestDto>>(guests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafirler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metod, sayfalanmış misafirleri getiriyor.
        public async Task<PagedResult<GetGuestDto>> GetGuestsPaged(int pageNumber, int pageSize)
        {
            return await GetGuestsPaged(pageNumber, pageSize, null);
        }

        // Bu metod, filtreleme ve sıralama ile sayfalanmış misafirleri getiriyor.
        public async Task<PagedResult<GetGuestDto>> GetGuestsPaged(int pageNumber, int pageSize, GuestFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _guestRepository.GetAll()
                    .ApplyGuestFilters(filters)
                    .ApplyGuestSorting(sorting);

                var totalCount = await query.CountAsync();
                var guests = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetGuestDto>>(guests);
                return new PagedResult<GetGuestDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış misafirler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metod, her misafir için benzersiz bir GuestCode oluşturuyor.
        private async Task<string> GenerateGuestCodeAsync()
        {
            const string prefix = "GUEST-";
            int maxNumber = 0;
            // Veritabanındaki tüm GuestCode'ları al ve en büyük sayıyı bul
            // Sadece aktif kayıtları IsDeleted = false kontrolü
            var existingCodes = await _guestRepository.GetAll(g => !g.IsDeleted)
            .Select(g => g.GuestCode)
            .Where(code => code.StartsWith(prefix))
            .ToListAsync();

            if (existingCodes.Any())
            {// GuestCode'lardan sayısal kısmı çıkar ve en büyüğünü bul
                maxNumber = existingCodes
                    .Select(code => int.Parse(code.Replace(prefix, "")))
                    .Max();
            }

            return $"{prefix}{(maxNumber + 1):D3}";
        }

        public async Task<GuestDetailDto> GetGuestDetailAsync(int id)
        {
            try
            {
                var guest = await _guestRepository.GetAll()
                    .Include(g => g.Transfers)
                    .Include(g => g.CityTours)
                        .ThenInclude(ct => ct.City)
                    .Include(g => g.YachtTours)
                        .ThenInclude(yt => yt.City)
                    .Include(g => g.Invoices)
                    .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                // İstatistikleri hesapla
                var statistics = new GuestStatisticsDto
                {
                    TotalTransfers = guest.Transfers.Count,
                    TotalCityTours = guest.CityTours.Count,
                    TotalYachtTours = guest.YachtTours.Count,
                    TotalBookings = guest.Transfers.Count + guest.CityTours.Count + guest.YachtTours.Count,
                    TotalInvoices = guest.Invoices.Count,
                    TotalSpent = guest.Transfers.Sum(t => (decimal?)t.FinalPrice) ?? 0 +
                                 guest.CityTours.Sum(ct => (decimal?)ct.FinalPrice) ?? 0 +
                                 guest.YachtTours.Sum(yt => (decimal?)yt.FinalPrice) ?? 0,
                    FirstBookingDate = guest.Transfers.Select(t => t.TransferDate)
                        .Concat(guest.CityTours.Select(ct => ct.TourDate))
                        .Concat(guest.YachtTours.Select(yt => yt.TourDate))
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Min(),
                    LastBookingDate = guest.Transfers.Select(t => t.TransferDate)
                        .Concat(guest.CityTours.Select(ct => ct.TourDate))
                        .Concat(guest.YachtTours.Select(yt => yt.TourDate))
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max()
                };

                statistics.AverageBookingValue = statistics.TotalBookings > 0
                    ? statistics.TotalSpent / statistics.TotalBookings
                    : 0;

                // Transferler
                var transfers = guest.Transfers
                    .OrderByDescending(t => t.TransferDate)
                    .Select(t => new GuestTransferDto
                    {
                        Id = t.Id,
                        TransferDate = t.TransferDate,
                        PickupAddress = t.PickupAddress,
                        DropoffAddress = t.DropoffAddress,
                        Price = t.Price,
                        FinalPrice = t.FinalPrice,
                        Status = t.Status.ToString(),
                        IsFromAirport = t.IsFromAirport,
                        Note = t.Note,
                        CreatedDate = t.CreatedDate
                    })
                    .ToList();

                // Şehir Turları
                var cityTours = guest.CityTours
                    .OrderByDescending(ct => ct.TourDate)
                    .Select(ct => new GuestCityTourDto
                    {
                        Id = ct.Id,
                        TourDate = ct.TourDate,
                        Language = ct.Language,
                        DurationHours = ct.DurationHours,
                        Price = ct.Price,
                        FinalPrice = ct.FinalPrice,
                        CityName = ct.City != null ? ct.City.CityName : null,
                        CreatedDate = ct.CreatedDate
                    })
                    .ToList();

                // Yat Turları
                var yachtTours = guest.YachtTours
                    .OrderByDescending(yt => yt.TourDate)
                    .Select(yt => new GuestYachtTourDto
                    {
                        Id = yt.Id,
                        TourDate = yt.TourDate,
                        NumberOfPeople = yt.NumberOfPeople,
                        Price = yt.Price,
                        FinalPrice = yt.FinalPrice,
                        YachtName = yt.YachtName,
                        CityName = yt.City != null ? yt.City.CityName : null,
                        SpecialRequest = yt.SpecialRequest,
                        CreatedDate = yt.CreatedDate
                    })
                    .ToList();

                // Faturalar
                var invoices = guest.Invoices
                    .OrderByDescending(i => i.IssueDate)
                    .Select(i => new GuestInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        IssueDate = i.IssueDate,
                        TotalAmount = i.TotalAmount,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl ?? string.Empty,
                        CreatedDate = i.CreatedDate
                    })
                    .ToList();

                // Zaman çizelgesi oluştur
                var timeline = new List<GuestTimelineItemDto>();

                // Transferler
                foreach (var transfer in guest.Transfers)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = transfer.Id,
                        Type = "Transfer",
                        Title = $"Transfer - {transfer.PickupAddress} → {transfer.DropoffAddress}",
                        Description = transfer.Note ?? "Transfer rezervasyonu",
                        Date = transfer.TransferDate,
                        Amount = transfer.FinalPrice,
                        Status = transfer.Status.ToString(),
                        CreatedDate = transfer.CreatedDate
                    });
                }

                // Şehir Turları
                foreach (var cityTour in guest.CityTours)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = cityTour.Id,
                        Type = "CityTour",
                        Title = $"Şehir Turu - {cityTour.City?.CityName ?? "Bilinmiyor"}",
                        Description = $"{cityTour.DurationHours} saatlik tur - {cityTour.Language}",
                        Date = cityTour.TourDate,
                        Amount = cityTour.FinalPrice,
                        Status = "Aktif",
                        CreatedDate = cityTour.CreatedDate
                    });
                }

                // Yat Turları
                foreach (var yachtTour in guest.YachtTours)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = yachtTour.Id,
                        Type = "YachtTour",
                        Title = $"Yat Turu - {yachtTour.YachtName}",
                        Description = $"{yachtTour.NumberOfPeople} kişi - {yachtTour.City?.CityName ?? "Bilinmiyor"}",
                        Date = yachtTour.TourDate,
                        Amount = yachtTour.FinalPrice,
                        Status = "Aktif",
                        CreatedDate = yachtTour.CreatedDate
                    });
                }

                // Faturalar
                foreach (var invoice in guest.Invoices)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = invoice.Id,
                        Type = "Invoice",
                        Title = $"Fatura #{invoice.InvoiceNumber}",
                        Description = $"{invoice.TotalAmount} {invoice.Currency}",
                        Date = invoice.IssueDate,
                        Amount = invoice.TotalAmount,
                        Status = invoice.PdfUrl != null ? "PDF Oluşturuldu" : "Bekliyor",
                        CreatedDate = invoice.CreatedDate
                    });
                }

                // Zaman çizelgesini tarihe göre sırala
                timeline = timeline.OrderByDescending(t => t.Date).ToList();

                return new GuestDetailDto
                {
                    Id = guest.Id,
                    FullName = guest.FullName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Nationality = guest.Nationality,
                    GuestCode = guest.GuestCode,
                    IsSpecialGuest = guest.IsSpecialGuest,
                    CreatedDate = guest.CreatedDate,
                    Statistics = statistics,
                    Transfers = transfers,
                    CityTours = cityTours,
                    YachtTours = yachtTours,
                    Invoices = invoices,
                    Timeline = timeline
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir detayı getirilirken hata: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<List<GuestInvoiceDto>> GetGuestInvoicesAsync(int guestId)
        {
            try
            {
                var guest = await _guestRepository.GetByIdAsync(guestId);
                if (guest == null || guest.IsDeleted)
                    throw new Exception("Misafir bulunamadı.");

                var invoices = await _invoiceRepository.GetAll()
                    .Where(i => i.GuestId == guestId && !i.IsDeleted)
                    .OrderByDescending(i => i.IssueDate)
                    .Select(i => new GuestInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        IssueDate = i.IssueDate,
                        TotalAmount = i.TotalAmount,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl ?? string.Empty,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir faturaları getirilirken hata: {ex.Message}. GuestId: {guestId}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<List<GuestTimelineItemDto>> GetGuestTimelineAsync(int guestId)
        {
            try
            {
                var guest = await _guestRepository.GetAll()
                    .Include(g => g.Transfers)
                    .Include(g => g.CityTours)
                        .ThenInclude(ct => ct.City)
                    .Include(g => g.YachtTours)
                        .ThenInclude(yt => yt.City)
                    .Include(g => g.Invoices)
                    .FirstOrDefaultAsync(g => g.Id == guestId && !g.IsDeleted);

                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                var timeline = new List<GuestTimelineItemDto>();

                // Transferler
                foreach (var transfer in guest.Transfers)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = transfer.Id,
                        Type = "Transfer",
                        Title = $"Transfer - {transfer.PickupAddress} → {transfer.DropoffAddress}",
                        Description = transfer.Note ?? "Transfer rezervasyonu",
                        Date = transfer.TransferDate,
                        Amount = transfer.FinalPrice,
                        Status = transfer.Status.ToString(),
                        CreatedDate = transfer.CreatedDate
                    });
                }

                // Şehir Turları
                foreach (var cityTour in guest.CityTours)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = cityTour.Id,
                        Type = "CityTour",
                        Title = $"Şehir Turu - {cityTour.City?.CityName ?? "Bilinmiyor"}",
                        Description = $"{cityTour.DurationHours} saatlik tur - {cityTour.Language}",
                        Date = cityTour.TourDate,
                        Amount = cityTour.FinalPrice,
                        Status = "Aktif",
                        CreatedDate = cityTour.CreatedDate
                    });
                }

                // Yat Turları
                foreach (var yachtTour in guest.YachtTours)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = yachtTour.Id,
                        Type = "YachtTour",
                        Title = $"Yat Turu - {yachtTour.YachtName}",
                        Description = $"{yachtTour.NumberOfPeople} kişi - {yachtTour.City?.CityName ?? "Bilinmiyor"}",
                        Date = yachtTour.TourDate,
                        Amount = yachtTour.FinalPrice,
                        Status = "Aktif",
                        CreatedDate = yachtTour.CreatedDate
                    });
                }

                // Faturalar
                foreach (var invoice in guest.Invoices)
                {
                    timeline.Add(new GuestTimelineItemDto
                    {
                        Id = invoice.Id,
                        Type = "Invoice",
                        Title = $"Fatura #{invoice.InvoiceNumber}",
                        Description = $"{invoice.TotalAmount} {invoice.Currency}",
                        Date = invoice.IssueDate,
                        Amount = invoice.TotalAmount,
                        Status = invoice.PdfUrl != null ? "PDF Oluşturuldu" : "Bekliyor",
                        CreatedDate = invoice.CreatedDate
                    });
                }

                // Zaman çizelgesini tarihe göre sırala
                return timeline.OrderByDescending(t => t.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir zaman çizelgesi getirilirken hata: {ex.Message}. GuestId: {guestId}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}
