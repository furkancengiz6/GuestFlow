using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Transfer
{
    public interface ITransferService
    {
        Task<ServiceMessage> AddTransfer(AddTransferDto transfer);
        Task<ServiceMessage> UpdateTransfer(UpdateTransferDto transfer);
        Task<ServiceMessage> DeleteTransfer(int id);
        Task<GetTransferDto> GetTransferById(int id);
        Task<List<GetTransferDto>> GetTransfers();
    }
}