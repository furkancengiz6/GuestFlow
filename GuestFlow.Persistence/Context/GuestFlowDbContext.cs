using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Persistence.Context
{
    public class GuestFlowDbContext : DbContext
    {
        public GuestFlowDbContext(DbContextOptions<GuestFlowDbContext> options) : base(options)
        {
        }

        public DbSet<AirportEntity> Airports => Set<AirportEntity>();
        public DbSet<PersonnelEntity> Personnels => Set<PersonnelEntity>();
        public DbSet<CityEntity> Cities => Set<CityEntity>();
        public DbSet<TourEntity> Tours => Set<TourEntity>();
        public DbSet<CityTourEntity> CityTours => Set<CityTourEntity>();
        public DbSet<DailyNoteEntity> DailyNotes => Set<DailyNoteEntity>();
        public DbSet<DailyRevenueEntity> DailyRevenues => Set<DailyRevenueEntity>();
        public DbSet<GuestEntity> Guests => Set<GuestEntity>();
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
        public DbSet<EmailQueueEntity> EmailQueues => Set<EmailQueueEntity>();
        public DbSet<EmailTemplateEntity> EmailTemplates => Set<EmailTemplateEntity>();
        public DbSet<EmailHistoryEntity> EmailHistories => Set<EmailHistoryEntity>();
        public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();
        public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
        public DbSet<SmsHistoryEntity> SmsHistories => Set<SmsHistoryEntity>();
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
        // Accounting - journal entries
        public DbSet<GuestFlow.Domain.Entities.Core.JournalEntry> JournalEntries => Set<GuestFlow.Domain.Entities.Core.JournalEntry>();
        public DbSet<GuestFlow.Domain.Entities.Core.JournalLine> JournalLines => Set<GuestFlow.Domain.Entities.Core.JournalLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Accounting - DB idempotency: 1 invoice -> max 1 journal entry
            // (InvoiceId is nullable for backward compatibility; multiple NULLs allowed.)
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => e.InvoiceId)
                .IsUnique();

            // Fluent API ile yapılandırmaları uyguluyoruz
            modelBuilder.ApplyConfiguration(new AirportConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
            modelBuilder.ApplyConfiguration(new TourConfiguration());
            modelBuilder.ApplyConfiguration(new CityTourConfiguration());
            modelBuilder.ApplyConfiguration(new DailyNoteConfiguration());
            modelBuilder.ApplyConfiguration(new DailyRevenueConfiguration());
            modelBuilder.ApplyConfiguration(new GuestConfiguration());
            modelBuilder.ApplyConfiguration(new InvoicesConfiguration());
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
            modelBuilder.Entity<SettingEntity>().HasData(new SettingEntity
            {
                Id = 1,
                MainteneceMode = false
            }


                );
        }
    }
}