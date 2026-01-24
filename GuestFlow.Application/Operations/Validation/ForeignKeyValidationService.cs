using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Validation
{
    public class ForeignKeyValidationService : IForeignKeyValidationService
    {
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<VehicleEntity> _vehicleRepository;
        private readonly IRepository<AirportEntity> _airportRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<TourEntity> _tourRepository;
        private readonly ILogger<ForeignKeyValidationService> _logger;

        public ForeignKeyValidationService(
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<VehicleEntity> vehicleRepository,
            IRepository<AirportEntity> airportRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<TourEntity> tourRepository,
            ILogger<ForeignKeyValidationService> logger)
        {
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _vehicleRepository = vehicleRepository;
            _airportRepository = airportRepository;
            _cityRepository = cityRepository;
            _tourRepository = tourRepository;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateGuestIdAsync(int guestId)
        {
            try
            {
                if (guestId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir misafir ID'si gereklidir.",
                        FieldName = "GuestId"
                    };
                }

                var exists = await _guestRepository.GetAll(x => x.Id == guestId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {guestId} olan misafir bulunamadı veya silinmiş.",
                        FieldName = "GuestId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir ID validasyonu sırasında hata: {ex.Message}. GuestId: {guestId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Misafir ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "GuestId"
                };
            }
        }

        public async Task<ValidationResult> ValidatePersonnelIdAsync(int personnelId)
        {
            try
            {
                if (personnelId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir personel ID'si gereklidir.",
                        FieldName = "PersonnelId"
                    };
                }

                var exists = await _personnelRepository.GetAll(x => x.Id == personnelId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {personnelId} olan personel bulunamadı veya silinmiş.",
                        FieldName = "PersonnelId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel ID validasyonu sırasında hata: {ex.Message}. PersonnelId: {personnelId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Personel ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "PersonnelId"
                };
            }
        }

        public async Task<ValidationResult> ValidateVehicleIdAsync(int vehicleId)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir araç ID'si gereklidir.",
                        FieldName = "VehicleId"
                    };
                }

                var exists = await _vehicleRepository.GetAll(x => x.Id == vehicleId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {vehicleId} olan araç bulunamadı veya silinmiş.",
                        FieldName = "VehicleId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Araç ID validasyonu sırasında hata: {ex.Message}. VehicleId: {vehicleId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Araç ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "VehicleId"
                };
            }
        }

        public async Task<ValidationResult> ValidateAirportIdAsync(int airportId)
        {
            try
            {
                if (airportId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir havalimanı ID'si gereklidir.",
                        FieldName = "AirportId"
                    };
                }

                var exists = await _airportRepository.GetAll(x => x.Id == airportId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {airportId} olan havalimanı bulunamadı veya silinmiş.",
                        FieldName = "AirportId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Havalimanı ID validasyonu sırasında hata: {ex.Message}. AirportId: {airportId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Havalimanı ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "AirportId"
                };
            }
        }

        public async Task<ValidationResult> ValidateCityIdAsync(int cityId)
        {
            try
            {
                if (cityId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir şehir ID'si gereklidir.",
                        FieldName = "CityId"
                    };
                }

                var exists = await _cityRepository.GetAll(x => x.Id == cityId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {cityId} olan şehir bulunamadı veya silinmiş.",
                        FieldName = "CityId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir ID validasyonu sırasında hata: {ex.Message}. CityId: {cityId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Şehir ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "CityId"
                };
            }
        }

        public async Task<ValidationResult> ValidateTourIdAsync(int tourId)
        {
            try
            {
                if (tourId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir tur ID'si gereklidir.",
                        FieldName = "TourId"
                    };
                }

                var exists = await _tourRepository.GetAll(x => x.Id == tourId && !x.IsDeleted && x.IsActive).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {tourId} olan tur bulunamadı, silinmiş veya aktif değil.",
                        FieldName = "TourId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur ID validasyonu sırasında hata: {ex.Message}. TourId: {tourId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Tur ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "TourId"
                };
            }
        }

        public async Task<ValidationResult> ValidateDriverIdAsync(int driverId)
        {
            try
            {
                if (driverId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir şoför ID'si gereklidir.",
                        FieldName = "DriverId"
                    };
                }

                var exists = await _personnelRepository.GetAll(x => x.Id == driverId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {driverId} olan şoför bulunamadı veya silinmiş.",
                        FieldName = "DriverId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şoför ID validasyonu sırasında hata: {ex.Message}. DriverId: {driverId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Şoför ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "DriverId"
                };
            }
        }

        public async Task<ValidationResult> ValidateYachtIdAsync(int yachtId)
        {
            try
            {
                if (yachtId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir yat ID'si gereklidir.",
                        FieldName = "YachtId"
                    };
                }

                var exists = await _vehicleRepository.GetAll(x => x.Id == yachtId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {yachtId} olan yat (araç) bulunamadı veya silinmiş.",
                        FieldName = "YachtId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat ID validasyonu sırasında hata: {ex.Message}. YachtId: {yachtId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Yat ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "YachtId"
                };
            }
        }

        public async Task<ValidationResult> ValidateCaptainIdAsync(int captainId)
        {
            try
            {
                if (captainId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir kaptan ID'si gereklidir.",
                        FieldName = "CaptainId"
                    };
                }

                var exists = await _personnelRepository.GetAll(x => x.Id == captainId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {captainId} olan kaptan bulunamadı veya silinmiş.",
                        FieldName = "CaptainId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kaptan ID validasyonu sırasında hata: {ex.Message}. CaptainId: {captainId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Kaptan ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "CaptainId"
                };
            }
        }

        public async Task<ValidationResult> ValidateTourGuideIdAsync(int tourGuideId)
        {
            try
            {
                if (tourGuideId <= 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Geçerli bir tur rehberi ID'si gereklidir.",
                        FieldName = "TourGuideId"
                    };
                }

                var exists = await _personnelRepository.GetAll(x => x.Id == tourGuideId && !x.IsDeleted).AnyAsync();
                if (!exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"ID'si {tourGuideId} olan tur rehberi bulunamadı veya silinmiş.",
                        FieldName = "TourGuideId"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur rehberi ID validasyonu sırasında hata: {ex.Message}. TourGuideId: {tourGuideId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Tur rehberi ID validasyonu sırasında bir hata oluştu.",
                    FieldName = "TourGuideId"
                };
            }
        }

        public async Task<ValidationResult> ValidateMultipleAsync(ForeignKeyValidationRequest request)
        {
            try
            {
                // Misafir ID kontrolü
                if (request.GuestId.HasValue)
                {
                    var guestResult = await ValidateGuestIdAsync(request.GuestId.Value);
                    if (!guestResult.IsValid)
                        return guestResult;
                }

                // Personel ID kontrolü
                if (request.PersonnelId.HasValue)
                {
                    var personnelResult = await ValidatePersonnelIdAsync(request.PersonnelId.Value);
                    if (!personnelResult.IsValid)
                        return personnelResult;
                }

                // Araç ID kontrolü
                if (request.VehicleId.HasValue)
                {
                    var vehicleResult = await ValidateVehicleIdAsync(request.VehicleId.Value);
                    if (!vehicleResult.IsValid)
                        return vehicleResult;
                }

                // Havalimanı ID kontrolü
                if (request.AirportId.HasValue)
                {
                    var airportResult = await ValidateAirportIdAsync(request.AirportId.Value);
                    if (!airportResult.IsValid)
                        return airportResult;
                }

                // Şehir ID kontrolü
                if (request.CityId.HasValue)
                {
                    var cityResult = await ValidateCityIdAsync(request.CityId.Value);
                    if (!cityResult.IsValid)
                        return cityResult;
                }

                // Alış şehri ID kontrolü
                if (request.PickupCityId.HasValue)
                {
                    var pickupCityResult = await ValidateCityIdAsync(request.PickupCityId.Value);
                    if (!pickupCityResult.IsValid)
                    {
                        pickupCityResult.FieldName = "PickupCityId";
                        pickupCityResult.ErrorMessage = pickupCityResult.ErrorMessage.Replace("şehir", "alış şehri");
                        return pickupCityResult;
                    }
                }

                // Bırakış şehri ID kontrolü
                if (request.DropoffCityId.HasValue)
                {
                    var dropoffCityResult = await ValidateCityIdAsync(request.DropoffCityId.Value);
                    if (!dropoffCityResult.IsValid)
                    {
                        dropoffCityResult.FieldName = "DropoffCityId";
                        dropoffCityResult.ErrorMessage = dropoffCityResult.ErrorMessage.Replace("şehir", "bırakış şehri");
                        return dropoffCityResult;
                    }
                }

                // Tur ID kontrolü
                if (request.TourId.HasValue)
                {
                    var tourResult = await ValidateTourIdAsync(request.TourId.Value);
                    if (!tourResult.IsValid)
                        return tourResult;
                }

                // Şoför ID kontrolü
                if (request.DriverId.HasValue)
                {
                    var driverResult = await ValidateDriverIdAsync(request.DriverId.Value);
                    if (!driverResult.IsValid)
                        return driverResult;
                }

                // Yat ID kontrolü
                if (request.YachtId.HasValue)
                {
                    var yachtResult = await ValidateYachtIdAsync(request.YachtId.Value);
                    if (!yachtResult.IsValid)
                        return yachtResult;
                }

                // Kaptan ID kontrolü
                if (request.CaptainId.HasValue)
                {
                    var captainResult = await ValidateCaptainIdAsync(request.CaptainId.Value);
                    if (!captainResult.IsValid)
                        return captainResult;
                }

                // Tur rehberi ID kontrolü
                if (request.TourGuideId.HasValue)
                {
                    var tourGuideResult = await ValidateTourGuideIdAsync(request.TourGuideId.Value);
                    if (!tourGuideResult.IsValid)
                        return tourGuideResult;
                }

                // Asistan tur rehberi ID kontrolü
                if (request.AssistantGuideId.HasValue)
                {
                    var assistantGuideResult = await ValidateTourGuideIdAsync(request.AssistantGuideId.Value);
                    if (!assistantGuideResult.IsValid)
                    {
                        assistantGuideResult.FieldName = "AssistantGuideId";
                        assistantGuideResult.ErrorMessage = assistantGuideResult.ErrorMessage.Replace("tur rehberi", "asistan tur rehberi");
                        return assistantGuideResult;
                    }
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Toplu foreign key validasyonu sırasında hata: {ex.Message}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Foreign key validasyonu sırasında bir hata oluştu."
                };
            }
        }
    }
}

