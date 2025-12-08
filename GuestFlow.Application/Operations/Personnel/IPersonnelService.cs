using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Personnel.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Personnel
{
    public interface IPersonnelService
    {
        Task<ServiceMessage> AddPersonnel(AddPersonnelDto addPersonnelDto);
        Task<ServiceMessage<PersonnelInfoDto>> Login(LoginPersonnelDto login);
        Task<ServiceMessage<PersonnelInfoDto>> GetPersonnelById(int id);
        Task<ServiceMessage<List<PersonnelInfoDto>>> GetAllPersonnel();
        Task<ServiceMessage> UpdatePersonnel(UpdatePersonnelDto updatePersonnelDto);
        Task<ServiceMessage> DeletePersonnel(int id);
        Task<ServiceMessage<string>> RequestPasswordReset(string email);
        Task<ServiceMessage> ResetPassword(string token, string newPassword);
        Task<ServiceMessage> ChangePassword(int personnelId, string currentPassword, string newPassword);
        
        /// <summary>
        /// Personel detayını getirir (ilgili veriler ile)
        /// </summary>
        Task<PersonnelDetailDto> GetPersonnelDetailAsync(int id);
        
        /// <summary>
        /// Sayfalanmış, filtrelenmiş ve sıralanmış personelleri getirir
        /// </summary>
        Task<PagedResult<PersonnelInfoDto>> GetPersonnelPagedAsync(int pageNumber, int pageSize, PersonnelFilterParameters? filters = null, SortingParameters? sorting = null);
        
        /// <summary>
        /// Personel aktivite günlüklerini getirir
        /// </summary>
        Task<List<PersonnelActivityDto>> GetPersonnelActivitiesAsync(int id, int? limit = 20);
    }
}
