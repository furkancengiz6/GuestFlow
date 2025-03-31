using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.UnitOfWork
{
   public  interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();//Kaç kayda etki ettiğini geriye döner , oyüzden int.

        Task BeginTransactionAsync();
        //Task asenkron metotların voidi gibi düşünülebilir

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();



    }
}
