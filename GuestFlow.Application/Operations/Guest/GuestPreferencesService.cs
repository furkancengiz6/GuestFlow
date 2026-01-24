// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    /// <summary>
    /// Guest Preferences servisi implementasyonu
    /// </summary>
    public class GuestPreferencesService : IGuestPreferencesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GuestPreferencesService> _logger;

        public GuestPreferencesService(
            IUnitOfWork unitOfWork,
            ILogger<GuestPreferencesService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<GuestPreferencesDto>> GetGuestPreferencesAsync(int guestId)
        {
            try
            {
                var preferences = await _unitOfWork.GuestPreferences
                    .GetAll(p => p.GuestId == guestId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (preferences == null)
                {
                    return ApiResponse<GuestPreferencesDto>.Fail("Guest preferences not found");
                }

                var dto = MapToDto(preferences);
                return ApiResponse<GuestPreferencesDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest preferences for guest {GuestId}", guestId);
                return ApiResponse<GuestPreferencesDto>.Fail($"Failed to get guest preferences: {ex.Message}");
            }
        }

        public async Task<ApiResponse<GuestPreferencesDto>> UpsertGuestPreferencesAsync(int guestId, UpsertGuestPreferencesDto dto)
        {
            try
            {
                // Guest kontrolü
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                {
                    return ApiResponse<GuestPreferencesDto>.Fail("Guest not found");
                }

                // Mevcut tercihleri kontrol et
                var existingPreferences = await _unitOfWork.GuestPreferences
                    .GetAll(p => p.GuestId == guestId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                GuestPreferencesEntity preferences;

                if (existingPreferences != null)
                {
                    // Güncelle
                    existingPreferences.PreferredRoomType = dto.PreferredRoomType;
                    existingPreferences.RoomSpecialRequests = dto.RoomSpecialRequests;
                    existingPreferences.BedPreference = dto.BedPreference;
                    existingPreferences.SmokingPreference = dto.SmokingPreference;
                    existingPreferences.DietaryPreferences = dto.DietaryPreferences;
                    existingPreferences.FoodAllergies = dto.FoodAllergies;
                    existingPreferences.SpecialFoodRequests = dto.SpecialFoodRequests;
                    existingPreferences.ActivityPreferences = dto.ActivityPreferences;
                    existingPreferences.Interests = dto.Interests;
                    existingPreferences.PrefersEmail = dto.PrefersEmail;
                    existingPreferences.PrefersSMS = dto.PrefersSMS;
                    existingPreferences.PrefersWhatsApp = dto.PrefersWhatsApp;
                    existingPreferences.PrefersPhone = dto.PrefersPhone;
                    existingPreferences.PreferredLanguage = dto.PreferredLanguage;
                    existingPreferences.Notes = dto.Notes;
                    existingPreferences.Source = dto.Source;

                    _unitOfWork.GuestPreferences.Update(existingPreferences);
                    preferences = existingPreferences;
                }
                else
                {
                    // Yeni oluştur
                    preferences = new GuestPreferencesEntity
                    {
                        GuestId = guestId,
                        PreferredRoomType = dto.PreferredRoomType,
                        RoomSpecialRequests = dto.RoomSpecialRequests,
                        BedPreference = dto.BedPreference,
                        SmokingPreference = dto.SmokingPreference,
                        DietaryPreferences = dto.DietaryPreferences,
                        FoodAllergies = dto.FoodAllergies,
                        SpecialFoodRequests = dto.SpecialFoodRequests,
                        ActivityPreferences = dto.ActivityPreferences,
                        Interests = dto.Interests,
                        PrefersEmail = dto.PrefersEmail,
                        PrefersSMS = dto.PrefersSMS,
                        PrefersWhatsApp = dto.PrefersWhatsApp,
                        PrefersPhone = dto.PrefersPhone,
                        PreferredLanguage = dto.PreferredLanguage,
                        Notes = dto.Notes,
                        Source = dto.Source
                    };

                    await _unitOfWork.GuestPreferences.AddAsync(preferences);
                }

                await _unitOfWork.CommitAsync();

                var resultDto = MapToDto(preferences);
                return ApiResponse<GuestPreferencesDto>.SuccessResponse(resultDto, 
                    existingPreferences != null ? "Guest preferences updated successfully" : "Guest preferences created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert guest preferences for guest {GuestId}", guestId);
                return ApiResponse<GuestPreferencesDto>.Fail($"Failed to upsert guest preferences: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteGuestPreferencesAsync(int guestId)
        {
            try
            {
                var preferences = await _unitOfWork.GuestPreferences
                    .GetAll(p => p.GuestId == guestId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (preferences == null)
                {
                    return ApiResponse<bool>.Fail("Guest preferences not found");
                }

                preferences.IsDeleted = true;
                _unitOfWork.GuestPreferences.Update(preferences);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Guest preferences deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete guest preferences for guest {GuestId}", guestId);
                return ApiResponse<bool>.Fail($"Failed to delete guest preferences: {ex.Message}");
            }
        }

        private GuestPreferencesDto MapToDto(GuestPreferencesEntity entity)
        {
            return new GuestPreferencesDto
            {
                Id = entity.Id,
                GuestId = entity.GuestId,
                PreferredRoomType = entity.PreferredRoomType,
                RoomSpecialRequests = entity.RoomSpecialRequests,
                BedPreference = entity.BedPreference,
                SmokingPreference = entity.SmokingPreference,
                DietaryPreferences = entity.DietaryPreferences,
                FoodAllergies = entity.FoodAllergies,
                SpecialFoodRequests = entity.SpecialFoodRequests,
                ActivityPreferences = entity.ActivityPreferences,
                Interests = entity.Interests,
                PrefersEmail = entity.PrefersEmail,
                PrefersSMS = entity.PrefersSMS,
                PrefersWhatsApp = entity.PrefersWhatsApp,
                PrefersPhone = entity.PrefersPhone,
                PreferredLanguage = entity.PreferredLanguage,
                Notes = entity.Notes,
                Source = entity.Source
            };
        }
    }
}
