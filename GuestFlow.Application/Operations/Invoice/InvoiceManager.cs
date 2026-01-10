using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public class InvoiceManager : IInvoiceService
    {
        // Bu değişkenleri sınıfın içinde kullanıyoruz.
        // _invoiceRepository: Faturalarla ilgili veritabanı işlemlerini yapmak için kullanıyoruz.
        // _guestRepository: Misafir bilgilerini almak için kullanıyoruz.
        // _personnelRepository: Personel bilgilerini almak için kullanıyoruz.
        // _pdfService: PDF oluşturmak için kullanıyoruz.
        // _emailService: E-posta göndermek için kullanıyoruz.
        // _configuration: Konfigürasyon ayarlarına erişmek için kullanıyoruz.
        // _logger: Hataları veya bilgileri loglamak (kaydetmek) için kullanıyoruz.
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<InvoiceItemEntity> _invoiceItemRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceManager> _logger;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly IMapper _mapper;
        private readonly IPaymentStatusService _paymentStatusService;

        // Constructor (yapıcı metod): Bu sınıf oluşturulurken bağımlılıkları (dependency) buradan alıyoruz.
        public InvoiceManager(
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<InvoiceItemEntity> invoiceItemRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IUnitOfWork unitOfWork,
            IPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<InvoiceManager> logger,
            IPdfUrlService pdfUrlService,
            IMapper mapper,
            IPaymentStatusService paymentStatusService)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _pdfUrlService = pdfUrlService;
            _mapper = mapper;
            _paymentStatusService = paymentStatusService;
        }

        // Bu metod, belirli bir faturayı ID'sine göre getiriyor.
        public async Task<GetInvoiceDto> GetInvoiceDtoById(int id)
        {
            try
            {
                // Veritabanından faturayı ID'sine göre çekiyoruz ve InvoiceItems'ı da yüklüyoruz.
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

                // Eğer fatura bulunamazsa, bir hata fırlatıyoruz (exception throw ediyoruz).
                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                // Fatura bulunduysa, onu bir DTO (Data Transfer Object) nesnesine çevirip geri döndürüyoruz.
                var dto = _mapper.Map<GetInvoiceDto>(invoice);

                // InvoiceItems'ı da map edelim
                if (invoice.InvoiceItems != null)
                {
                    dto.InvoiceItems = _mapper.Map<List<InvoiceItemDto>>(invoice.InvoiceItems);
                }

                return dto;
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, bunu logluyoruz (kaydediyoruz).
                // LogError metodu, hatayı ve detaylarını kaydeder. Burada fatura ID'sini de ekledik ki hangi faturada sorun olduğunu bilelim.
                _logger.LogError(ex, $"Fatura getirilirken hata: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");

                // Hata olduğu için bu hatayı yukarıya fırlatıyoruz (throw). Böylece bu metodu çağıran yer hatayı yakalayabilir.
                throw;
            }
        }

        // Bu metod, tüm faturaları getiriyor.
        public async Task<List<GetInvoiceDto>> GetInvoices()
        {
            try
            {
                // Veritabanından tüm faturaları çekiyoruz.
                // GetAll metodu tüm faturaları alır, AutoMapper ile her bir faturayı GetInvoiceDto'ya çeviriyoruz.
                // ToListAsync ile bu verileri bir liste haline getirip döndürüyoruz.
                var invoices = await _invoiceRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetInvoiceDto>>(invoices);
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, bunu logluyoruz.
                // Burada tüm faturaları çekerken bir sorun olduysa, hatayı ve varsa iç hatayı (InnerException) kaydediyoruz.
                _logger.LogError(ex, $"Faturalar listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                // Hata olduğu için bu hatayı yukarıya fırlatıyoruz (throw).
                throw;
            }
        }

        // Bu metod, belirli bir misafire ait faturaları getiriyor.
        public async Task<List<GetInvoiceDto>> GetInvoicesByGuestId(int guestId)
        {
            try
            {
                // Veritabanından sadece belirli bir misafire (guestId) ait faturaları çekiyoruz.
                // GetAll metodu ile filtreleme yapıyoruz (x => x.GuestId == guestId), sonra AutoMapper ile her bir faturayı GetInvoiceDto'ya çeviriyoruz.
                // ToListAsync ile bu verileri bir liste haline getirip döndürüyoruz.
                var invoices = await _invoiceRepository.GetAll(x => x.GuestId == guestId).ToListAsync();
                return _mapper.Map<List<GetInvoiceDto>>(invoices);
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, bunu logluyoruz.
                // Burada misafire ait faturaları çekerken bir sorun olduysa, hatayı, misafir ID'sini ve varsa iç hatayı (InnerException) kaydediyoruz.
                _logger.LogError(ex, $"Misafire ait faturalar listelenirken hata: {ex.Message}. GuestId: {guestId}. InnerException: {ex.InnerException?.Message}");

                // Hata olduğu için bu hatayı yukarıya fırlatıyoruz (throw).
                throw;
            }
        }

        /// <summary>
        /// Fatura için PDF oluşturur ve PdfUrl'i günceller
        /// </summary>
        /// <summary>
        /// Validates that an invoice can be modified (only Draft invoices, not PDF generated)
        /// INVOICE REALITY: Once PDF is generated, invoice becomes IMMUTABLE
        /// </summary>
        /// <summary>
        /// Get current personnel ID from async context (if available)
        /// This is a placeholder - in a real implementation, this would come from
        /// the current user's context (HttpContext, ClaimsPrincipal, etc.)
        /// </summary>
        private int? GetCurrentPersonnelId()
        {
            // TODO: Implement proper personnel context retrieval
            // For now, return null - the invoice will still be locked
            return null;
        }

        /// <summary>
        /// Validates that an invoice can be modified (only Draft invoices, not PDF generated)
        /// INVOICE REALITY: Once PDF is generated, invoice becomes IMMUTABLE
        /// </summary>
        private void ValidateInvoiceCanBeModified(InvoicesEntity invoice)
        {
            if (!invoice.CanBeModified)
            {
                var reason = invoice.IsPdfGenerated
                    ? "PDF oluşturulmuş faturalar değiştirilemez."
                    : $"Durumu '{invoice.Status}' olan faturalar değiştirilemez. Sadece 'Draft' durumundaki faturalar değiştirilebilir.";

                throw new InvalidOperationException($"Fatura değiştirilemez: {reason} Fatura ID: {invoice.Id}, Durum: {invoice.Status}, PDF Oluşturulmuş: {invoice.IsPdfGenerated}");
            }
        }

        public async Task<string> GeneratePdfForInvoiceAsync(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .Include(i => i.Personnel)
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                // INVOICE IMMUTABILITY: Prevent PDF generation on already generated invoices
                if (invoice.IsPdfGenerated)
                {
                    throw new InvalidOperationException($"Fatura için PDF zaten oluşturulmuş. Fatura ID: {invoiceId}");
                }

                var guest = invoice.Guest ?? await _guestRepository.GetByIdAsync(invoice.GuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                PersonnelEntity? personnel = null;
                if (invoice.PersonnelId.HasValue)
                {
                    personnel = invoice.Personnel ?? await _personnelRepository.GetByIdAsync(invoice.PersonnelId.Value);
                }

                var pdfUrl = await _pdfService.GenerateInvoicePdfAsync(invoice, guest, personnel);

                // INVOICE IMMUTABILITY: Lock the invoice after PDF generation (makes it IMMUTABLE)
                // This sets IsPdfGenerated=true, Status=Generated, PdfGeneratedDate, and LockedByPersonnelId
                var currentPersonnelId = GetCurrentPersonnelId(); // Get from context if available
                invoice.LockAfterPdfGeneration(pdfUrl, currentPersonnelId);
                await _invoiceRepository.UpdateAsync(invoice);

                _logger.LogInformation($"Fatura PDF'i oluşturuldu ve güncellendi. InvoiceId: {invoiceId}, PdfUrl: {pdfUrl}");

                // E-posta gönder (misafir e-postası varsa)
                if (!string.IsNullOrEmpty(guest.Email) && !guest.IsSpecialGuest)
                {
                    try
                    {
                        var fullPdfPath = _pdfUrlService.GetFullFilePathFromUrl(pdfUrl);

                        await _emailService.SendInvoiceEmailAsync(
                            guest.Email,
                            guest.FullName,
                            invoice.InvoiceNumber,
                            fullPdfPath
                        );

                        _logger.LogInformation($"Fatura e-postası gönderildi. InvoiceId: {invoiceId}, Email: {guest.Email}");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Fatura e-postası gönderilirken hata: {emailEx.Message}. InvoiceId: {invoiceId}");
                        // E-posta hatası fatura oluşturmayı engellemez
                    }
                }

                return pdfUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura PDF'i oluşturulurken hata: {ex.Message}. InvoiceId: {invoiceId}");
                throw;
            }
        }

        public async Task<InvoiceDetailDto> GetInvoiceDetailAsync(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .Include(i => i.Personnel)
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                var detail = _mapper.Map<InvoiceDetailDto>(invoice);

                // Accounting: is there any posted journal line referencing this invoice?
                detail.IsJournalPosted = await _unitOfWork.JournalLines
                    .GetAll(jl => jl.ReferenceId == id)
                    .AnyAsync();

                // Calculate payment status using PaymentStatusService
                var paymentStatus = await _paymentStatusService.GetInvoicePaymentStatusAsync(id);
                if (paymentStatus != null)
                {
                    detail.PaymentStatus = paymentStatus.PaymentStatus;
                    detail.PaidAmount = paymentStatus.PaidAmount;
                    detail.RemainingAmount = paymentStatus.RemainingAmount;
                    detail.PaidAmountByCurrency = paymentStatus.PaidAmountByCurrency;
                    detail.RemainingAmountByCurrency = paymentStatus.RemainingAmountByCurrency;
                }

                // For multi-service invoices, create a summary service info
                if (invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
                {
                    var itemCount = invoice.InvoiceItems.Count;
                    var serviceTypes = string.Join(", ", invoice.InvoiceItems.Select(i => i.ServiceType).Distinct());
                    var dateRange = invoice.InvoiceItems.Any() ?
                        $"{invoice.InvoiceItems.Min(i => i.CreatedDate):dd.MM.yyyy} - {invoice.InvoiceItems.Max(i => i.CreatedDate):dd.MM.yyyy}" :
                        "N/A";

                    detail.Service = new InvoiceServiceDto
                    {
                        ServiceType = "Multi-Service",
                        ServiceId = 0, // Not applicable for multi-service
                        ServiceName = $"{itemCount} hizmet ({serviceTypes})",
                        ServiceDate = invoice.InvoiceItems.First().CreatedDate,
                        ServiceAmount = invoice.TotalAmount,
                        AdditionalInfo = $"Tarih aralığı: {dateRange}"
                    };
                }

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura detayı getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<PagedResult<GetInvoiceDto>> GetInvoicesPagedAsync(int pageNumber, int pageSize, InvoiceFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .ApplyInvoiceFilters(filters)
                    .ApplyInvoiceSorting(sorting);

                var totalCount = await query.CountAsync();
                var invoices = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetInvoiceDto>>(invoices);
                return new PagedResult<GetInvoiceDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış faturalar listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today.AddMonths(-1);
                var end = endDate?.Date ?? today;

                var invoices = await _invoiceRepository.GetAll()
                    .Where(i => i.IssueDate.Date >= start && i.IssueDate.Date <= end && !i.IsDeleted)
                    .ToListAsync();

                var statistics = new InvoiceStatisticsDto
                {
                    TotalInvoices = invoices.Count,
                    InvoicesWithPdf = invoices.Count(i => !string.IsNullOrEmpty(i.PdfUrl)),
                    InvoicesWithoutPdf = invoices.Count(i => string.IsNullOrEmpty(i.PdfUrl)),
                    TotalRevenue = invoices.Sum(i => i.TotalAmount),
                    AverageInvoiceAmount = invoices.Count > 0 ? invoices.Average(i => i.TotalAmount) : 0,
                    TotalGuests = invoices.Select(i => i.GuestId).Distinct().Count()
                };

                // Para birimine göre grupla
                var invoicesByCurrency = invoices
                    .GroupBy(i => i.Currency)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueByCurrency = invoices
                    .GroupBy(i => i.Currency)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.TotalAmount));

                statistics.InvoicesByCurrency = invoicesByCurrency;
                statistics.RevenueByCurrency = revenueByCurrency;

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<ServiceMessage> SendInvoiceByEmailAsync(int invoiceId, string? recipientEmail = null)
        {
            try
            {
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

                if (invoice == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Fatura bulunamadı." };

                // PDF yoksa oluştur
                if (string.IsNullOrEmpty(invoice.PdfUrl))
                {
                    try
                    {
                        var pdfUrl = await GeneratePdfForInvoiceAsync(invoiceId);
                        invoice = await _invoiceRepository.GetAll()
                            .Include(i => i.Guest)
                            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogError(pdfEx, $"Fatura PDF'i oluşturulurken hata: {pdfEx.Message}");
                        return new ServiceMessage { IsSuccess = false, Message = $"PDF oluşturulamadı: {pdfEx.Message}" };
                    }
                }

                // E-posta adresini belirle
                var email = recipientEmail ?? invoice.Guest?.Email;
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceMessage { IsSuccess = false, Message = "E-posta adresi bulunamadı." };

                // PDF dosya yolunu al
                var pdfPath = invoice.PdfUrl;
                if (string.IsNullOrEmpty(pdfPath))
                    return new ServiceMessage { IsSuccess = false, Message = "PDF dosyası bulunamadı." };

                // PDF dosyasının fiziksel yolunu al (IPdfUrlService kullanarak)
                var pdfPhysicalPath = _pdfUrlService.GetFullFilePathFromUrl(pdfPath);
                if (string.IsNullOrEmpty(pdfPhysicalPath) || !System.IO.File.Exists(pdfPhysicalPath))
                {
                    // Fallback: Eski yöntem (geriye dönük uyumluluk için)
                    var fileName = _pdfUrlService.GetFileNameFromUrl(pdfPath);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = Path.GetFileName(pdfPath);
                    }
                    
                    // OutputPath kullan (e-posta için farklı bir path olabilir)
                    pdfPhysicalPath = Path.Combine(_configuration["PdfSettings:OutputPath"] ?? "wwwroot/pdfs", fileName);
                }

                // E-posta gönder
                try
                {
                    var guestName = invoice.Guest?.FullName ?? "Değerli Müşteri";
                    var subject = $"Fatura #{invoice.InvoiceNumber} - GuestFlow";
                    var body = $@"
Merhaba {guestName},

Faturanız ektedir.

Fatura Detayları:
- Fatura No: {invoice.InvoiceNumber}
- Tarih: {invoice.IssueDate:dd.MM.yyyy}
- Tutar: {invoice.TotalAmount:N2} {invoice.Currency}

İyi günler dileriz.
GuestFlow Ekibi";

                    var attachments = new List<string>();
                    if (System.IO.File.Exists(pdfPhysicalPath))
                    {
                        attachments.Add(pdfPhysicalPath);
                    }

                    var emailSent = await _emailService.SendEmailAsync(
                        email,
                        subject,
                        body,
                        false, // HTML değil, plain text
                        attachments
                    );

                    if (emailSent)
                    {
                        _logger.LogInformation($"Fatura e-postası gönderildi: {invoiceId} -> {email}");
                        return new ServiceMessage { IsSuccess = true, Message = "Fatura başarıyla e-posta ile gönderildi." };
                    }
                    else
                    {
                        return new ServiceMessage { IsSuccess = false, Message = "E-posta gönderilemedi." };
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"Fatura e-postası gönderilirken hata: {emailEx.Message}");
                    return new ServiceMessage { IsSuccess = false, Message = $"E-posta gönderilemedi: {emailEx.Message}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura e-postası gönderilirken hata: {ex.Message}. InvoiceId: {invoiceId}");
                return new ServiceMessage { IsSuccess = false, Message = $"E-posta gönderilemedi: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get services eligible for invoice creation for a guest
        /// </summary>
        public async Task<List<EligibleServiceDto>> GetEligibleServicesForInvoiceAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var result = new List<EligibleServiceDto>();

                // Get transfers not already invoiced
                var transferIds = await _invoiceItemRepository.GetAll()
                    .Where(ii => ii.ServiceType == "Transfer")
                    .Select(ii => ii.ServiceId)
                    .Distinct()
                    .ToListAsync();

                var transfers = await _transferRepository.GetAll()
                    .Where(t => t.GuestId == guestId && t.TransferDate.Date >= start.Date && t.TransferDate.Date <= end.Date)
                    .Where(t => !transferIds.Contains(t.Id)) // Not already in any invoice
                    .Select(t => new EligibleServiceDto
                    {
                        ServiceType = "Transfer",
                        ServiceId = t.Id,
                        ServiceDescription = $"{t.PickupAddress} → {t.DropoffAddress}",
                        ServiceDate = t.TransferDate,
                        Amount = t.FinalPrice,
                        Currency = t.Currency ?? "TRY",
                        IsAlreadyInvoiced = false,
                        GuestName = t.Guest.FullName
                    })
                    .ToListAsync();

                result.AddRange(transfers);

                // Get city tours not already invoiced
                var cityTourIds = await _invoiceItemRepository.GetAll()
                    .Where(ii => ii.ServiceType == "CityTour")
                    .Select(ii => ii.ServiceId)
                    .Distinct()
                    .ToListAsync();

                var cityTours = await _cityTourRepository.GetAll()
                    .Where(ct => ct.OwnerGuestId == guestId && ct.TourDate.Date >= start.Date && ct.TourDate.Date <= end.Date)
                    .Where(ct => !cityTourIds.Contains(ct.Id)) // Not already in any invoice
                    .Select(ct => new EligibleServiceDto
                    {
                        ServiceType = "CityTour",
                        ServiceId = ct.Id,
                        ServiceDescription = $"City Tour - {ct.DurationHours} hours",
                        ServiceDate = ct.TourDate,
                        Amount = ct.FinalPrice,
                        Currency = ct.Currency ?? "TRY",
                        IsAlreadyInvoiced = false,
                        GuestName = ct.OwnerGuest.FullName
                    })
                    .ToListAsync();

                result.AddRange(cityTours);

                // Get yacht tours not already invoiced
                var yachtTourIds = await _invoiceItemRepository.GetAll()
                    .Where(ii => ii.ServiceType == "YachtTour")
                    .Select(ii => ii.ServiceId)
                    .Distinct()
                    .ToListAsync();

                var yachtTours = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.OwnerGuestId == guestId && yt.TourDate.Date >= start.Date && yt.TourDate.Date <= end.Date)
                    .Where(yt => !yachtTourIds.Contains(yt.Id)) // Not already in any invoice
                    .Select(yt => new EligibleServiceDto
                    {
                        ServiceType = "YachtTour",
                        ServiceId = yt.Id,
                        ServiceDescription = $"Yacht Tour - {yt.YachtName ?? "Unknown"}",
                        ServiceDate = yt.TourDate,
                        Amount = yt.FinalPrice,
                        Currency = yt.Currency ?? "TRY",
                        IsAlreadyInvoiced = false,
                        GuestName = yt.OwnerGuest.FullName
                    })
                    .ToListAsync();

                result.AddRange(yachtTours);

                return result.OrderByDescending(s => s.ServiceDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eligible services for invoice error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create invoice manually with selected services
        /// </summary>
        public async Task<ServiceMessage<GetInvoiceDto>> CreateInvoiceAsync(CreateInvoiceDto createDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate guest exists
                var guest = await _guestRepository.GetByIdAsync(createDto.GuestId);
                if (guest == null)
                {
                    return new ServiceMessage<GetInvoiceDto> { IsSuccess = false, Message = "Misafir bulunamadı." };
                }

                // Get eligible services
                var eligibleServices = await GetEligibleServicesForInvoiceAsync(
                    createDto.GuestId,
                    createDto.StartDate,
                    createDto.EndDate);

                // If specific service IDs provided, filter to those
                if (createDto.SelectedServiceIds != null && createDto.SelectedServiceIds.Any())
                {
                    eligibleServices = eligibleServices
                        .Where(s => createDto.SelectedServiceIds.Contains(s.ServiceId))
                        .ToList();
                }

                if (!eligibleServices.Any())
                {
                    return new ServiceMessage<GetInvoiceDto> { IsSuccess = false, Message = "Fatura oluşturulacak uygun hizmet bulunamadı." };
                }

                // Create invoice
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = await GenerateInvoiceNumber(),
                    IssueDate = DateTime.UtcNow,
                    TotalAmount = eligibleServices.Sum(s => s.Amount),
                    Currency = createDto.Currency,
                    Notes = createDto.Notes,
                    GuestId = createDto.GuestId,
                    PersonnelId = createDto.CreatedByPersonnelId,
                    CreatedDate = DateTime.UtcNow,
                    Status = InvoiceStatus.Draft
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Create invoice items
                foreach (var service in eligibleServices)
                {
                    var invoiceItem = new InvoiceItemEntity
                    {
                        InvoiceId = invoice.Id,
                        ServiceType = service.ServiceType,
                        ServiceId = service.ServiceId,
                        Amount = service.Amount,
                        Currency = service.Currency,
                        Notes = $"Service: {service.ServiceDescription}",
                        CreatedDate = DateTime.UtcNow
                    };

                    await _invoiceItemRepository.AddAsync(invoiceItem);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Get the created invoice with items
                var result = await GetInvoiceDtoById(invoice.Id);

                _logger.LogInformation($"Fatura manuel olarak oluşturuldu: {invoice.InvoiceNumber}");
                return new ServiceMessage<GetInvoiceDto>
                {
                    IsSuccess = true,
                    Message = "Fatura başarıyla oluşturuldu.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Fatura oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<GetInvoiceDto> { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Update invoice (only allowed for Draft invoices that haven't been PDF generated)
        /// INVOICE IMMUTABILITY: Once PDF is generated, invoices become IMMUTABLE
        /// </summary>
        public async Task<ServiceMessage<GetInvoiceDto>> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceDto updateDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    return new ServiceMessage<GetInvoiceDto> { IsSuccess = false, Message = "Fatura bulunamadı." };
                }

                // INVOICE IMMUTABILITY: Check if invoice can be modified
                ValidateInvoiceCanBeModified(invoice);

                // Update allowed fields for Draft invoices only
                if (!string.IsNullOrEmpty(updateDto.Notes))
                {
                    invoice.Notes = updateDto.Notes;
                }

                await _invoiceRepository.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = _mapper.Map<GetInvoiceDto>(invoice);
                return new ServiceMessage<GetInvoiceDto> { IsSuccess = true, Message = "Fatura başarıyla güncellendi.", Data = result };
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex, $"Fatura güncelleme reddedildi (immutable): {ex.Message}");
                return new ServiceMessage<GetInvoiceDto> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Fatura güncelleme hatası: {ex.Message}");
                return new ServiceMessage<GetInvoiceDto> { IsSuccess = false, Message = $"Fatura güncelleme hatası: {ex.Message}" };
            }
        }

        /// <summary>
        /// Cancel invoice (only allowed for Draft invoices)
        /// INVOICE IMMUTABILITY: Generated invoices cannot be cancelled
        /// </summary>
        public async Task<ServiceMessage> CancelInvoiceAsync(int invoiceId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Fatura bulunamadı." };
                }

                // INVOICE IMMUTABILITY: Check if invoice can be modified
                ValidateInvoiceCanBeModified(invoice);

                // Only Draft invoices can be cancelled
                if (invoice.Status != InvoiceStatus.Draft)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Sadece Draft durumundaki faturalar iptal edilebilir." };
                }

                invoice.Status = InvoiceStatus.Cancelled;
                await _invoiceRepository.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ServiceMessage { IsSuccess = true, Message = "Fatura başarıyla iptal edildi." };
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex, $"Fatura iptal reddedildi (immutable): {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Fatura iptal hatası: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura iptal hatası: {ex.Message}" };
            }
        }

        /// <summary>
        /// Generate a unique, sequential invoice number
        /// INVOICE REALITY: Sequential numbering for financial records
        /// </summary>
        private async Task<int> GenerateInvoiceNumber()
        {
            try
            {
                // Get the highest existing invoice number
                var lastInvoice = await _invoiceRepository.GetAll()
                    .OrderByDescending(i => i.InvoiceNumber)
                    .Select(i => i.InvoiceNumber)
                    .FirstOrDefaultAsync();

                // Start from 1000 if no invoices exist, otherwise increment
                return lastInvoice >= 1000 ? lastInvoice + 1 : 1000;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice number");
                // Fallback to timestamp-based number to avoid conflicts
                return (int)(DateTime.UtcNow.Ticks % int.MaxValue);
            }
        }
    }
}