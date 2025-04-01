using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.YachtTour
{
    public class YachtTourManager : IYachtTourService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly DailyRevenueJob _dailyRevenueJob;
        private readonly ILogger<YachtTourManager> _logger;

        public YachtTourManager(
            IUnitOfWork unitOfWork,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            DailyRevenueJob dailyRevenueJob,
            ILogger<YachtTourManager> logger)
        {
            _unitOfWork = unitOfWork;
            _yachtTourRepository = yachtTourRepository;
            _guestRepository = guestRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _dailyRevenueJob = dailyRevenueJob;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddYachtTour(AddYachtTourDto yachtTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyonlar
                if (!await _guestRepository.GetAll(x => x.Id == yachtTour.OwnerGuestId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                if (!await _cityRepository.GetAll(x => x.Id == yachtTour.CityId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                if (!await _personnelRepository.GetAll(x => x.Id == yachtTour.PersonnelId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                // Fiyat hesaplama
                decimal finalPrice = yachtTour.Price;
                if (yachtTour.DiscountPercentage.HasValue)
                    finalPrice -= finalPrice * (yachtTour.DiscountPercentage.Value / 100);

                // Yat turu oluşturma
                var yachtTourEntity = new YachtTourEntity
                {
                    TourDate = yachtTour.TourDate,
                    NumberOfPeople = yachtTour.NumberOfPeople,
                    Price = yachtTour.Price,
                    SpecialRequest = yachtTour.SpecialRequest,
                    YachtName = yachtTour.YachtName,
                    OwnerGuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.PersonnelId,
                    CityId = yachtTour.CityId,
                    DiscountPercentage = yachtTour.DiscountPercentage,
                    FinalPrice = finalPrice
                };

                await _yachtTourRepository.AddAsync(yachtTourEntity);
                await _unitOfWork.SaveChangesAsync();

                // Fatura oluşturma
                if (yachtTour.CreateInvoice)
                {
                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = "TRY",
                        Notes = yachtTour.InvoiceDescription ?? "Yat turu faturası",
                        PdfUrl = $"https://example.com/invoices/invoice_{Guid.NewGuid()}.pdf",
                        GuestId = yachtTour.OwnerGuestId,
                        YachtTourId = yachtTourEntity.Id,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Günlük gelir hesaplama
                await _dailyRevenueJob.CalculateDailyRevenue(yachtTour.TourDate.Date);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu eklendi: {yachtTourEntity.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> UpdateYachtTour(UpdateYachtTourDto yachtTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _yachtTourRepository.GetAsync(x => x.Id == yachtTour.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu bulunamadı." };

                if (!await _guestRepository.GetAll(x => x.Id == yachtTour.OwnerGuestId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                if (!await _cityRepository.GetAll(x => x.Id == yachtTour.CityId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                if (!await _personnelRepository.GetAll(x => x.Id == yachtTour.PersonnelId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                // Güncelleme
                existing.TourDate = yachtTour.TourDate;
                existing.NumberOfPeople = yachtTour.NumberOfPeople;
                existing.Price = yachtTour.Price;
                existing.SpecialRequest = yachtTour.SpecialRequest;
                existing.YachtName = yachtTour.YachtName;
                existing.OwnerGuestId = yachtTour.OwnerGuestId;
                existing.PersonnelId = yachtTour.PersonnelId;
                existing.CityId = yachtTour.CityId;

                await _yachtTourRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu güncellendi: {yachtTour.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> DeleteYachtTour(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _yachtTourRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<GetYachtTourDto> GetYachtTourById(int id)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetByIdAsync(id);
                if (yachtTour == null)
                    throw new Exception("Yat turu bulunamadı.");

                return new GetYachtTourDto
                {
                    Id = yachtTour.Id,
                    TourDate = yachtTour.TourDate,
                    NumberOfPeople = yachtTour.NumberOfPeople,
                    Price = yachtTour.Price,
                    SpecialRequest = yachtTour.SpecialRequest,
                    YachtName = yachtTour.YachtName,
                    OwnerGuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.PersonnelId,
                    CityId = yachtTour.CityId,
                    CreatedDate = yachtTour.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turu getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<List<GetYachtTourDto>> GetYachtTours()
        {
            try
            {
                return await _yachtTourRepository.GetAll()
                    .Select(yt => new GetYachtTourDto
                    {
                        Id = yt.Id,
                        TourDate = yt.TourDate,
                        NumberOfPeople = yt.NumberOfPeople,
                        Price = yt.Price,
                        SpecialRequest = yt.SpecialRequest,
                        YachtName = yt.YachtName,
                        OwnerGuestId = yt.OwnerGuestId,
                        PersonnelId = yt.PersonnelId,
                        CityId = yt.CityId,
                        CreatedDate = yt.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turları listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        private async Task<int> GenerateInvoiceNumber()
        {
            var lastInvoice = await _invoiceRepository.GetAll()
                .OrderByDescending(x => x.InvoiceNumber)
                .FirstOrDefaultAsync();
            return lastInvoice != null ? lastInvoice.InvoiceNumber + 1 : 1000;
        }
    }
}