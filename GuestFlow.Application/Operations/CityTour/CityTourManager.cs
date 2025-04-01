using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.CityTour
{
    public class CityTourManager : ICityTourService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly ILogger<CityTourManager> _logger;

        public CityTourManager(
            IUnitOfWork unitOfWork,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            ILogger<CityTourManager> logger)
        {
            _unitOfWork = unitOfWork;
            _cityTourRepository = cityTourRepository;
            _guestRepository = guestRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddCityTour(AddCityTourDto cityTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var guestExists = await _guestRepository.GetAll(x => x.Id == cityTour.OwnerGuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == cityTour.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                var personnelExists = await _personnelRepository.GetAll(x => x.Id == cityTour.PersonnelId).AnyAsync();
                if (!personnelExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                decimal finalPrice = cityTour.Price;
                if (cityTour.DiscountPercentage.HasValue)
                    finalPrice -= finalPrice * (cityTour.DiscountPercentage.Value / 100);

                var cityTourEntity = new CityTourEntity
                {
                    TourDate = cityTour.TourDate,
                    Language = cityTour.Language,
                    DurationHours = cityTour.DurationHours,
                    Price = cityTour.Price,
                    OwnerGuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId,
                    CityId = cityTour.CityId,
                    DiscountPercentage = cityTour.DiscountPercentage,
                    FinalPrice = finalPrice
                };

                await _cityTourRepository.AddAsync(cityTourEntity);
                await _unitOfWork.SaveChangesAsync();

                if (cityTour.CreateInvoice)
                {
                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = "TRY", // Varsayılan para birimi
                        Notes = cityTour.InvoiceDescription ?? "Şehir turu faturası",
                        PdfUrl = "https://example.com/invoices/invoice_" + Guid.NewGuid().ToString() + ".pdf", // Örnek bir PDF URL
                        GuestId = cityTour.OwnerGuestId,
                        CityTourId = cityTourEntity.Id,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Şehir turu eklendi: {Id}", cityTourEntity.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Şehir turu eklenirken hata oluştu. InnerException: {InnerException}", ex.InnerException?.Message);
                return new ServiceMessage { IsSuccess = false, Message = "Şehir turu eklenirken hata: " + ex.Message + (ex.InnerException != null ? " InnerException: " + ex.InnerException.Message : "") };
            }
        }

        private async Task<int> GenerateInvoiceNumber()
        {
            var lastInvoice = await _invoiceRepository.GetAll().OrderByDescending(x => x.InvoiceNumber).FirstOrDefaultAsync();
            return lastInvoice != null ? lastInvoice.InvoiceNumber + 1 : 1000; // Başlangıç numarası 1000
        }

        public async Task<ServiceMessage> UpdateCityTour(UpdateCityTourDto cityTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _cityTourRepository.GetAsync(x => x.Id == cityTour.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu bulunamadı." };

                var guestExists = await _guestRepository.GetAll(x => x.Id == cityTour.OwnerGuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == cityTour.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                var personnelExists = await _personnelRepository.GetAll(x => x.Id == cityTour.PersonnelId).AnyAsync();
                if (!personnelExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                existing.TourDate = cityTour.TourDate;
                existing.Language = cityTour.Language;
                existing.DurationHours = cityTour.DurationHours;
                existing.Price = cityTour.Price;
                existing.OwnerGuestId = cityTour.OwnerGuestId;
                existing.PersonnelId = cityTour.PersonnelId;
                existing.CityId = cityTour.CityId;

                await _cityTourRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Şehir turu güncellendi: {Id}", cityTour.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Şehir turu güncellenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Şehir turu güncellenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> DeleteCityTour(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _cityTourRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Şehir turu silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Şehir turu silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Şehir turu silinirken hata: " + ex.Message };
            }
        }

        public async Task<GetCityTourDto> GetCityTourById(int id)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetByIdAsync(id);
                if (cityTour == null)
                    throw new Exception("Şehir turu bulunamadı.");

                return new GetCityTourDto
                {
                    Id = cityTour.Id,
                    TourDate = cityTour.TourDate,
                    Language = cityTour.Language,
                    DurationHours = cityTour.DurationHours,
                    Price = cityTour.Price,
                    OwnerGuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId,
                    CityId = cityTour.CityId,
                    CreatedDate = cityTour.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şehir turu getirilirken hata oluştu: {Id}", id);
                throw;
            }
        }

        public async Task<List<GetCityTourDto>> GetCityTours()
        {
            try
            {
                var cityTours = await _cityTourRepository.GetAll()
                    .Select(ct => new GetCityTourDto
                    {
                        Id = ct.Id,
                        TourDate = ct.TourDate,
                        Language = ct.Language,
                        DurationHours = ct.DurationHours,
                        Price = ct.Price,
                        OwnerGuestId = ct.OwnerGuestId,
                        PersonnelId = ct.PersonnelId,
                        CityId = ct.CityId,
                        CreatedDate = ct.CreatedDate
                    })
                    .ToListAsync();

                return cityTours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şehir turları listelenirken hata oluştu.");
                throw;
            }
        }
    }
}