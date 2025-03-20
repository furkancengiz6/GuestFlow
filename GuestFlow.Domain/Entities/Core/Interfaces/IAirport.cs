using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface IAirport
    {
       string Name { get; set; }
     string Code { get; set; }
    }
}
