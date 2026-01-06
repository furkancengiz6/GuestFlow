using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    public interface IRoomAssignmentService
    {
        /// <summary>
        /// Create a new room assignment for a guest
        /// </summary>
        Task<ServiceMessage<RoomAssignmentDto>> CreateRoomAssignmentAsync(CreateRoomAssignmentDto request);

        /// <summary>
        /// Update an existing room assignment
        /// </summary>
        Task<ServiceMessage<RoomAssignmentDto>> UpdateRoomAssignmentAsync(UpdateRoomAssignmentDto request);

        /// <summary>
        /// Close an active room assignment (set EndDate)
        /// </summary>
        Task<ServiceMessage<bool>> CloseRoomAssignmentAsync(int assignmentId, CloseRoomAssignmentDto request);

        /// <summary>
        /// Get room assignments for a guest
        /// </summary>
        Task<ServiceMessage<List<RoomAssignmentDto>>> GetGuestRoomAssignmentsAsync(int guestId);

        /// <summary>
        /// Get current room assignment for a guest (if any)
        /// </summary>
        Task<ServiceMessage<RoomAssignmentDto>> GetCurrentRoomAssignmentAsync(int guestId);

        /// <summary>
        /// Get room context by room number and date range
        /// </summary>
        Task<ServiceMessage<RoomContextDto>> GetRoomContextAsync(RoomContextRequestDto request);
    }
}
