// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Concierge Dashboard servisi - PMS entegrasyonlu concierge operasyonları için
    /// </summary>
    public interface IConciergeDashboardService
    {
        /// <summary>
        /// Bugünkü check-in'leri getirir (PMS + GuestFlow birleşik)
        /// </summary>
        Task<ConciergeCheckInOutDto> GetTodayCheckInsAsync();

        /// <summary>
        /// Bugünkü check-out'ları getirir (PMS + GuestFlow birleşik)
        /// </summary>
        Task<ConciergeCheckInOutDto> GetTodayCheckOutsAsync();

        /// <summary>
        /// Aktif misafirleri getirir (PMS + GuestFlow birleşik)
        /// </summary>
        Task<List<ActiveGuestDto>> GetActiveGuestsAsync();

        /// <summary>
        /// Unified guest profile getirir (PMS + GuestFlow verileri birleşik)
        /// </summary>
        Task<UnifiedGuestProfileDto> GetUnifiedGuestProfileAsync(int guestId);

        /// <summary>
        /// Yaklaşan servisleri getirir (bugün ve yarın)
        /// </summary>
        Task<UpcomingServicesDto> GetUpcomingServicesForTodayAsync();

        /// <summary>
        /// Guest history dashboard getirir (önceki konaklamalar, hizmet geçmişi, harcama analizi)
        /// </summary>
        Task<GuestHistoryDashboardDto> GetGuestHistoryDashboardAsync(int guestId);

        /// <summary>
        /// Concierge dashboard özet getirir (tüm bilgileri birleştirir)
        /// </summary>
        Task<ConciergeDashboardSummaryDto> GetConciergeDashboardSummaryAsync();

        /// <summary>
        /// Misafir durumu göstergelerini getirir (VIP, özel istekler, doğum günü, vb.)
        /// </summary>
        Task<List<GuestStatusIndicatorDto>> GetGuestStatusIndicatorsAsync();
    }
}
