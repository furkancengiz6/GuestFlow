using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly ILogger<YachtTourManager> _logger;

        public YachtTourManager(
            IUnitOfWork unitOfWork,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            ILogger<YachtTourManager> logger)
        {
            _unitOfWork = unitOfWork;
            _yachtTourRepository = yachtTourRepository;
            _guestRepository = guestRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }
        public async Task<ServiceMessage> AddYachtTour(AddYachtTourDto yachtTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var guestExists = await _guestRepository.GetAll(x => x.Id == yachtTour.OwnerGuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == yachtTour.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                var personnelExists = await _personnelRepository.GetAll(x => x.Id == yachtTour.PersonnelId).AnyAsync();
                if (!personnelExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                decimal finalPrice = yachtTour.Price;
                if (yachtTour.DiscountPercentage.HasValue)
                    finalPrice -= finalPrice * (yachtTour.DiscountPercentage.Value / 100);

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

                if (yachtTour.CreateInvoice)
                {
                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = "TRY",
                        Notes = yachtTour.InvoiceDescription ?? "Yat turu faturası",
                        PdfUrl = "https://example.com/invoices/invoice_" + Guid.NewGuid().ToString() + ".pdf",
                        GuestId = yachtTour.OwnerGuestId,
                        YachtTourId = yachtTourEntity.Id,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Yat turu eklendi: {Id}", yachtTourEntity.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Yat turu eklenirken hata oluştu. InnerException: {InnerException}", ex.InnerException?.Message);
                return new ServiceMessage { IsSuccess = false, Message = "Yat turu eklenirken hata: " + ex.Message + (ex.InnerException != null ? " InnerException: " + ex.InnerException.Message : "") };
            }
        }

        private async Task<int> GenerateInvoiceNumber()
        {
            var lastInvoice = await _invoiceRepository.GetAll().OrderByDescending(x => x.InvoiceNumber).FirstOrDefaultAsync();
            return lastInvoice != null ? lastInvoice.InvoiceNumber + 1 : 1000;
        }

        public async Task<ServiceMessage> UpdateYachtTour(UpdateYachtTourDto yachtTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _yachtTourRepository.GetAsync(x => x.Id == yachtTour.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu bulunamadı." };

                var guestExists = await _guestRepository.GetAll(x => x.Id == yachtTour.OwnerGuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == yachtTour.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                var personnelExists = await _personnelRepository.GetAll(x => x.Id == yachtTour.PersonnelId).AnyAsync();
                if (!personnelExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

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

                _logger.LogInformation("Yat turu güncellendi: {Id}", yachtTour.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Yat turu güncellenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Yat turu güncellenirken hata: " + ex.Message };
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

                _logger.LogInformation("Yat turu silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Yat turu silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Yat turu silinirken hata: " + ex.Message };
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
                _logger.LogError(ex, "Yat turu getirilirken hata oluştu: {Id}", id);
                throw;
            }
        }

        public async Task<List<GetYachtTourDto>> GetYachtTours()
        {
            try
            {
                var yachtTours = await _yachtTourRepository.GetAll()
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

                return yachtTours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yat turları listelenirken hata oluştu.");
                throw;
            }
        }
    }
}