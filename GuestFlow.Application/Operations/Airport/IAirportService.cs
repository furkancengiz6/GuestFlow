using GuestFlow.Application.Operations.Airport.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Airport
{
    public interface IAirportService
    {
        Task<ServiceMessage> AddAirport(AddAirportDto airport);
        Task<ServiceMessage> UpdateAirport(UpdateAirportDto airport);
        Task<ServiceMessage> DeleteAirport(int id);
        Task<GetAirportDto> GetAirportById(int id);
        Task<List<GetAirportDto>> GetAirports();
    }
}