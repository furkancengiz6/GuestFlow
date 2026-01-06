using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Validation
{
    /// <summary>
    /// Business rules validation implementation
    /// İş kuralları validasyon implementasyonu
    /// </summary>
    public class BusinessRuleValidator : IBusinessRuleValidator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<VehicleEntity> _vehicleRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly ILogger<BusinessRuleValidator> _logger;

        public BusinessRuleValidator(
            IUnitOfWork unitOfWork,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<VehicleEntity> vehicleRepository,
            IRepository<PersonnelEntity> personnelRepository,
            ILogger<BusinessRuleValidator> logger)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _vehicleRepository = vehicleRepository;
            _personnelRepository = personnelRepository;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateTransferAsync(TransferEntity transfer, AddTransferDto? dto = null)
        {
            var errors = new List<string>();

            // Temel tarih kontrolü
            if (transfer.TransferDate < DateTime.Now.Date)
            {
                errors.Add("Transfer tarihi geçmiş bir tarih olamaz.");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                ErrorMessages = errors
            };
        }

        public async Task<ValidationResult> ValidateCityTourAsync(CityTourEntity tour, AddCityTourDto? dto = null)
        {
            var errors = new List<string>();

            // Temel tarih kontrolü
            if (tour.TourDate < DateTime.Now.Date)
            {
                errors.Add("Tur tarihi geçmiş bir tarih olamaz.");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                ErrorMessages = errors
            };
        }

        public async Task<ValidationResult> ValidateYachtTourAsync(YachtTourEntity tour, AddYachtTourDto? dto = null)
        {
            var errors = new List<string>();

            // Temel tarih kontrolü
            if (tour.TourDate < DateTime.Now.Date)
            {
                errors.Add("Yat turu tarihi geçmiş bir tarih olamaz.");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                ErrorMessages = errors
            };
        }

        public async Task<ValidationResult> ValidateTransferTimeConflictsAsync(TransferEntity transfer)
        {
            // Şoför çakışması kontrolü
            if (!transfer.DriverId.HasValue)
                return new ValidationResult { IsValid = true, ErrorMessages = new List<string>() };

            var conflictingTransfers = await _transferRepository.GetAll()
                .Where(t => t.DriverId == transfer.DriverId &&
                           t.Status != "Cancelled" &&
                           t.IsDeleted == false &&
                           t.Id != transfer.Id &&
                           t.TransferDate.Date == transfer.TransferDate.Date)
                .ToListAsync();

            var errors = new List<string>();
            if (conflictingTransfers.Any())
            {
                errors.Add("Seçilen şoför bu tarihte başka bir transferi var.");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                ErrorMessages = errors
            };
        }

        public async Task<ValidationResult> ValidateTourCapacityAsync(CityTourEntity tour)
        {
            var totalParticipants = (tour.AdultCount ?? 0) + (tour.ChildCount ?? 0) + (tour.InfantCount ?? 0);
            var errors = new List<string>();

            // Kapasite kontrolü
            if (tour.MaximumParticipantCount.HasValue && totalParticipants > tour.MaximumParticipantCount.Value)
            {
                errors.Add($"Katılımcı sayısı ({totalParticipants}) tur kapasitesini aşmaktadır.");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                ErrorMessages = errors
            };
        }

        public async Task<ValidationResult> ValidateYachtTourCapacityAsync(YachtTourEntity tour)
        {
            var errors = new List<string>();
            var totalParticipants = tour.NumberOfPeople;

            // Yasal limit kontrolü
            const int LEGAL_LIMIT = 12;
            if (totalParticipants > LEGAL_LIMIT)
            {
                errors.Add($"Katılımcı sayısı ({totalParticipants}) yasal limiti aşmaktadır.");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                ErrorMessages = errors
            };
        }

        public async Task<decimal> CalculateDynamicPriceAsync(TransferEntity transfer)
        {
            var basePrice = transfer.FinalPrice;

            // VIP ek ücreti
            if (transfer.IsVip)
            {
                basePrice *= 1.5m;
            }

            // Grup indirimi
            var groupSize = transfer.GroupSize ?? 1;
            if (groupSize >= 4)
            {
                basePrice *= 0.9m;
            }

            return Math.Round(basePrice, 2);
        }

        public async Task<decimal> CalculateTourDynamicPriceAsync(CityTourEntity tour)
        {
            var basePrice = tour.Price;

            // Grup indirimi
            var totalParticipants = (tour.AdultCount ?? 0) + (tour.ChildCount ?? 0);
            if (totalParticipants >= 6)
            {
                basePrice *= 0.85m;
            }

            return Math.Round(basePrice, 2);
        }

        public async Task<decimal> ApplyAutomaticDiscountsAsync(TransferEntity transfer, decimal basePrice)
        {
            var finalPrice = basePrice;

            // Erken rezervasyon indirimi
            var daysUntilTransfer = (transfer.TransferDate - DateTime.Now).TotalDays;
            if (daysUntilTransfer >= 30)
            {
                finalPrice *= 0.95m; // %5 indirim
            }
            else if (daysUntilTransfer >= 14)
            {
                finalPrice *= 0.97m; // %3 indirim
            }

            return Math.Round(finalPrice, 2);
        }
    }
}