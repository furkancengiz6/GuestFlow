using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.City.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.City
{
   public interface ICityService
    {
        Task<ServiceMessage> AddCity(AddCityDto city);
        Task<ServiceMessage> UpdateCity(UpdateCityDto city);
        Task<ServiceMessage> DeleteCity(int id);
        Task<GetCityDto> GetCityById(int id);
        Task<List<GetCityDto>> GetCities();
        
        /// <summary>
        /// Sayfalanmış şehirleri getirir
        /// </summary>
        Task<PagedResult<GetCityDto>> GetCitiesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
    }
}
