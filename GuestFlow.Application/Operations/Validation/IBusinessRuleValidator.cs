using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Application.Operations.Validation
{
    /// <summary>
    /// Business rules validation interface
    /// İş kuralları validasyon arayüzü
    /// </summary>
    public interface IBusinessRuleValidator
    {
        /// <summary>
        /// Validate transfer business rules
        /// Transfer iş kurallarını doğrula
        /// </summary>
        Task<ValidationResult> ValidateTransferAsync(TransferEntity transfer, AddTransferDto? dto = null);

        /// <summary>
        /// Validate city tour business rules
        /// Şehir turu iş kurallarını doğrula
        /// </summary>
        Task<ValidationResult> ValidateCityTourAsync(CityTourEntity tour, AddCityTourDto? dto = null);

        /// <summary>
        /// Validate yacht tour business rules
        /// Yat turu iş kurallarını doğrula
        /// </summary>
        Task<ValidationResult> ValidateYachtTourAsync(YachtTourEntity tour, AddYachtTourDto? dto = null);

        /// <summary>
        /// Validate transfer time conflicts
        /// Transfer zaman çakışmalarını doğrula
        /// </summary>
        Task<ValidationResult> ValidateTransferTimeConflictsAsync(TransferEntity transfer);

        /// <summary>
        /// Validate tour capacity
        /// Tur kapasitesini doğrula
        /// </summary>
        Task<ValidationResult> ValidateTourCapacityAsync(CityTourEntity tour);

        /// <summary>
        /// Validate yacht tour capacity
        /// Yat turu kapasitesini doğrula
        /// </summary>
        Task<ValidationResult> ValidateYachtTourCapacityAsync(YachtTourEntity tour);

        /// <summary>
        /// Calculate dynamic pricing
        /// Dinamik fiyat hesapla
        /// </summary>
        Task<decimal> CalculateDynamicPriceAsync(TransferEntity transfer);

        /// <summary>
        /// Calculate tour dynamic pricing
        /// Tur dinamik fiyat hesapla
        /// </summary>
        Task<decimal> CalculateTourDynamicPriceAsync(CityTourEntity tour);

        /// <summary>
        /// Apply automatic discounts
        /// Otomatik indirimleri uygula
        /// </summary>
        Task<decimal> ApplyAutomaticDiscountsAsync(TransferEntity transfer, decimal basePrice);
    }
}
