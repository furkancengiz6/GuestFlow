// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Intelligence.Graph.Dtos;

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    /// <summary>
    /// Graph data service interface - Node ve Edge oluşturma/yönetme
    /// </summary>
    public interface IGraphDataService
    {
        /// <summary>
        /// Guest node oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateGuestNodeAsync(int guestId, string guestName, string? guestCode = null);

        /// <summary>
        /// Staff node oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateStaffNodeAsync(int staffId, string staffName);

        /// <summary>
        /// Service node oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateServiceNodeAsync(int serviceId, string serviceType, string serviceName);

        /// <summary>
        /// Time node oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateTimeNodeAsync(DateTime date, string? timeOfDay = null, string? season = null);

        /// <summary>
        /// Emotion node oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateEmotionNodeAsync(string emotionType, double sentimentScore);

        /// <summary>
        /// Guest-Staff INTERACTS ilişkisi oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateGuestStaffInteractionAsync(GuestStaffInteractionDto dto);

        /// <summary>
        /// Guest PREFERS ilişkisi oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateGuestPreferenceAsync(GuestPreferenceDto dto);

        /// <summary>
        /// Service SATISFIES ilişkisi oluştur veya güncelle
        /// </summary>
        Task<bool> CreateOrUpdateServiceSatisfactionAsync(ServiceSatisfactionDto dto);

        /// <summary>
        /// Guest-Service OCCURS_AT ilişkisi oluştur
        /// </summary>
        Task<bool> CreateOccursAtRelationshipAsync(int guestId, int serviceId, DateTime date);

        /// <summary>
        /// Guest FEELS ilişkisi oluştur
        /// </summary>
        Task<bool> CreateFeelsRelationshipAsync(int guestId, string emotionType, double sentimentScore, DateTime timestamp);

        /// <summary>
        /// Guest node'u sil
        /// </summary>
        Task<bool> DeleteGuestNodeAsync(int guestId);

        /// <summary>
        /// Guest'in tüm ilişkilerini getir
        /// </summary>
        Task<Dictionary<string, object>> GetGuestRelationshipsAsync(int guestId);

        /// <summary>
        /// Guest-Staff uyum skorunu hesapla
        /// </summary>
        Task<double> CalculateGuestStaffCompatibilityAsync(int guestId, int staffId);
    }
}
