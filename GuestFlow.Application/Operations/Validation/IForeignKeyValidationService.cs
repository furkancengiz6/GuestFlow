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
        /// Tur ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateTourIdAsync(int tourId);

        /// <summary>
        /// Şoför ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateDriverIdAsync(int driverId);

        /// <summary>
        /// Yat ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateYachtIdAsync(int yachtId);

        /// <summary>
        /// Kaptan ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateCaptainIdAsync(int captainId);

        /// <summary>
        /// Tur rehberi ID'sinin geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateTourGuideIdAsync(int tourGuideId);

        /// <summary>
        /// Birden fazla foreign key'i toplu olarak kontrol eder
        /// </summary>
        Task<ValidationResult> ValidateMultipleAsync(ForeignKeyValidationRequest request);
    }


    /// <summary>
    /// Toplu foreign key validasyon isteği
    /// </summary>
    public class ForeignKeyValidationRequest
    {
        public int? GuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public int? AirportId { get; set; }
        public int? CityId { get; set; }
        public int? PickupCityId { get; set; }
        public int? DropoffCityId { get; set; }
        public int? TourId { get; set; }
        public int? YachtId { get; set; }
        public int? CaptainId { get; set; }
        public int? TourGuideId { get; set; }
        public int? AssistantGuideId { get; set; }
    }
}

