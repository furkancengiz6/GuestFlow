using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Application.Types;
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
    }
}