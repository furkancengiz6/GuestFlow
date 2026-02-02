using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Domain.Entities.Finance;
using GuestFlow.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Persistence.MultiTenancy;
using System.Linq.Expressions;
using System.Reflection;

namespace GuestFlow.Persistence.Context
{
    public class GuestFlowDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public GuestFlowDbContext(DbContextOptions<GuestFlowDbContext> options, ITenantProvider tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<AirportEntity> Airports => Set<AirportEntity>();
        public DbSet<PricingRuleEntity> PricingRules => Set<PricingRuleEntity>();
        public DbSet<PersonnelEntity> Personnels => Set<PersonnelEntity>();
        public DbSet<CityEntity> Cities => Set<CityEntity>();
        public DbSet<TourEntity> Tours => Set<TourEntity>();
        public DbSet<CityTourEntity> CityTours => Set<CityTourEntity>();
        public DbSet<DailyNoteEntity> DailyNotes => Set<DailyNoteEntity>();
        public DbSet<DailyRevenueEntity> DailyRevenues => Set<DailyRevenueEntity>();
        public DbSet<GuestEntity> Guests => Set<GuestEntity>();
        public DbSet<GuestPreferencesEntity> GuestPreferences => Set<GuestPreferencesEntity>();
        public DbSet<RoomAssignmentEntity> RoomAssignments => Set<RoomAssignmentEntity>();
        public DbSet<InvoicesEntity> Invoices => Set<InvoicesEntity>();
        public DbSet<InvoiceItemEntity> InvoiceItems => Set<InvoiceItemEntity>();
        public DbSet<TransferEntity> Transfers => Set<TransferEntity>();
        public DbSet<VehicleEntity> Vehicles => Set<VehicleEntity>();
        public DbSet<YachtTourEntity> YachtTours => Set<YachtTourEntity>();
        public DbSet<GuestYachtTour> GuestYachtTours => Set<GuestYachtTour>();
        public DbSet<GuestCityTour> GuestCityTours => Set<GuestCityTour>();
        public DbSet<SettingEntity> Settings {  get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
        public DbSet<LoginAttemptEntity> LoginAttempts => Set<LoginAttemptEntity>();
        public DbSet<PrivacyActionHistoryEntity> PrivacyActionHistories => Set<PrivacyActionHistoryEntity>();
        public DbSet<FeatureFlagEntity> FeatureFlags => Set<FeatureFlagEntity>();
        public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
        public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();
        public DbSet<EmailQueueEntity> EmailQueues => Set<EmailQueueEntity>();
        public DbSet<EmailTemplateEntity> EmailTemplates => Set<EmailTemplateEntity>();
        public DbSet<EmailHistoryEntity> EmailHistories => Set<EmailHistoryEntity>();
        public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();
        public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
        public DbSet<SmsHistoryEntity> SmsHistories => Set<SmsHistoryEntity>();
        public DbSet<WhatsAppHistoryEntity> WhatsAppHistories => Set<WhatsAppHistoryEntity>();
        public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();
        public DbSet<NotificationRuleEntity> NotificationRules => Set<NotificationRuleEntity>();
        public DbSet<HotelEntity> Hotels => Set<HotelEntity>();
        public DbSet<RestaurantEntity> Restaurants => Set<RestaurantEntity>();
        public DbSet<ItineraryEntity> Itineraries => Set<ItineraryEntity>();
        public DbSet<ItineraryItemEntity> ItineraryItems => Set<ItineraryItemEntity>();
        public DbSet<RestaurantReservationEntity> RestaurantReservations => Set<RestaurantReservationEntity>();
        public DbSet<ServicePackageEntity> ServicePackages => Set<ServicePackageEntity>();
        public DbSet<PackageTransferEntity> PackageTransfers => Set<PackageTransferEntity>();
        public DbSet<PackageCityTourEntity> PackageCityTours => Set<PackageCityTourEntity>();
        public DbSet<PackageYachtTourEntity> PackageYachtTours => Set<PackageYachtTourEntity>();
        public DbSet<PackageRestaurantReservationEntity> PackageRestaurantReservations => Set<PackageRestaurantReservationEntity>();

        // Audit logging
        public DbSet<GuestFlow.Domain.Entities.Core.AuditLog> AuditLogs => Set<GuestFlow.Domain.Entities.Core.AuditLog>();

        // Supplier management
        public DbSet<GuestFlow.Domain.Entities.Core.Supplier> Suppliers => Set<GuestFlow.Domain.Entities.Core.Supplier>();
        public DbSet<GuestFlow.Domain.Entities.Operations.SupplierCost> SupplierCosts => Set<GuestFlow.Domain.Entities.Operations.SupplierCost>();

        // OTA integrations
        public DbSet<GuestFlow.Domain.Entities.Operations.OTAIntegration> OTAIntegrations => Set<GuestFlow.Domain.Entities.Operations.OTAIntegration>();
        public DbSet<GuestFlow.Domain.Entities.Operations.OTAHotelMapping> OTAHotelMappings => Set<GuestFlow.Domain.Entities.Operations.OTAHotelMapping>();
        public DbSet<GuestFlow.Domain.Entities.Operations.OTAReservation> OTAReservations => Set<GuestFlow.Domain.Entities.Operations.OTAReservation>();
        public DbSet<GuestFlow.Domain.Entities.Operations.OTAPriceUpdate> OTAPriceUpdates => Set<GuestFlow.Domain.Entities.Operations.OTAPriceUpdate>();
        public DbSet<GuestFlow.Domain.Entities.Operations.OTAWebhookLog> OTAWebhookLogs => Set<GuestFlow.Domain.Entities.Operations.OTAWebhookLog>();

        // PMS integrations
        public DbSet<GuestFlow.Domain.Entities.Operations.PMSIntegration> PMSIntegrations => Set<GuestFlow.Domain.Entities.Operations.PMSIntegration>();
        public DbSet<GuestFlow.Domain.Entities.Operations.PMSSyncHistory> PMSSyncHistories => Set<GuestFlow.Domain.Entities.Operations.PMSSyncHistory>();
        public DbSet<GuestFlow.Domain.Entities.Operations.PMSGuestMapping> PMSGuestMappings => Set<GuestFlow.Domain.Entities.Operations.PMSGuestMapping>();
        public DbSet<GuestFlow.Domain.Entities.Operations.PMSReservationMapping> PMSReservationMappings => Set<GuestFlow.Domain.Entities.Operations.PMSReservationMapping>();
        // Accounting - journal entries
        public DbSet<GuestFlow.Domain.Entities.Core.JournalEntry> JournalEntries => Set<GuestFlow.Domain.Entities.Core.JournalEntry>();
        public DbSet<GuestFlow.Domain.Entities.Core.JournalLine> JournalLines => Set<GuestFlow.Domain.Entities.Core.JournalLine>();
        public DbSet<GuestReview> GuestReviews => Set<GuestReview>();

        // Intelligence Layer - Behavioral Data Collection
        public DbSet<GuestBehaviorEntity> GuestBehaviors => Set<GuestBehaviorEntity>();
        public DbSet<StaffBehaviorEntity> StaffBehaviors => Set<StaffBehaviorEntity>();
        public DbSet<GuestStaffInteractionEntity> GuestStaffInteractions => Set<GuestStaffInteractionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Accounting - DB idempotency: 1 invoice -> max 1 journal entry
            // (InvoiceId is nullable for backward compatibility; multiple NULLs allowed.)
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => e.InvoiceId)
                .IsUnique()
                .HasFilter("[InvoiceId] IS NOT NULL");

            // Journal Entry and Line configurations
            modelBuilder.ApplyConfiguration(new JournalEntryConfiguration());
            modelBuilder.ApplyConfiguration(new JournalLineConfiguration());

            // Fluent API ile yapılandırmaları uyguluyoruz
            modelBuilder.ApplyConfiguration(new AirportConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
            modelBuilder.ApplyConfiguration(new TourConfiguration());
            modelBuilder.ApplyConfiguration(new CityTourConfiguration());
            modelBuilder.ApplyConfiguration(new DailyNoteConfiguration());
            modelBuilder.ApplyConfiguration(new DailyRevenueConfiguration());
            modelBuilder.ApplyConfiguration(new GuestConfiguration());
            modelBuilder.ApplyConfiguration(new GuestPreferencesConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationRuleConfiguration());
            modelBuilder.ApplyConfiguration(new InvoicesConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());
            modelBuilder.ApplyConfiguration(new PersonnelConfiguration());
            modelBuilder.ApplyConfiguration(new TransferConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleConfiguration());
            modelBuilder.ApplyConfiguration(new YachtTourConfiguration());
            modelBuilder.ApplyConfiguration(new GuestYachtTourConfiguration());
            modelBuilder.ApplyConfiguration(new GuestCityTourConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new EmailQueueConfiguration());
            modelBuilder.ApplyConfiguration(new EmailTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new EmailHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new ReservationConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new SmsHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new WhatsAppHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new HotelConfiguration());
            modelBuilder.ApplyConfiguration(new RestaurantConfiguration());
            modelBuilder.ApplyConfiguration(new ItineraryConfiguration());
            modelBuilder.ApplyConfiguration(new ItineraryItemConfiguration());
            modelBuilder.ApplyConfiguration(new RestaurantReservationConfiguration());
            modelBuilder.ApplyConfiguration(new ServicePackageConfiguration());
            modelBuilder.ApplyConfiguration(new PackageTransferConfiguration());
            modelBuilder.ApplyConfiguration(new PackageCityTourConfiguration());
            modelBuilder.ApplyConfiguration(new PackageYachtTourConfiguration());
            modelBuilder.ApplyConfiguration(new PackageRestaurantReservationConfiguration());

            // Intelligence Layer - Behavioral Data Collection
            modelBuilder.ApplyConfiguration(new GuestBehaviorConfiguration());
            modelBuilder.ApplyConfiguration(new StaffBehaviorConfiguration());
            modelBuilder.ApplyConfiguration(new GuestStaffInteractionConfiguration());
            
            // Finance - Pricing Rules
            modelBuilder.ApplyConfiguration(new PricingRuleConfiguration());

            // OTA Webhook Log configuration
            modelBuilder.ApplyConfiguration(new GuestFlow.Domain.Entities.Operations.OTAWebhookLogConfiguration());

            // Login Attempt configuration
            modelBuilder.ApplyConfiguration(new LoginAttemptConfiguration());

            // Privacy Action History configuration
            modelBuilder.ApplyConfiguration(new PrivacyActionHistoryConfiguration());

            // Feature Flag configuration
            modelBuilder.ApplyConfiguration(new FeatureFlagConfiguration());

            // Permission configuration
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());

            // PMS Integration configurations
            ConfigurePMSIntegrations(modelBuilder);

            // GuestReview configuration to avoid multiple cascade paths
            modelBuilder.Entity<GuestReview>(entity =>
            {
                entity.HasOne(e => e.Guest)
                    .WithMany()
                    .HasForeignKey(e => e.GuestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Reservation)
                    .WithMany()
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Restrict); // Corrected to restrict to avoid multiple cascade paths
            });

