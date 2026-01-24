// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Guest.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    /// <summary>
    /// Guest Preference Analysis Service - Misafir tercih analizi ve önerileri
    /// </summary>
    public interface IGuestPreferenceAnalysisService
    {
        /// <summary>
        /// Misafir tercih analizini getirir (oda, yemek, aktivite, iletişim)
        /// </summary>
        Task<ApiResponse<GuestPreferenceAnalysisDto>> GetPreferenceAnalysisAsync(int guestId);

        /// <summary>
        /// Misafir tercih önerileri getirir (Intelligence Layer'dan)
        /// </summary>
        Task<ApiResponse<List<PreferenceRecommendationDto>>> GetPreferenceRecommendationsAsync(int guestId);

        /// <summary>
        /// PMS'den gelen tercihleri GuestFlow tercihleri ile birleştirir
        /// </summary>
        Task<ApiResponse<GuestPreferencesDto>> MergePreferencesFromPMSAsync(int guestId, int pmsIntegrationId);

        /// <summary>
        /// Tercih uyumluluğunu hesaplar (misafir tercihleri ile mevcut hizmetler arasında)
        /// </summary>
        Task<ApiResponse<PreferenceCompatibilityDto>> CalculatePreferenceCompatibilityAsync(int guestId, string serviceType, int? serviceId = null);
    }

    /// <summary>
    /// Guest Preference Analysis DTO
    /// </summary>
    public class GuestPreferenceAnalysisDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;

        // Oda Tercih Analizi
        public RoomPreferenceAnalysis? RoomPreferences { get; set; }

        // Yemek Tercih Analizi
        public FoodPreferenceAnalysis? FoodPreferences { get; set; }

        // Aktivite Tercih Analizi
        public ActivityPreferenceAnalysis? ActivityPreferences { get; set; }

        // İletişim Tercih Analizi
        public CommunicationPreferenceAnalysis? CommunicationPreferences { get; set; }

        // Genel İstatistikler
        public PreferenceStatistics? Statistics { get; set; }
    }

    /// <summary>
    /// Oda Tercih Analizi
    /// </summary>
    public class RoomPreferenceAnalysis
    {
        public string? PreferredRoomType { get; set; }
        public List<string> SpecialRequests { get; set; } = new List<string>();
        public string? BedPreference { get; set; }
        public string? SmokingPreference { get; set; }
        public int UsageCount { get; set; } // Bu tercihlerin kaç kez kullanıldığı
        public double SatisfactionScore { get; set; } // Bu tercihlerle ilgili memnuniyet skoru
    }

    /// <summary>
    /// Yemek Tercih Analizi
    /// </summary>
    public class FoodPreferenceAnalysis
    {
        public List<string> DietaryPreferences { get; set; } = new List<string>(); // vegan, vegetarian, halal, kosher
        public List<string> FoodAllergies { get; set; } = new List<string>(); // peanut, dairy, gluten
        public List<string> SpecialRequests { get; set; } = new List<string>();
        public int RestaurantVisitCount { get; set; }
        public double AverageSatisfaction { get; set; }
    }

    /// <summary>
    /// Aktivite Tercih Analizi
    /// </summary>
    public class ActivityPreferenceAnalysis
    {
        public List<string> PreferredActivities { get; set; } = new List<string>(); // spor, kültür, eğlence
        public List<string> Interests { get; set; } = new List<string>(); // müze, plaj, gece hayatı, spa
        public int TourCount { get; set; }
        public double AverageSatisfaction { get; set; }
    }

    /// <summary>
    /// İletişim Tercih Analizi
    /// </summary>
    public class CommunicationPreferenceAnalysis
    {
        public bool PrefersEmail { get; set; }
        public bool PrefersSMS { get; set; }
        public bool PrefersWhatsApp { get; set; }
        public bool PrefersPhone { get; set; }
        public string? PreferredLanguage { get; set; }
        public Dictionary<string, int> ChannelUsageCount { get; set; } = new Dictionary<string, int>(); // Kanal bazlı kullanım sayısı
    }

    /// <summary>
    /// Tercih İstatistikleri
    /// </summary>
    public class PreferenceStatistics
    {
        public int TotalPreferences { get; set; }
        public int RoomPreferencesCount { get; set; }
        public int FoodPreferencesCount { get; set; }
        public int ActivityPreferencesCount { get; set; }
        public int CommunicationPreferencesCount { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Source { get; set; } // Manual, PMS, Intelligence
    }

    /// <summary>
    /// Tercih Önerisi DTO
    /// </summary>
    public class PreferenceRecommendationDto
    {
        public string RecommendationType { get; set; } = string.Empty; // Room, Food, Activity, Communication
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Confidence { get; set; } // 0.0 to 1.0
        public string? RecommendedValue { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// Tercih Uyumluluğu DTO
    /// </summary>
    public class PreferenceCompatibilityDto
    {
        public int GuestId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public int? ServiceId { get; set; }
        public double CompatibilityScore { get; set; } // 0.0 to 1.0
        public List<CompatibilityFactorDto> Factors { get; set; } = new List<CompatibilityFactorDto>();
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Uyumluluk Faktörü DTO
    /// </summary>
    public class CompatibilityFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public double Score { get; set; } // 0.0 to 1.0
        public string Description { get; set; } = string.Empty;
    }
}
