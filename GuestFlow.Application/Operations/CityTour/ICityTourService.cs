using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour
{
    public interface ICityTourService
    {
        Task<ServiceMessage> AddCityTour(AddCityTourDto cityTour);
        Task<ServiceMessage> UpdateCityTour(UpdateCityTourDto cityTour);
        Task<ServiceMessage> DeleteCityTour(int id);
        Task<GetCityTourDto> GetCityTourById(int id);
        Task<List<GetCityTourDto>> GetCityTours();
    }
}