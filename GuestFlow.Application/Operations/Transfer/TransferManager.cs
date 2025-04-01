using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Transfer
{
    public class TransferManager : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<VehicleEntity> _vehicleRepository;
        private readonly IRepository<AirportEntity> _airportRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly DailyRevenueJob _dailyRevenueJob;
        private readonly ILogger<TransferManager> _logger;

        public TransferManager(
            IUnitOfWork unitOfWork,
            IRepository<TransferEntity> transferRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<VehicleEntity> vehicleRepository,
            IRepository<AirportEntity> airportRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            DailyRevenueJob dailyRevenueJob,
            ILogger<TransferManager> logger)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _guestRepository = guestRepository;
            _vehicleRepository = vehicleRepository;
            _airportRepository = airportRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _dailyRevenueJob = dailyRevenueJob;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddTransfer(AddTransferDto transfer)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyonlar
                if (!await _guestRepository.GetAll(x => x.Id == transfer.GuestId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                if (!await _vehicleRepository.GetAll(x => x.Id == transfer.VehicleId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Araç bulunamadı." };

                if (!await _airportRepository.GetAll(x => x.Id == transfer.AirportId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Havalimanı bulunamadı." };

                if (!await _cityRepository.GetAll(x => x.Id == transfer.PickupCityId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Alış şehri bulunamadı." };

                if (!await _cityRepository.GetAll(x => x.Id == transfer.DropoffCityId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Bırakış şehri bulunamadı." };

                if (!await _personnelRepository.GetAll(x => x.Id == transfer.PersonnelId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                // Fiyat hesaplama
                decimal finalPrice = transfer.Price;
                if (transfer.DiscountPercentage.HasValue)
                    finalPrice -= finalPrice * (transfer.DiscountPercentage.Value / 100);

                // Transfer oluşturma
                var transferEntity = new TransferEntity
                {
                    TransferDate = transfer.TransferDate,
                    PickupAddress = transfer.PickupAddress,
                    DropoffAddress = transfer.DropoffAddress,
                    Price = transfer.Price,
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    AirportId = transfer.AirportId,
                    VehicleId = transfer.VehicleId,
                    Note = transfer.Note,
                    Status = transfer.Status,
                    IsFromAirport = transfer.IsFromAirport,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId,
                    DiscountPercentage = transfer.DiscountPercentage,
                    FinalPrice = finalPrice
                };

                await _transferRepository.AddAsync(transferEntity);
                await _unitOfWork.SaveChangesAsync();

                // Fatura oluşturma
                if (transfer.CreateInvoice)
                {
                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = "TRY",
                        Notes = transfer.InvoiceDescription ?? "Transfer faturası",
                        PdfUrl = $"https://example.com/invoices/invoice_{Guid.NewGuid()}.pdf",
                        GuestId = transfer.GuestId,
                        TransferId = transferEntity.Id,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Günlük gelir hesaplama
                await _dailyRevenueJob.CalculateDailyRevenue(transfer.TransferDate.Date);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer eklendi: {transferEntity.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> UpdateTransfer(UpdateTransferDto transfer)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyonlar
                var existing = await _transferRepository.GetAsync(x => x.Id == transfer.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                if (!await _guestRepository.GetAll(x => x.Id == transfer.GuestId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                if (!await _vehicleRepository.GetAll(x => x.Id == transfer.VehicleId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Araç bulunamadı." };

                if (!await _airportRepository.GetAll(x => x.Id == transfer.AirportId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Havalimanı bulunamadı." };

                if (!await _cityRepository.GetAll(x => x.Id == transfer.PickupCityId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Alış şehri bulunamadı." };

                if (!await _cityRepository.GetAll(x => x.Id == transfer.DropoffCityId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Bırakış şehri bulunamadı." };

                if (!await _personnelRepository.GetAll(x => x.Id == transfer.PersonnelId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                // Güncelleme
                existing.TransferDate = transfer.TransferDate;
                existing.PickupAddress = transfer.PickupAddress;
                existing.DropoffAddress = transfer.DropoffAddress;
                existing.Price = transfer.Price;
                existing.GuestId = transfer.GuestId;
                existing.PersonnelId = transfer.PersonnelId;
                existing.AirportId = transfer.AirportId;
                existing.VehicleId = transfer.VehicleId;
                existing.Note = transfer.Note;
                existing.Status = transfer.Status;
                existing.IsFromAirport = transfer.IsFromAirport;
                existing.PickupCityId = transfer.PickupCityId;
                existing.DropoffCityId = transfer.DropoffCityId;

                await _transferRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer güncellendi: {transfer.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> DeleteTransfer(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _transferRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<GetTransferDto> GetTransferById(int id)
        {
            try
            {
                var transfer = await _transferRepository.GetByIdAsync(id);
                if (transfer == null)
                    throw new Exception("Transfer bulunamadı.");

                return new GetTransferDto
                {
                    Id = transfer.Id,
                    TransferDate = transfer.TransferDate,
                    PickupAddress = transfer.PickupAddress,
                    DropoffAddress = transfer.DropoffAddress,
                    Price = transfer.Price,
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    AirportId = transfer.AirportId,
                    VehicleId = transfer.VehicleId,
                    Note = transfer.Note,
                    Status = transfer.Status,
                    IsFromAirport = transfer.IsFromAirport,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId,
                    CreatedDate = transfer.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<List<GetTransferDto>> GetTransfers()
        {
            try
            {
                return await _transferRepository.GetAll()
                    .Select(t => new GetTransferDto
                    {
                        Id = t.Id,
                        TransferDate = t.TransferDate,
                        PickupAddress = t.PickupAddress,
                        DropoffAddress = t.DropoffAddress,
                        Price = t.Price,
                        GuestId = t.GuestId,
                        PersonnelId = t.PersonnelId,
                        AirportId = t.AirportId,
                        VehicleId = t.VehicleId,
                        Note = t.Note,
                        Status = t.Status,
                        IsFromAirport = t.IsFromAirport,
                        PickupCityId = t.PickupCityId,
                        DropoffCityId = t.DropoffCityId,
                        CreatedDate = t.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transferler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
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