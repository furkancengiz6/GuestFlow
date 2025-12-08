using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.YachtTour
{
    public interface IYachtTourService
    {
        Task<ServiceMessage> AddYachtTour(AddYachtTourDto yachtTour);
        Task<ServiceMessage> UpdateYachtTour(UpdateYachtTourDto yachtTour);
        Task<ServiceMessage> DeleteYachtTour(int id);
        Task<GetYachtTourDto> GetYachtTourById(int id);
        Task<List<GetYachtTourDto>> GetYachtTours();
        
        /// <summary>
        /// Yat turu detayını getirir (ilgili veriler ile)
        /// </summary>
        Task<YachtTourDetailDto> GetYachtTourDetailAsync(int id);
        
        /// <summary>
        /// Sayfalanmış yat turlarını getirir
        /// </summary>
        Task<PagedResult<GetYachtTourDto>> GetYachtToursPaged(int pageNumber, int pageSize, YachtTourFilterParameters? filters = null, SortingParameters? sorting = null);
    }
}