using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Invoice
{
    public class InvoiceManager : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly ILogger<InvoiceManager> _logger;

        public InvoiceManager(
            IUnitOfWork unitOfWork,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<GuestEntity> guestRepository,
            ILogger<InvoiceManager> logger)
        {
            _unitOfWork = unitOfWork;
            _invoiceRepository = invoiceRepository;
            _guestRepository = guestRepository;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddInvoice(AddInvoiceDto invoice)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var guestExists = await _guestRepository.GetAll(x => x.Id == invoice.GuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                var invoiceEntity = new InvoicesEntity
                {
                    TotalAmount = invoice.TotalAmount,
                    IssueDate = invoice.IssueDate,
                    Notes = invoice.Notes,
                    GuestId = invoice.GuestId,
                    TransferId = invoice.TransferId,
                    CityTourId = invoice.CityTourId,
                    YachtTourId = invoice.YachtTourId
                };

                await _invoiceRepository.AddAsync(invoiceEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Fatura eklendi: {Id}", invoiceEntity.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Fatura başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Fatura eklenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Fatura eklenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> UpdateInvoice(UpdateInvoiceDto invoice)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _invoiceRepository.GetAsync(x => x.Id == invoice.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Fatura bulunamadı." };

                var guestExists = await _guestRepository.GetAll(x => x.Id == invoice.GuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                existing.TotalAmount = invoice.TotalAmount;
                existing.IssueDate = invoice.IssueDate;
                existing.Notes = invoice.Notes;
                existing.GuestId = invoice.GuestId;
                existing.TransferId = invoice.TransferId;
                existing.CityTourId = invoice.CityTourId;
                existing.YachtTourId = invoice.YachtTourId;

                await _invoiceRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Fatura güncellendi: {Id}", invoice.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Fatura başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Fatura güncellenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Fatura güncellenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> DeleteInvoice(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _invoiceRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Fatura silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Fatura başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Fatura silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Fatura silinirken hata: " + ex.Message };
            }
        }

        public async Task<GetInvoiceDto> GetInvoiceById(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(id);
                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                return new GetInvoiceDto
                {
                    Id = invoice.Id,
                    TotalAmount = invoice.TotalAmount,
                    IssueDate = invoice.IssueDate,
                    Notes = invoice.Notes,
                    GuestId = invoice.GuestId,
                    TransferId = invoice.TransferId,
                    CityTourId = invoice.CityTourId,
                    YachtTourId = invoice.YachtTourId,
                    CreatedDate = invoice.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura getirilirken hata oluştu: {Id}", id);
                throw;
            }
        }

        public async Task<List<GetInvoiceDto>> GetInvoices()
        {
            try
            {
                var invoices = await _invoiceRepository.GetAll()
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Notes = i.Notes,
                        GuestId = i.GuestId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Faturalar listelenirken hata oluştu.");
                throw;
            }
        }

        public async Task<List<GetInvoiceDto>> GetInvoicesByGuestId(int guestId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetAll(x => x.GuestId == guestId)
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Notes = i.Notes,
                        GuestId = i.GuestId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Misafire ait faturalar listelenirken hata oluştu: {GuestId}", guestId);
                throw;
            }
        }
    }
}