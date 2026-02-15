using AutoMapper;
using GuestFlow.Application.Operations.DailyNote.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using GuestFlow.Domain.Events;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyNote
{
    public class DailyNoteManager : IDailyNoteService
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<DailyNoteEntity> _dailyNoteRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly ILogger<DailyNoteManager> _logger;
        private readonly IMapper _mapper;

     
        public DailyNoteManager(
            IUnitOfWork unitOfWork,
            IRepository<DailyNoteEntity> dailyNoteRepository,
            IRepository<PersonnelEntity> personnelRepository,
            ILogger<DailyNoteManager> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dailyNoteRepository = dailyNoteRepository;
            _personnelRepository = personnelRepository;
            _logger = logger;
        }

        // Bu metodumla yeni bir günlük not ekliyorum.
        public async Task<ServiceMessage> AddDailyNote(AddDailyNoteDto dailyNote)
        {
            try
            {
                // Veritabanında bir işlem başlatıyorum.
                await _unitOfWork.BeginTransactionAsync();

                // Eğer bir personel ID'si verilmişse, bu personelin var olup olmadığını kontrol ediyorum.
                if (dailyNote.PersonnelId.HasValue)
                {
                    var personnelExists = await _personnelRepository.GetAll(x => x.Id == dailyNote.PersonnelId.Value).AnyAsync();
                    if (!personnelExists)
                        return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };
                }

                // Yeni bir günlük not nesnesi oluşturuyorum ve DTO'dan gelen bilgileri buraya aktarıyorum.
                var dailyNoteEntity = new DailyNoteEntity
                {
                    NoteDate = dailyNote.NoteDate,
                    RoomNumber = dailyNote.RoomNumber,
                    NoteText = dailyNote.NoteText,
                    PersonnelId = dailyNote.PersonnelId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                // Trigger Domain Event
                dailyNoteEntity.AddDomainEvent(new DailyNoteCreatedEvent(dailyNoteEntity));

                // Yeni günlük notu veritabanına ekliyorum.
                await _dailyNoteRepository.AddAsync(dailyNoteEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Günlük not eklendi: {dailyNoteEntity.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Günlük not başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Günlük not eklenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Günlük not eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla mevcut bir günlük notu güncelliyorum.
        public async Task<ServiceMessage> UpdateDailyNote(UpdateDailyNoteDto dailyNote)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Güncellenecek günlük notu ID'sine göre veritabanından çekiyorum.
                var existing = await _dailyNoteRepository.GetAsync(x => x.Id == dailyNote.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Günlük not bulunamadı." };

                // Eğer bir personel ID'si verilmişse, bu personelin var olup olmadığını kontrol ediyorum.
                if (dailyNote.PersonnelId.HasValue)
                {
                    var personnelExists = await _personnelRepository.GetAll(x => x.Id == dailyNote.PersonnelId.Value).AnyAsync();
                    if (!personnelExists)
                        return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };
                }

                // Güncel bilgileri mevcut kayda aktarıyorum.
                existing.NoteDate = dailyNote.NoteDate;
                existing.RoomNumber = dailyNote.RoomNumber;
                existing.NoteText = dailyNote.NoteText;
                existing.PersonnelId = dailyNote.PersonnelId;

                await _dailyNoteRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Günlük not güncellendi: {dailyNote.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Günlük not başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Günlük not güncellenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Günlük not güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla bir günlük notu siliyorum.
        public async Task<ServiceMessage> DeleteDailyNote(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Silinecek günlük notu ID'sine göre kontrol ediyorum.
                var dailyNote = await _dailyNoteRepository.GetAsync(x => x.Id == id);
                if (dailyNote == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Günlük not bulunamadı." };

                await _dailyNoteRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Günlük not silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Günlük not başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Günlük not silinirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Günlük not silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla belirli bir günlük notu ID'sine göre getiriyorum.
        public async Task<GetDailyNoteDto> GetDailyNoteById(int id)
        {
            try
            {
                var dailyNote = await _dailyNoteRepository.GetByIdAsync(id);
                if (dailyNote == null)
                    throw new Exception("Günlük not bulunamadı.");

                return _mapper.Map<GetDailyNoteDto>(dailyNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük not getirilirken hata çıktı: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metodumla tüm günlük notları getiriyorum.
        public async Task<List<GetDailyNoteDto>> GetDailyNotes()
        {
            try
            {
                var dailyNotes = await _dailyNoteRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetDailyNoteDto>>(dailyNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük notlar listelenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}