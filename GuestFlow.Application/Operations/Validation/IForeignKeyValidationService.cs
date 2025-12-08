using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Validation
{
    /// <summary>
    /// Foreign key validasyonları için servis
    /// </summary>
    public interface IForeignKeyValidationService
    {
        /// <summary>
        /// Misafir ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateGuestIdAsync(int guestId);

        /// <summary>
        /// Personel ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidatePersonnelIdAsync(int personnelId);

        /// <summary>
        /// Araç ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateVehicleIdAsync(int vehicleId);

        /// <summary>
        /// Havalimanı ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateAirportIdAsync(int airportId);

        /// <summary>
        /// Şehir ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateCityIdAsync(int cityId);

        /// <summary>
        /// Birden fazla foreign key'i toplu olarak kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateMultipleAsync(ForeignKeyValidationRequest request);
    }

    /// <summary>
    /// Validasyon sonucu
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Toplu foreign key validasyon isteği
    /// </summary>
    public class ForeignKeyValidationRequest
    {
        public int? GuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int? VehicleId { get; set; }
        public int? AirportId { get; set; }
        public int? CityId { get; set; }
        public int? PickupCityId { get; set; }
        public int? DropoffCityId { get; set; }
    }
}

