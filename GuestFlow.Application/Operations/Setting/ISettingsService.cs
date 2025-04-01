using GuestFlow.Application.Types;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Setting
{
    public interface ISettingsService
    {
        Task<ServiceMessage> ToggleMaintenence();
        Task<bool> GetMaintenanceState();
    }
}