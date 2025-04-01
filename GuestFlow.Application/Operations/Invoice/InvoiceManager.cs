using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public class InvoiceManager : IInvoiceService
    {
        // Bu iki değişkeni sınıfın içinde kullanıyoruz.
        // _invoiceRepository: Faturalarla ilgili veritabanı işlemlerini yapmak için kullanıyoruz.
        // _logger: Hataları veya bilgileri loglamak (kaydetmek) için kullanıyoruz.
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly ILogger<InvoiceManager> _logger;

        // Constructor (yapıcı metod): Bu sınıf oluşturulurken bağımlılıkları (dependency) buradan alıyoruz.
        public InvoiceManager(
            IRepository<InvoicesEntity> invoiceRepository,
            ILogger<InvoiceManager> logger)
        {
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        // Bu metod, belirli bir faturayı ID'sine göre getiriyor.
        public async Task<GetInvoiceDto> GetInvoiceById(int id)
        {
            try
            {
                // Veritabanından faturayı ID'sine göre çekiyoruz.
                // GetByIdAsync metodu, verilen ID ile eşleşen faturayı bulur.
                var invoice = await _invoiceRepository.GetByIdAsync(id);

                // Eğer fatura bulunamazsa, bir hata fırlatıyoruz (exception throw ediyoruz).
                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                // Fatura bulunduysa, onu bir DTO (Data Transfer Object) nesnesine çevirip geri döndürüyoruz.
                // DTO, veriyi taşımak için kullandığımız bir yapı. Burada fatura bilgilerini GetInvoiceDto'ya aktarıyoruz.
                return new GetInvoiceDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    TotalAmount = invoice.TotalAmount,
                    IssueDate = invoice.IssueDate,
                    Currency = invoice.Currency,
                    Notes = invoice.Notes,
                    PdfUrl = invoice.PdfUrl,
                    GuestId = invoice.GuestId,
                    PersonnelId = invoice.PersonnelId,
                    TransferId = invoice.TransferId,
                    CityTourId = invoice.CityTourId,
                    YachtTourId = invoice.YachtTourId,
                    CreatedDate = invoice.CreatedDate
                };
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
                // GetAll metodu tüm faturaları alır, Select ile her bir faturayı GetInvoiceDto'ya çeviriyoruz.
                // ToListAsync ile bu verileri bir liste haline getirip döndürüyoruz.
                return await _invoiceRepository.GetAll()
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl,
                        GuestId = i.GuestId,
                        PersonnelId = i.PersonnelId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();
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
                // GetAll metodu ile filtreleme yapıyoruz (x => x.GuestId == guestId), sonra Select ile her bir faturayı GetInvoiceDto'ya çeviriyoruz.
                // ToListAsync ile bu verileri bir liste haline getirip döndürüyoruz.
                return await _invoiceRepository.GetAll(x => x.GuestId == guestId)
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl,
                        GuestId = i.GuestId,
                        PersonnelId = i.PersonnelId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();
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
    }
}