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
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.GuestEntity> Guests { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.GuestPreferencesEntity> GuestPreferences { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.ReservationEntity> Reservations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.TransferEntity> Transfers { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.CityTourEntity> CityTours { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.YachtTourEntity> YachtTours { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.RestaurantReservationEntity> RestaurantReservations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.Supplier> Suppliers { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAIntegration> OTAIntegrations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAPriceUpdate> OTAPriceUpdates { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAReservation> OTAReservations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.OTAWebhookLog> OTAWebhookLogs { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.PMSIntegration> PMSIntegrations { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.PMSSyncHistory> PMSSyncHistories { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.PMSGuestMapping> PMSGuestMappings { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.PMSReservationMapping> PMSReservationMappings { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.InvoicesEntity> Invoices { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.InvoiceItemEntity> InvoiceItems { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.JournalEntry> JournalEntries { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.JournalLine> JournalLines { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Operations.SupplierCost> SupplierCosts { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.EmailHistoryEntity> EmailHistories { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.SmsHistoryEntity> SmsHistories { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.WhatsAppHistoryEntity> WhatsAppHistories { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.NotificationEntity> Notifications { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.NotificationRuleEntity> NotificationRules { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.LoginAttemptEntity> LoginAttempts { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.PrivacyActionHistoryEntity> PrivacyActionHistories { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.FeatureFlagEntity> FeatureFlags { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.PermissionEntity> Permissions { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.RolePermissionEntity> RolePermissions { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Core.PersonnelEntity> Personnels { get; }
        
        // Intelligence Layer - Behavioral Data Collection
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Intelligence.GuestBehaviorEntity> GuestBehaviors { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Intelligence.StaffBehaviorEntity> StaffBehaviors { get; }
        GuestFlow.Domain.Entities.Repositories.IRepository<GuestFlow.Domain.Entities.Intelligence.GuestStaffInteractionEntity> GuestStaffInteractions { get; }
        
        // Convenience commit used throughout app
        Task CommitAsync();
    }
}
