using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Common
{
    /// <summary>
    /// Fatura oluşturma servisi - Tekrarlanan fatura oluşturma mantığını merkezileştirir
    /// </summary>
    public interface IInvoiceCreationService
    {
        /// <summary>
        /// Transfer için fatura oluşturur
        /// </summary>
        Task<ServiceMessage> CreateInvoiceForTransferAsync(
            TransferEntity transfer,
            GuestEntity guest,
            PersonnelEntity? personnel = null);

        /// <summary>
        /// Şehir turu için fatura oluşturur
        /// </summary>
        Task<ServiceMessage> CreateInvoiceForCityTourAsync(
            CityTourEntity cityTour,
            GuestEntity guest,
            PersonnelEntity? personnel = null);

        /// <summary>
        /// Yat turu için fatura oluşturur
        /// </summary>
        Task<ServiceMessage> CreateInvoiceForYachtTourAsync(
            YachtTourEntity yachtTour,
            GuestEntity guest,
            PersonnelEntity? personnel = null);
    }

    public class InvoiceCreationService : IInvoiceCreationService
    {
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceCreationService> _logger;

        public InvoiceCreationService(
            IRepository<InvoicesEntity> invoiceRepository,
            IPdfService pdfService,
            IEmailService emailService,
            IPdfUrlService pdfUrlService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<InvoiceCreationService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _pdfService = pdfService;
            _emailService = emailService;
            _pdfUrlService = pdfUrlService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceMessage> CreateInvoiceForTransferAsync(
            TransferEntity transfer,
            GuestEntity guest,
            PersonnelEntity? personnel = null)
        {
            try
            {
                // Fatura numarası oluştur
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber,
                    IssueDate = DateTime.UtcNow,
                    TotalAmount = transfer.FinalPrice,
                    Currency = transfer.Currency,
                    Notes = $"Transfer - {transfer.PickupAddress} → {transfer.DropoffAddress}",
                    PdfUrl = string.Empty,
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId > 0 ? transfer.PersonnelId : null,
                    // TransferId removed - invoices are now multi-service
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // PDF oluştur ve e-posta gönder
                await GeneratePdfAndSendEmail(invoice, guest, personnel, "Transfer");

                return new ServiceMessage { IsSuccess = true, Message = "Transfer için fatura başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer fatura oluşturulurken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> CreateInvoiceForCityTourAsync(
            CityTourEntity cityTour,
            GuestEntity guest,
            PersonnelEntity? personnel = null)
        {
            try
            {
                // Fatura numarası oluştur
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber,
                    IssueDate = DateTime.UtcNow,
                    TotalAmount = cityTour.FinalPrice,
                    Currency = cityTour.Currency,
                    Notes = $"Şehir Turu - {cityTour.City?.CityName ?? "Şehir"} - {cityTour.Language}",
                    PdfUrl = string.Empty,
                    GuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId > 0 ? cityTour.PersonnelId : null,
                    // CityTourId removed - invoices are now multi-service
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // PDF oluştur ve e-posta gönder
                await GeneratePdfAndSendEmail(invoice, guest, personnel, "CityTour");

                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu için fatura başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turu fatura oluşturulurken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> CreateInvoiceForYachtTourAsync(
            YachtTourEntity yachtTour,
            GuestEntity guest,
            PersonnelEntity? personnel = null)
        {
            try
            {
                // Fatura numarası oluştur
                var invoiceNumber = await GenerateInvoiceNumberAsync();

                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber,
                    IssueDate = DateTime.UtcNow,
                    TotalAmount = yachtTour.FinalPrice,
                    Currency = yachtTour.Currency,
                    Notes = $"Yat Turu - {yachtTour.YachtName} - {yachtTour.City?.CityName ?? "Şehir"}",
                    PdfUrl = string.Empty,
                    GuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.PersonnelId > 0 ? yachtTour.PersonnelId : null,
                    // YachtTourId removed - invoices are now multi-service
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // PDF oluştur ve e-posta gönder
                await GeneratePdfAndSendEmail(invoice, guest, personnel, "YachtTour");

                return new ServiceMessage { IsSuccess = true, Message = "Yat turu için fatura başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turu fatura oluşturulurken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        #region Private Methods

        private async Task GeneratePdfAndSendEmail(InvoicesEntity invoice, GuestEntity guest, PersonnelEntity? personnel, string serviceType)
        {
            try
            {
                // PDF oluştur
                var pdfUrl = await _pdfService.GenerateInvoicePdfAsync(invoice, guest, personnel);
                invoice.PdfUrl = pdfUrl;
                await _invoiceRepository.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // E-posta gönder
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
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"{serviceType} fatura e-postası gönderilirken hata: {emailEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{serviceType} fatura PDF'i oluşturulurken hata: {ex.Message}");
                // PDF oluşturma hatası fatura oluşturmayı engellemez, sadece loglanır
            }
        }

        private async Task<int> GenerateInvoiceNumberAsync()
        {
            var maxAttempts = 100;
            var newInvoiceNumber = DateTime.UtcNow.Year * 1000000 + DateTime.UtcNow.Month * 10000 + DateTime.UtcNow.Day * 100 + 1;

            for (int i = 0; i < maxAttempts; i++)
            {
                var exists = await _invoiceRepository.GetAll()
                    .AnyAsync(inv => inv.InvoiceNumber == newInvoiceNumber && !inv.IsDeleted);

                if (!exists)
                {
                    return newInvoiceNumber;
                }

                newInvoiceNumber++;
            }

            _logger.LogWarning("Fatura numarası oluşturulurken benzersizlik kontrolü başarısız oldu, timestamp bazlı numara kullanılıyor.");
            return int.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss")) % 10000000;
        }

        #endregion
    }
}

