// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Guest.Dtos;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    /// <summary>
    /// Guest Preferences servisi
    /// </summary>
    public interface IGuestPreferencesService
    {
        /// <summary>
        /// Misafir tercihlerini getirir
        /// </summary>
        Task<ApiResponse<GuestPreferencesDto>> GetGuestPreferencesAsync(int guestId);

        /// <summary>
        /// Misafir tercihlerini oluşturur veya günceller
        /// </summary>
        Task<ApiResponse<GuestPreferencesDto>> UpsertGuestPreferencesAsync(int guestId, UpsertGuestPreferencesDto dto);

        /// <summary>
        /// Misafir tercihlerini siler
        /// </summary>
        Task<ApiResponse<bool>> DeleteGuestPreferencesAsync(int guestId);
    }
}