            modelBuilder.Entity<SettingEntity>().HasData(new SettingEntity
            {
                Id = 1,
                TenantId = 1, // Default tenant
                MainteneceMode = false,
                // IMPORTANT: keep seed deterministic; BaseEntity constructor sets CreatedDate=UtcNow.
                CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false,
                CreatedByPersonnelId = null,
                UpdatedByPersonnelId = null,
                UpdatedDate = null
            });

            ApplyGlobalFilters(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>().Where(e => e.State == EntityState.Added))
            {
                entry.Entity.TenantId = _tenantProvider.TenantId;
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(GuestFlowDbContext)
                        .GetMethod(nameof(ApplyFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        private void ApplyFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId && !((ISoftDelete)(object)e).IsDeleted);
        }

        private void ConfigurePMSIntegrations(ModelBuilder modelBuilder)
        {
            // PMSIntegration configuration
            modelBuilder.Entity<PMSIntegration>(entity =>
            {
                entity.ToTable("PMSIntegrations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProviderName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ProviderCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ApiEndpoint).IsRequired();
                entity.Property(e => e.ApiKey).IsRequired();
                entity.Property(e => e.LastSyncStatus).HasMaxLength(50);
                entity.Property(e => e.SyncErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.SyncMode).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => e.ProviderCode);
                entity.HasIndex(e => e.IsActive);
            });

            // PMSSyncHistory configuration
            modelBuilder.Entity<PMSSyncHistory>(entity =>
            {
                entity.ToTable("PMSSyncHistories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
                entity.Property(e => e.SyncDetails).HasColumnType("nvarchar(max)");
                entity.Property(e => e.SyncType).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasOne(e => e.PMSIntegration)
                    .WithMany(i => i.SyncHistories)
                    .HasForeignKey(e => e.PMSIntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.PMSIntegrationId);
                entity.HasIndex(e => e.SyncStartTime);
                entity.HasIndex(e => new { e.PMSIntegrationId, e.SyncType, e.Status });
            });

            // PMSGuestMapping configuration
            modelBuilder.Entity<PMSGuestMapping>(entity =>
            {
                entity.ToTable("PMSGuestMappings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PMSGuestId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SyncStatus).HasMaxLength(50);
                entity.Property(e => e.ConflictDetails).HasMaxLength(2000);
                entity.HasOne(e => e.PMSIntegration)
                    .WithMany(i => i.GuestMappings)
                    .HasForeignKey(e => e.PMSIntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.GuestFlowGuest)
                    .WithMany()
                    .HasForeignKey(e => e.GuestFlowGuestId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.PMSIntegrationId);
                entity.HasIndex(e => e.PMSGuestId);
                entity.HasIndex(e => e.GuestFlowGuestId);
                entity.HasIndex(e => new { e.PMSIntegrationId, e.PMSGuestId }).IsUnique();
            });

            // PMSReservationMapping configuration
            modelBuilder.Entity<PMSReservationMapping>(entity =>
            {
                entity.ToTable("PMSReservationMappings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PMSReservationId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SyncStatus).HasMaxLength(50);
                entity.Property(e => e.ConflictDetails).HasMaxLength(2000);
                entity.HasOne(e => e.PMSIntegration)
                    .WithMany(i => i.ReservationMappings)
                    .HasForeignKey(e => e.PMSIntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.PMSIntegrationId);
                entity.HasIndex(e => e.PMSReservationId);
                entity.HasIndex(e => new { e.PMSIntegrationId, e.PMSReservationId }).IsUnique();
            });
        }
    }
}