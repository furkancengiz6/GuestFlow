using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Transfer
{
    public interface ITransferService
    {
        Task<ServiceMessage<AddTransferResponseDto>> AddTransfer(AddTransferDto transfer);
        Task<ServiceMessage> UpdateTransfer(UpdateTransferDto transfer);
        Task<ServiceMessage> DeleteTransfer(int id);
        Task<GetTransferDto> GetTransferById(int id);
        Task<List<GetTransferDto>> GetTransfers();
        Task<PagedResult<GetTransferDto>> GetTransfersPaged(int pageNumber, int pageSize, TransferFilterParameters? filters = null, SortingParameters? sorting = null);
        
        /// <summary>
        /// Transfer detayını getirir (ilgili veriler ile)
        /// </summary>
        Task<TransferDetailDto> GetTransferDetailAsync(int id);
        
        /// <summary>
        /// Transfer takvim görünümünü getirir
        /// </summary>
        Task<TransferCalendarDto> GetTransferCalendarAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Transfer istatistiklerini getirir
        /// </summary>
        Task<TransferStatisticsDto> GetTransferStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Transfer durumunu günceller (iş akışı için)
        /// </summary>
        Task<ServiceMessage> UpdateTransferStatusAsync(int id, string status);
        
        /// <summary>
        /// Transfer'e araç atar
        /// </summary>
        Task<ServiceMessage> AssignVehicleAsync(int id, int vehicleId);

        /// <summary>
        /// Transfer için fatura oluşturur
        /// </summary>
        Task<ServiceMessage> CreateTransferInvoiceAsync(int id);

        /// <summary>
        /// Transfer onay maili gönderir
        /// </summary>
        Task<ServiceMessage> SendTransferConfirmationAsync(int id);

        /// <summary>
        /// Transfer için gidiş-dönüş transferi oluşturur
        /// </summary>
        Task<ServiceMessage> CreateRoundTripTransferAsync(int id);
    }
}
