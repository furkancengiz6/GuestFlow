using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Reservation.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Reservation
{
    /// <summary>
    /// Rezervasyon servisi interface'i
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Yeni rezervasyon oluşturur
        /// </summary>
        Task<ServiceMessage> CreateReservationAsync(AddReservationDto reservation);

        /// <summary>
        /// Rezervasyonu onaylar
        /// </summary>
        Task<ServiceMessage> ConfirmReservationAsync(int reservationId);

        /// <summary>
        /// Rezervasyonu iptal eder
        /// </summary>
        Task<ServiceMessage> CancelReservationAsync(int reservationId, string? cancellationReason = null);

        /// <summary>
        /// Rezervasyonu günceller
        /// </summary>
        Task<ServiceMessage> UpdateReservationAsync(UpdateReservationDto reservation);

        /// <summary>
        /// Rezervasyonu ID'ye göre getirir
        /// </summary>
        Task<GetReservationDto?> GetReservationByIdAsync(int id);

        /// <summary>
        /// Rezervasyon detayını getirir
        /// </summary>
        Task<ReservationDetailDto?> GetReservationDetailAsync(int id);

        /// <summary>
        /// Tüm rezervasyonları getirir
        /// </summary>
        Task<List<GetReservationDto>> GetReservationsAsync();

        /// <summary>
        /// Sayfalanmış rezervasyonları getirir
        /// </summary>
        Task<PagedResult<GetReservationDto>> GetReservationsPagedAsync(
            int pageNumber, 
            int pageSize, 
            ReservationFilterParameters? filters = null, 
            SortingParameters? sorting = null);

        /// <summary>
        /// Misafire ait rezervasyonları getirir
        /// </summary>
        Task<List<GetReservationDto>> GetReservationsByGuestIdAsync(int guestId);

        /// <summary>
        /// Personel'e ait rezervasyonları getirir
        /// </summary>
        Task<List<GetReservationDto>> GetReservationsByPersonnelIdAsync(int personnelId);

        /// <summary>
        /// Tarih aralığına göre rezervasyonları getirir
        /// </summary>
        Task<List<GetReservationDto>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Duruma göre rezervasyonları getirir
        /// </summary>
        Task<List<GetReservationDto>> GetReservationsByStatusAsync(string status);
    }
}

