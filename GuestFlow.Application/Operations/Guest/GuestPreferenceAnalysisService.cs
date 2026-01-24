// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    /// <summary>
    /// Guest Preference Analysis Service implementation
    /// </summary>
    public class GuestPreferenceAnalysisService : IGuestPreferenceAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGuestPreferencesService _guestPreferencesService;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly IRelationshipIntelligenceService _relationshipIntelligenceService;
        private readonly GuestFlow.Domain.Entities.Repositories.IRepository<RoomAssignmentEntity> _roomAssignmentRepository;
        private readonly ILogger<GuestPreferenceAnalysisService> _logger;

        public GuestPreferenceAnalysisService(
            IUnitOfWork unitOfWork,
            IGuestPreferencesService guestPreferencesService,
            IPMSIntegrationService pmsIntegrationService,
            IRelationshipIntelligenceService relationshipIntelligenceService,
            GuestFlow.Domain.Entities.Repositories.IRepository<RoomAssignmentEntity> roomAssignmentRepository,
            ILogger<GuestPreferenceAnalysisService> logger)
        {
            _unitOfWork = unitOfWork;
            _guestPreferencesService = guestPreferencesService;
            _pmsIntegrationService = pmsIntegrationService;
            _relationshipIntelligenceService = relationshipIntelligenceService;
            _roomAssignmentRepository = roomAssignmentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<GuestPreferenceAnalysisDto>> GetPreferenceAnalysisAsync(int guestId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<GuestPreferenceAnalysisDto>.Fail("Guest not found");

                var preferencesResponse = await _guestPreferencesService.GetGuestPreferencesAsync(guestId);
                var preferences = preferencesResponse.Data;

                // Oda tercih analizi
                var roomAnalysis = await AnalyzeRoomPreferencesAsync(guestId, preferences);

                // Yemek tercih analizi
                var foodAnalysis = await AnalyzeFoodPreferencesAsync(guestId, preferences);

                // Aktivite tercih analizi
                var activityAnalysis = await AnalyzeActivityPreferencesAsync(guestId, preferences);

                // İletişim tercih analizi
                var communicationAnalysis = await AnalyzeCommunicationPreferencesAsync(guestId, preferences);

                // İstatistikler
                var statistics = new PreferenceStatistics
                {
                    TotalPreferences = CalculateTotalPreferencesCount(preferences),
                    RoomPreferencesCount = string.IsNullOrEmpty(preferences?.PreferredRoomType) ? 0 : 1,
                    FoodPreferencesCount = CalculateFoodPreferencesCount(preferences),
                    ActivityPreferencesCount = CalculateActivityPreferencesCount(preferences),
                    CommunicationPreferencesCount = CalculateCommunicationPreferencesCount(preferences),
                    LastUpdated = null, // UpdatedDate not available in GuestPreferencesDto
                    Source = preferences?.Source
                };

                var analysis = new GuestPreferenceAnalysisDto
                {
                    GuestId = guest.Id,
                    GuestName = guest.FullName,
                    RoomPreferences = roomAnalysis,
                    FoodPreferences = foodAnalysis,
                    ActivityPreferences = activityAnalysis,
                    CommunicationPreferences = communicationAnalysis,
                    Statistics = statistics
                };

                return ApiResponse<GuestPreferenceAnalysisDto>.SuccessResponse(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get preference analysis for guest {GuestId}", guestId);
                return ApiResponse<GuestPreferenceAnalysisDto>.Fail($"Failed to get preference analysis: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PreferenceRecommendationDto>>> GetPreferenceRecommendationsAsync(int guestId)
        {
            try
            {
                // Intelligence Layer'dan tercih önerilerini al
                var preferencePatterns = await _relationshipIntelligenceService.GetGuestPreferencePatternsAsync(guestId);
                
                var recommendations = new List<PreferenceRecommendationDto>();

                // Intelligence Layer'dan gelen pattern'lere göre öneriler oluştur
                if (preferencePatterns.ContainsKey("roomPreferences"))
                {
                    recommendations.Add(new PreferenceRecommendationDto
                    {
                        RecommendationType = "Room",
                        Title = "Oda Tercihi Önerisi",
                        Description = "Geçmiş konaklamalarınıza göre önerilen oda tipi",
                        Confidence = 0.8,
                        RecommendedValue = preferencePatterns["roomPreferences"]?.ToString(),
                        Context = preferencePatterns
                    });
                }

                if (preferencePatterns.ContainsKey("foodPreferences"))
                {
                    recommendations.Add(new PreferenceRecommendationDto
                    {
                        RecommendationType = "Food",
                        Title = "Yemek Tercihi Önerisi",
                        Description = "Tercih ettiğiniz yemek türleri",
                        Confidence = 0.75,
                        RecommendedValue = preferencePatterns["foodPreferences"]?.ToString(),
                        Context = preferencePatterns
                    });
                }

                return ApiResponse<List<PreferenceRecommendationDto>>.SuccessResponse(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get preference recommendations for guest {GuestId}", guestId);
                return ApiResponse<List<PreferenceRecommendationDto>>.Fail($"Failed to get preference recommendations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<GuestPreferencesDto>> MergePreferencesFromPMSAsync(int guestId, int pmsIntegrationId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<GuestPreferencesDto>.Fail("Guest not found");

                // PMS guest mapping'i bul
                var mapping = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.PMSIntegrationId == pmsIntegrationId && 
                                m.GuestFlowGuestId == guestId)
                    .FirstOrDefaultAsync();

                if (mapping == null)
                    return ApiResponse<GuestPreferencesDto>.Fail("PMS guest mapping not found");

                // PMS'den misafir profilini çek
                var pmsGuestResponse = await _pmsIntegrationService.GetGuestProfileAsync(
                    pmsIntegrationId, mapping.PMSGuestId);

                if (!pmsGuestResponse.Success || pmsGuestResponse.Data == null)
                    return ApiResponse<GuestPreferencesDto>.Fail("Failed to get PMS guest profile");

                var pmsGuest = pmsGuestResponse.Data;

                // Mevcut GuestFlow tercihlerini al
                var currentPreferencesResponse = await _guestPreferencesService.GetGuestPreferencesAsync(guestId);
                var currentPreferences = currentPreferencesResponse.Data;

                // PMS tercihlerini parse et (Preferences JSON'dan)
                var mergedPreferences = new UpsertGuestPreferencesDto
                {
                    GuestId = guestId,
                    Source = "PMS"
                };

                // PMS'den gelen tercihleri merge et
                if (!string.IsNullOrEmpty(pmsGuest.Preferences))
                {
                    // JSON parse et ve tercihleri çıkar
                    // TODO: PMS preferences format'ına göre parse et
                }

                // Mevcut tercihleri koru (PMS'de yoksa)
                if (currentPreferences != null)
                {
                    mergedPreferences.PreferredRoomType = currentPreferences.PreferredRoomType;
                    mergedPreferences.RoomSpecialRequests = currentPreferences.RoomSpecialRequests;
                    mergedPreferences.BedPreference = currentPreferences.BedPreference;
                    mergedPreferences.SmokingPreference = currentPreferences.SmokingPreference;
                    mergedPreferences.DietaryPreferences = currentPreferences.DietaryPreferences;
                    mergedPreferences.FoodAllergies = currentPreferences.FoodAllergies;
                    mergedPreferences.SpecialFoodRequests = currentPreferences.SpecialFoodRequests;
                    mergedPreferences.ActivityPreferences = currentPreferences.ActivityPreferences;
                    mergedPreferences.Interests = currentPreferences.Interests;
                    mergedPreferences.PrefersEmail = currentPreferences.PrefersEmail;
                    mergedPreferences.PrefersSMS = currentPreferences.PrefersSMS;
                    mergedPreferences.PrefersWhatsApp = currentPreferences.PrefersWhatsApp;
                    mergedPreferences.PrefersPhone = currentPreferences.PrefersPhone;
                    mergedPreferences.PreferredLanguage = currentPreferences.PreferredLanguage;
                    mergedPreferences.Notes = currentPreferences.Notes;
                }

                // Tercihleri kaydet
                var result = await _guestPreferencesService.UpsertGuestPreferencesAsync(guestId, mergedPreferences);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to merge preferences from PMS for guest {GuestId}", guestId);
                return ApiResponse<GuestPreferencesDto>.Fail($"Failed to merge preferences from PMS: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PreferenceCompatibilityDto>> CalculatePreferenceCompatibilityAsync(int guestId, string serviceType, int? serviceId = null)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<PreferenceCompatibilityDto>.Fail("Guest not found");

                var preferencesResponse = await _guestPreferencesService.GetGuestPreferencesAsync(guestId);
                var preferences = preferencesResponse.Data;

                var factors = new List<CompatibilityFactorDto>();
                double totalScore = 0.0;
                int factorCount = 0;

                // Service type'a göre uyumluluk hesapla
                switch (serviceType.ToUpperInvariant())
                {
                    case "RESTAURANT":
                        if (serviceId.HasValue)
                        {
                            // Restaurant entity'si için repository kontrolü
                            // TODO: Restaurant repository eklendiğinde bu kısım güncellenecek
                            // var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(serviceId.Value);
                            // if (restaurant != null)
                            // {
                            //     // Yemek tercihleri ile restoran uyumluluğu
                            //     if (preferences != null && !string.IsNullOrEmpty(preferences.DietaryPreferences))
                            //     {
                            //         // TODO: Restoran menüsü ile tercih uyumluluğu kontrol et
                            //         factors.Add(new CompatibilityFactorDto
                            //         {
                            //             FactorName = "Dietary Preferences",
                            //             Score = 0.8,
                            //             Description = "Dietary preferences match"
                            //         });
                            //         totalScore += 0.8;
                            //         factorCount++;
                            //     }
                            // }
                            
                            // Şimdilik basit kontrol
                            if (preferences != null && !string.IsNullOrEmpty(preferences.DietaryPreferences))
                            {
                                factors.Add(new CompatibilityFactorDto
                                {
                                    FactorName = "Dietary Preferences",
                                    Score = 0.8,
                                    Description = "Dietary preferences match"
                                });
                                totalScore += 0.8;
                                factorCount++;
                            }
                        }
                        break;

                    case "TOUR":
                    case "CITYTOUR":
                    case "YACHTTOUR":
                        if (preferences != null && !string.IsNullOrEmpty(preferences.ActivityPreferences))
                        {
                            factors.Add(new CompatibilityFactorDto
                            {
                                FactorName = "Activity Preferences",
                                Score = 0.85,
                                Description = "Tour matches activity preferences"
                            });
                            totalScore += 0.85;
                            factorCount++;
                        }
                        break;

                    case "ROOM":
                        if (preferences != null)
                        {
                            if (!string.IsNullOrEmpty(preferences.PreferredRoomType))
                            {
                                factors.Add(new CompatibilityFactorDto
                                {
                                    FactorName = "Room Type",
                                    Score = 0.9,
                                    Description = "Room type matches preference"
                                });
                                totalScore += 0.9;
                                factorCount++;
                            }

                            if (!string.IsNullOrEmpty(preferences.RoomSpecialRequests))
                            {
                                factors.Add(new CompatibilityFactorDto
                                {
                                    FactorName = "Special Requests",
                                    Score = 0.75,
                                    Description = "Special requests considered"
                                });
                                totalScore += 0.75;
                                factorCount++;
                            }
                        }
                        break;
                }

                var compatibilityScore = factorCount > 0 ? totalScore / factorCount : 0.0;

                var result = new PreferenceCompatibilityDto
                {
                    GuestId = guestId,
                    ServiceType = serviceType,
                    ServiceId = serviceId,
                    CompatibilityScore = compatibilityScore,
                    Factors = factors,
                    Recommendation = compatibilityScore >= 0.8 
                        ? "Highly compatible with guest preferences" 
                        : compatibilityScore >= 0.6 
                            ? "Moderately compatible" 
                            : "Low compatibility - consider alternatives"
                };

                return ApiResponse<PreferenceCompatibilityDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate preference compatibility for guest {GuestId}", guestId);
                return ApiResponse<PreferenceCompatibilityDto>.Fail($"Failed to calculate preference compatibility: {ex.Message}");
            }
        }

        // Helper methods
        private async Task<RoomPreferenceAnalysis?> AnalyzeRoomPreferencesAsync(int guestId, GuestPreferencesDto? preferences)
        {
            if (preferences == null)
                return null;

            // Oda atama geçmişinden analiz yap
            var roomAssignments = await _roomAssignmentRepository
                .GetAll(ra => ra.GuestId == guestId && !ra.IsDeleted)
                .ToListAsync();

            return new RoomPreferenceAnalysis
            {
                PreferredRoomType = preferences.PreferredRoomType,
                SpecialRequests = string.IsNullOrEmpty(preferences.RoomSpecialRequests) 
                    ? new List<string>() 
                    : preferences.RoomSpecialRequests.Split(',').Select(s => s.Trim()).ToList(),
                BedPreference = preferences.BedPreference,
                SmokingPreference = preferences.SmokingPreference,
                UsageCount = roomAssignments.Count(),
                SatisfactionScore = 0.85 // TODO: Intelligence Layer'dan al
            };
        }

        private async Task<FoodPreferenceAnalysis?> AnalyzeFoodPreferencesAsync(int guestId, GuestPreferencesDto? preferences)
        {
            if (preferences == null)
                return null;

            // Restoran rezervasyon geçmişinden analiz yap
            var restaurantReservations = await _unitOfWork.RestaurantReservations
                .GetAll(rr => rr.GuestId == guestId && !rr.IsDeleted)
                .ToListAsync();

            return new FoodPreferenceAnalysis
            {
                DietaryPreferences = string.IsNullOrEmpty(preferences.DietaryPreferences)
                    ? new List<string>()
                    : preferences.DietaryPreferences.Split(',').Select(s => s.Trim()).ToList(),
                FoodAllergies = string.IsNullOrEmpty(preferences.FoodAllergies)
                    ? new List<string>()
                    : preferences.FoodAllergies.Split(',').Select(s => s.Trim()).ToList(),
                SpecialRequests = string.IsNullOrEmpty(preferences.SpecialFoodRequests)
                    ? new List<string>()
                    : preferences.SpecialFoodRequests.Split(',').Select(s => s.Trim()).ToList(),
                RestaurantVisitCount = restaurantReservations.Count,
                AverageSatisfaction = 0.8 // TODO: Intelligence Layer'dan al
            };
        }

        private async Task<ActivityPreferenceAnalysis?> AnalyzeActivityPreferencesAsync(int guestId, GuestPreferencesDto? preferences)
        {
            if (preferences == null)
                return null;

            // Tur geçmişinden analiz yap
            var cityTours = await _unitOfWork.CityTours
                .GetAll(ct => ct.OwnerGuestId == guestId && !ct.IsDeleted)
                .ToListAsync();

            var yachtTours = await _unitOfWork.YachtTours
                .GetAll(yt => yt.OwnerGuestId == guestId && !yt.IsDeleted)
                .ToListAsync();

            return new ActivityPreferenceAnalysis
            {
                PreferredActivities = string.IsNullOrEmpty(preferences.ActivityPreferences)
                    ? new List<string>()
                    : preferences.ActivityPreferences.Split(',').Select(s => s.Trim()).ToList(),
                Interests = string.IsNullOrEmpty(preferences.Interests)
                    ? new List<string>()
                    : preferences.Interests.Split(',').Select(s => s.Trim()).ToList(),
                TourCount = cityTours.Count + yachtTours.Count,
                AverageSatisfaction = 0.85 // TODO: Intelligence Layer'dan al
            };
        }

        private async Task<CommunicationPreferenceAnalysis?> AnalyzeCommunicationPreferencesAsync(int guestId, GuestPreferencesDto? preferences)
        {
            if (preferences == null)
                return null;

            // İletişim geçmişinden analiz yap
            var emailCount = await _unitOfWork.EmailHistories
                .GetAll(e => e.To == preferences.PrefersEmail.ToString() && !e.IsDeleted)
                .CountAsync();

            var smsCount = await _unitOfWork.SmsHistories
                .GetAll(s => s.GuestId == guestId && !s.IsDeleted)
                .CountAsync();

            return new CommunicationPreferenceAnalysis
            {
                PrefersEmail = preferences.PrefersEmail,
                PrefersSMS = preferences.PrefersSMS,
                PrefersWhatsApp = preferences.PrefersWhatsApp,
                PrefersPhone = preferences.PrefersPhone,
                PreferredLanguage = preferences.PreferredLanguage,
                ChannelUsageCount = new Dictionary<string, int>
                {
                    { "Email", emailCount },
                    { "SMS", smsCount },
                    { "WhatsApp", 0 }, // TODO: WhatsApp history'den al
                    { "Phone", 0 }
                }
            };
        }

        private int CalculateTotalPreferencesCount(GuestPreferencesDto? preferences)
        {
            if (preferences == null) return 0;

            int count = 0;
            if (!string.IsNullOrEmpty(preferences.PreferredRoomType)) count++;
            if (!string.IsNullOrEmpty(preferences.RoomSpecialRequests)) count++;
            if (!string.IsNullOrEmpty(preferences.BedPreference)) count++;
            if (!string.IsNullOrEmpty(preferences.SmokingPreference)) count++;
            if (!string.IsNullOrEmpty(preferences.DietaryPreferences)) count++;
            if (!string.IsNullOrEmpty(preferences.FoodAllergies)) count++;
            if (!string.IsNullOrEmpty(preferences.SpecialFoodRequests)) count++;
            if (!string.IsNullOrEmpty(preferences.ActivityPreferences)) count++;
            if (!string.IsNullOrEmpty(preferences.Interests)) count++;
            if (preferences.PrefersEmail) count++;
            if (preferences.PrefersSMS) count++;
            if (preferences.PrefersWhatsApp) count++;
            if (preferences.PrefersPhone) count++;
            if (!string.IsNullOrEmpty(preferences.PreferredLanguage)) count++;

            return count;
        }

        private int CalculateFoodPreferencesCount(GuestPreferencesDto? preferences)
        {
            if (preferences == null) return 0;
            int count = 0;
            if (!string.IsNullOrEmpty(preferences.DietaryPreferences)) count++;
            if (!string.IsNullOrEmpty(preferences.FoodAllergies)) count++;
            if (!string.IsNullOrEmpty(preferences.SpecialFoodRequests)) count++;
            return count;
        }

        private int CalculateActivityPreferencesCount(GuestPreferencesDto? preferences)
        {
            if (preferences == null) return 0;
            int count = 0;
            if (!string.IsNullOrEmpty(preferences.ActivityPreferences)) count++;
            if (!string.IsNullOrEmpty(preferences.Interests)) count++;
            return count;
        }

        private int CalculateCommunicationPreferencesCount(GuestPreferencesDto? preferences)
        {
            if (preferences == null) return 0;
            int count = 0;
            if (preferences.PrefersEmail) count++;
            if (preferences.PrefersSMS) count++;
            if (preferences.PrefersWhatsApp) count++;
            if (preferences.PrefersPhone) count++;
            if (!string.IsNullOrEmpty(preferences.PreferredLanguage)) count++;
            return count;
        }
    }
}
