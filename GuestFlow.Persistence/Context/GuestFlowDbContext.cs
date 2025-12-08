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
        public DbSet<CityTourEntity> CityTours => Set<CityTourEntity>();
        public DbSet<DailyNoteEntity> DailyNotes => Set<DailyNoteEntity>();
        public DbSet<DailyRevenueEntity> DailyRevenues => Set<DailyRevenueEntity>();
        public DbSet<GuestEntity> Guests => Set<GuestEntity>();
        public DbSet<InvoicesEntity> Invoices => Set<InvoicesEntity>();
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API ile yapılandırmaları uyguluyoruz
            modelBuilder.ApplyConfiguration(new AirportConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
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
            modelBuilder.Entity<SettingEntity>().HasData(new SettingEntity
            {
                Id = 1,
                MainteneceMode = false
            }


                );
        }
    }
}