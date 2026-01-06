using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour
{
    public interface ICityTourService
    {
        Task<ServiceMessage<AddCityTourResponseDto>> AddCityTour(AddCityTourDto cityTour);
        Task<ServiceMessage> UpdateCityTour(UpdateCityTourDto cityTour);
        Task<ServiceMessage> DeleteCityTour(int id);
        Task<GetCityTourDto> GetCityTourById(int id);
        Task<List<GetCityTourDto>> GetCityTours();
        
        /// <summary>
        /// Şehir turu detayını getirir (ilgili veriler ile)
        /// </summary>
        Task<CityTourDetailDto> GetCityTourDetailAsync(int id);
        
        /// <summary>
        /// Sayfalanmış şehir turlarını getirir
        /// </summary>
        Task<PagedResult<GetCityTourDto>> GetCityToursPaged(int pageNumber, int pageSize, CityTourFilterParameters? filters = null, SortingParameters? sorting = null);

        /// <summary>
        /// Şehir turu için fatura oluşturur
        /// </summary>
        Task<ServiceMessage> CreateCityTourInvoiceAsync(int id);

        /// <summary>
        /// Şehir turu onay maili gönderir
        /// </summary>
        Task<ServiceMessage> SendCityTourConfirmationAsync(int id);

        /// <summary>
        /// Şehir turu durumunu günceller (tamamlandı/iptal edildi)
        /// </summary>
        Task<ServiceMessage> UpdateCityTourStatusAsync(int id, string status);
    }
}