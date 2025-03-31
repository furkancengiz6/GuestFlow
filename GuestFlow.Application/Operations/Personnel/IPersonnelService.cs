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
        Task<ServiceMessage> DeletePersonnel(int id);
    }
}
