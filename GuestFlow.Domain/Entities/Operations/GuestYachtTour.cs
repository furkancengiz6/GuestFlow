using GuestFlow.Domain.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Operations
{
    public class GuestYachtTour:BaseEntity
    {
        public int GuestId { get; set; }
        public virtual GuestEntity Guest { get; set; }
        public int YachtTourId { get; set; }
        public virtual YachtTourEntity YachtTour { get; set; }
    }
}
