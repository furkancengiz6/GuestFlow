// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Guest.Dtos
{
    /// <summary>
    /// Guest Preferences DTO
    /// </summary>
    public class GuestPreferencesDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }

        // Oda Tercihleri
        public string? PreferredRoomType { get; set; }
        public string? RoomSpecialRequests { get; set; }
        public string? BedPreference { get; set; }
        public string? SmokingPreference { get; set; }

        // Yemek Tercihleri
        public string? DietaryPreferences { get; set; }
        public string? FoodAllergies { get; set; }
        public string? SpecialFoodRequests { get; set; }

        // Aktivite Tercihleri
        public string? ActivityPreferences { get; set; }
        public string? Interests { get; set; }

        // İletişim Tercihleri
        public bool PrefersEmail { get; set; } = true;
        public bool PrefersSMS { get; set; } = true;
        public bool PrefersWhatsApp { get; set; } = false;
        public bool PrefersPhone { get; set; } = true;
        public string? PreferredLanguage { get; set; }

        // Genel
        public string? Notes { get; set; }
        public string Source { get; set; } = "Manual";
    }

    /// <summary>
    /// Guest Preferences Create/Update DTO
    /// </summary>
    public class UpsertGuestPreferencesDto
    {
        public int GuestId { get; set; }

        // Oda Tercihleri
        public string? PreferredRoomType { get; set; }
        public string? RoomSpecialRequests { get; set; }
        public string? BedPreference { get; set; }
        public string? SmokingPreference { get; set; }

        // Yemek Tercihleri
        public string? DietaryPreferences { get; set; }
        public string? FoodAllergies { get; set; }
        public string? SpecialFoodRequests { get; set; }

        // Aktivite Tercihleri
        public string? ActivityPreferences { get; set; }
        public string? Interests { get; set; }

        // İletişim Tercihleri
        public bool PrefersEmail { get; set; } = true;
        public bool PrefersSMS { get; set; } = true;
        public bool PrefersWhatsApp { get; set; } = false;
        public bool PrefersPhone { get; set; } = true;
        public string? PreferredLanguage { get; set; }

        // Genel
        public string? Notes { get; set; }
        public string Source { get; set; } = "Manual";
    }
}
