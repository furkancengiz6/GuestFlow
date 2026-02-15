using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Persistence.Repositories;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Domain.Entities.Finance;

namespace GuestFlow.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GuestFlowDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;
        // Repository instances
        private readonly IRepository<GuestEntity> _guests;
        private readonly IRepository<GuestPreferencesEntity> _guestPreferences;
        private readonly IRepository<ReservationEntity> _reservations;
        private readonly IRepository<TransferEntity> _transfers;
        private readonly IRepository<CityTourEntity> _cityTours;
        private readonly IRepository<YachtTourEntity> _yachtTours;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservations;
        private readonly IRepository<Supplier> _suppliers;
        private readonly IRepository<OTAIntegration> _otaIntegrations;
        private readonly IRepository<OTAHotelMapping> _otaHotelMappings;
        private readonly IRepository<OTAPriceUpdate> _otaPriceUpdates;
        private readonly IRepository<OTAReservation> _otaReservations;
        private readonly IRepository<OTAWebhookLog> _otaWebhookLogs;
        private readonly IRepository<InvoicesEntity> _invoices;
        private readonly IRepository<InvoiceItemEntity> _invoiceItems;
        private readonly IRepository<JournalEntry> _journalEntries;
        private readonly IRepository<JournalLine> _journalLines;
        private readonly IRepository<SupplierCost> _supplierCosts;
        private readonly IRepository<PMSIntegration> _pmsIntegrations;
        private readonly IRepository<PMSSyncHistory> _pmsSyncHistories;
        private readonly IRepository<PMSGuestMapping> _pmsGuestMappings;
        private readonly IRepository<PMSReservationMapping> _pmsReservationMappings;
        private readonly IRepository<EmailHistoryEntity> _emailHistories;
        private readonly IRepository<SmsHistoryEntity> _smsHistories;
        private readonly IRepository<WhatsAppHistoryEntity> _whatsAppHistories;
        private readonly IRepository<NotificationEntity> _notifications;
        private readonly IRepository<NotificationRuleEntity> _notificationRules;
        private readonly IRepository<LoginAttemptEntity> _loginAttempts;
        private readonly IRepository<PrivacyActionHistoryEntity> _privacyActionHistories;
        private readonly IRepository<FeatureFlagEntity> _featureFlags;
        private readonly IRepository<PermissionEntity> _permissions;
        private readonly IRepository<RolePermissionEntity> _rolePermissions;
        private readonly IRepository<PersonnelEntity> _personnels;
        private readonly IRepository<GuestBehaviorEntity> _guestBehaviors;
        private readonly IRepository<StaffBehaviorEntity> _staffBehaviors;
        private readonly IRepository<GuestStaffInteractionEntity> _guestStaffInteractions;
        private readonly IRepository<PricingRuleEntity> _pricingRules;
        private readonly IRepository<RoomAssignmentEntity> _roomAssignments;
        private readonly IRepository<GuestIntelligenceActionEntity> _guestIntelligenceActions;

        public UnitOfWork(GuestFlowDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            // instantiate repositories
            _guests = new Repository<GuestEntity>(_context);
            _guestPreferences = new Repository<GuestPreferencesEntity>(_context);
            _reservations = new Repository<ReservationEntity>(_context);
            _transfers = new Repository<TransferEntity>(_context);
            _cityTours = new Repository<CityTourEntity>(_context);
            _yachtTours = new Repository<YachtTourEntity>(_context);
            _restaurantReservations = new Repository<RestaurantReservationEntity>(_context);
            _suppliers = new Repository<Supplier>(_context);
            _otaIntegrations = new Repository<OTAIntegration>(_context);
            _otaHotelMappings = new Repository<OTAHotelMapping>(_context);
            _otaPriceUpdates = new Repository<OTAPriceUpdate>(_context);
            _otaReservations = new Repository<OTAReservation>(_context);
            _otaWebhookLogs = new Repository<OTAWebhookLog>(_context);
            _invoices = new Repository<InvoicesEntity>(_context);
            _invoiceItems = new Repository<InvoiceItemEntity>(_context);
            _journalEntries = new Repository<JournalEntry>(_context);
            _journalLines = new Repository<JournalLine>(_context);
            _supplierCosts = new Repository<SupplierCost>(_context);
            _pmsIntegrations = new Repository<PMSIntegration>(_context);
            _pmsSyncHistories = new Repository<PMSSyncHistory>(_context);
            _pmsGuestMappings = new Repository<PMSGuestMapping>(_context);
            _pmsReservationMappings = new Repository<PMSReservationMapping>(_context);
            _emailHistories = new Repository<EmailHistoryEntity>(_context);
            _smsHistories = new Repository<SmsHistoryEntity>(_context);
            _whatsAppHistories = new Repository<WhatsAppHistoryEntity>(_context);
            _notifications = new Repository<NotificationEntity>(_context);
            _notificationRules = new Repository<NotificationRuleEntity>(_context);
            _loginAttempts = new Repository<LoginAttemptEntity>(_context);
            _privacyActionHistories = new Repository<PrivacyActionHistoryEntity>(_context);
            _featureFlags = new Repository<FeatureFlagEntity>(_context);
            _permissions = new Repository<PermissionEntity>(_context);
            _rolePermissions = new Repository<RolePermissionEntity>(_context);
            _personnels = new Repository<PersonnelEntity>(_context);
            _guestBehaviors = new Repository<GuestBehaviorEntity>(_context);
            _staffBehaviors = new Repository<StaffBehaviorEntity>(_context);
            _guestStaffInteractions = new Repository<GuestStaffInteractionEntity>(_context);
            _pricingRules = new Repository<PricingRuleEntity>(_context);
            _roomAssignments = new Repository<RoomAssignmentEntity>(_context);
            _guestIntelligenceActions = new Repository<GuestIntelligenceActionEntity>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("Bir işlem zaten başlatılmış.");
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _transaction?.Dispose();
                _context?.Dispose();
            }

            _disposed = true;
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        public async Task CommitAsync()
        {
            await SaveChangesAsync();
        }
        // Expose repositories
        public IRepository<GuestEntity> Guests => _guests;
        public IRepository<GuestPreferencesEntity> GuestPreferences => _guestPreferences;
        public IRepository<ReservationEntity> Reservations => _reservations;
        public IRepository<TransferEntity> Transfers => _transfers;
        public IRepository<CityTourEntity> CityTours => _cityTours;
        public IRepository<YachtTourEntity> YachtTours => _yachtTours;
        public IRepository<RestaurantReservationEntity> RestaurantReservations => _restaurantReservations;
        public IRepository<Supplier> Suppliers => _suppliers;
        public IRepository<OTAIntegration> OTAIntegrations => _otaIntegrations;
        public IRepository<OTAHotelMapping> OTAHotelMappings => _otaHotelMappings;
        public IRepository<OTAPriceUpdate> OTAPriceUpdates => _otaPriceUpdates;
        public IRepository<OTAReservation> OTAReservations => _otaReservations;
        public IRepository<OTAWebhookLog> OTAWebhookLogs => _otaWebhookLogs;
        public IRepository<InvoicesEntity> Invoices => _invoices;
        public IRepository<InvoiceItemEntity> InvoiceItems => _invoiceItems;
        public IRepository<JournalEntry> JournalEntries => _journalEntries;
        public IRepository<JournalLine> JournalLines => _journalLines;
        public IRepository<SupplierCost> SupplierCosts => _supplierCosts;
        public IRepository<PMSIntegration> PMSIntegrations => _pmsIntegrations;
        public IRepository<PMSSyncHistory> PMSSyncHistories => _pmsSyncHistories;
        public IRepository<PMSGuestMapping> PMSGuestMappings => _pmsGuestMappings;
        public IRepository<PMSReservationMapping> PMSReservationMappings => _pmsReservationMappings;
        public IRepository<EmailHistoryEntity> EmailHistories => _emailHistories;
        public IRepository<SmsHistoryEntity> SmsHistories => _smsHistories;
        public IRepository<WhatsAppHistoryEntity> WhatsAppHistories => _whatsAppHistories;
        public IRepository<NotificationEntity> Notifications => _notifications;
        public IRepository<NotificationRuleEntity> NotificationRules => _notificationRules;
        public IRepository<LoginAttemptEntity> LoginAttempts => _loginAttempts;
        public IRepository<PrivacyActionHistoryEntity> PrivacyActionHistories => _privacyActionHistories;
        public IRepository<FeatureFlagEntity> FeatureFlags => _featureFlags;
        public IRepository<PermissionEntity> Permissions => _permissions;
        public IRepository<RolePermissionEntity> RolePermissions => _rolePermissions;
        public IRepository<PersonnelEntity> Personnels => _personnels;
        public IRepository<GuestBehaviorEntity> GuestBehaviors => _guestBehaviors;
        public IRepository<StaffBehaviorEntity> StaffBehaviors => _staffBehaviors;
        public IRepository<GuestStaffInteractionEntity> GuestStaffInteractions => _guestStaffInteractions;
        public IRepository<PricingRuleEntity> PricingRules => _pricingRules;
        public IRepository<RoomAssignmentEntity> RoomAssignments => _roomAssignments;
        public IRepository<GuestIntelligenceActionEntity> GuestIntelligenceActions => _guestIntelligenceActions;
    }
}