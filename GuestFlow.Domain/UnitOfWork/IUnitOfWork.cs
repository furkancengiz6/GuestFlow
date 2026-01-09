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
        //
        // Repository properties used across the application
        //
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.TransferEntity> Transfers { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.CityTourEntity> CityTours { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.YachtTourEntity> YachtTours { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.RestaurantReservationEntity> RestaurantReservations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.Supplier> Suppliers { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAIntegration> OTAIntegrations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAPriceUpdate> OTAPriceUpdates { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAReservation> OTAReservations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.InvoicesEntity> Invoices { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.InvoiceItemEntity> InvoiceItems { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.JournalEntry> JournalEntries { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.SupplierCost> SupplierCosts { get; }
        // Convenience commit used throughout app
        Task CommitAsync();
    }
}
