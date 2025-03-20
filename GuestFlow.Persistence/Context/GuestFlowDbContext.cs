using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Persistence.Context
{
    public class GuestFlowDbContext :DbContext
    {
        public GuestFlowDbContext(DbContextOptions<GuestFlowDbContext> options):base(options)
        {
            
        }

        public DbSet<AirportEntity> Airports => Set<AirportEntity>();

        public DbSet<PersonnelEntity> Personnels => Set<PersonnelEntity>();

        public DbSet<CityEntity> Cities => Set<CityEntity>();

        public DbSet<CityTourEntity> CityTours =>  Set<CityTourEntity>();

        public DbSet<DailyNote> DailyNotes => Set<DailyNote>();

        public DbSet<DailyRevenueEntity> DailyRevenues => Set<DailyRevenueEntity>();

        public DbSet<GuestEntity> Guests => Set<GuestEntity>();


        public DbSet<InvoicesEntity> Invoices => Set<InvoicesEntity>();


        public DbSet<TransferEntity> Transfers => Set<TransferEntity>();

        public DbSet<VehicleEntity> Vehicles => Set<VehicleEntity>();

        public DbSet<YachtTourEntity> YachtTours => Set<YachtTourEntity>();

        public DbSet<GuestYachtTour> GuestYachtTours =>Set<GuestYachtTour>();

        public DbSet<GuestCityTour> GuestCityTours => Set<GuestCityTour>();


        //Fluent Api
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Çoka-Çok (Many-to-Many) İlişkiler
            modelBuilder.Entity<GuestYachtTour>().HasKey(gt => new { gt.GuestId, gt.YachtTourId });
            modelBuilder.Entity<GuestYachtTour>()
                .HasOne(gt => gt.Guest)
                .WithMany(g => g.GuestYachtTours)
                .HasForeignKey(gt => gt.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GuestYachtTour>()
                .HasOne(gt => gt.YachtTour)
                .WithMany(yt => yt.GuestYachtTours)
                .HasForeignKey(gt => gt.YachtTourId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GuestCityTour>().HasKey(gt => new { gt.GuestId, gt.CityTourId });
            modelBuilder.Entity<GuestCityTour>()
                .HasOne(gt => gt.Guest)
                .WithMany(g => g.GuestCityTours)
                .HasForeignKey(gt => gt.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GuestCityTour>()
                .HasOne(gt => gt.CityTour)
                .WithMany(ct => ct.GuestCityTours)
                .HasForeignKey(gt => gt.CityTourId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir-Çok (One-to-Many) İlişkiler
            modelBuilder.Entity<TransferEntity>().HasOne(t => t.Guest).WithMany(g => g.Transfers).HasForeignKey(t => t.GuestId);
            modelBuilder.Entity<TransferEntity>().HasOne(t => t.Personnel).WithMany(p => p.Transfers).HasForeignKey(t => t.PersonnelId);
            modelBuilder.Entity<TransferEntity>().HasOne(t => t.Airport).WithMany(a => a.Transfers).HasForeignKey(t => t.AirportId);
            modelBuilder.Entity<TransferEntity>().HasOne(t => t.Vehicle).WithMany(v => v.Transfers).HasForeignKey(t => t.VehicleId);

            modelBuilder.Entity<YachtTourEntity>().HasOne(yt => yt.Guest).WithMany(g => g.YachtTours).HasForeignKey(yt => yt.GuestId);
            modelBuilder.Entity<YachtTourEntity>()
                .HasOne(yt => yt.Personnel)
                .WithMany(p => p.YachtTours)
                .HasForeignKey(yt => yt.PersonnelId);

            modelBuilder.Entity<CityTourEntity>()
                .HasOne(ct => ct.Personnel)
                .WithMany(p => p.CityTours)
                .HasForeignKey(ct => ct.PersonnelId);
            modelBuilder.Entity<CityTourEntity>()
                .Property(ct => ct.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DailyNote>()
                .HasOne(dn => dn.Personnel)
                .WithMany(p => p.DailyNotes)
                .HasForeignKey(dn => dn.PersonnelId);

            modelBuilder.Entity<DailyRevenueEntity>()
                .Property(dr => dr.TotalRevenue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InvoicesEntity>()
                .HasOne(i => i.Guest)
                .WithMany(g => g.Invoices)
                .HasForeignKey(i => i.GuestId);
            modelBuilder.Entity<InvoicesEntity>()
                .HasOne(i => i.Personnel)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PersonnelId);
            modelBuilder.Entity<InvoicesEntity>()
                .HasOne(i => i.Transfer)
                .WithMany(t => t.Invoices)
                .HasForeignKey(i => i.TransferId);
            modelBuilder.Entity<InvoicesEntity>()
                .HasOne(i => i.YachtTour)
                .WithMany()
                .HasForeignKey(i => i.YachtTourId);
            modelBuilder.Entity<InvoicesEntity>()
                .HasOne(i => i.CityTour)
                .WithMany()
                .HasForeignKey(i => i.CityTourId);
            modelBuilder.Entity<InvoicesEntity>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<TransferEntity>()
                .HasOne(t => t.Guest)
                .WithMany(g => g.Transfers)
                .HasForeignKey(t => t.GuestId);
            modelBuilder.Entity<TransferEntity>()
                .HasOne(t => t.Personnel)
                .WithMany(p => p.Transfers)
                .HasForeignKey(t => t.PersonnelId);
            modelBuilder.Entity<TransferEntity>()
                .HasOne(t => t.Airport)
                .WithMany(a => a.Transfers)
                .HasForeignKey(t => t.AirportId);
            modelBuilder.Entity<TransferEntity>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Transfers)
                .HasForeignKey(t => t.VehicleId);
            modelBuilder.Entity<TransferEntity>()
                .Property(t => t.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<VehicleEntity>()
                .Property(v => v.DailyPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<YachtTourEntity>()
                .HasOne(yt => yt.Guest)
                .WithMany(g => g.YachtTours)
                .HasForeignKey(yt => yt.GuestId);
            modelBuilder.Entity<YachtTourEntity>()
                .HasOne(yt => yt.Personnel)
                .WithMany(p => p.YachtTours)
                .HasForeignKey(yt => yt.PersonnelId);
            modelBuilder.Entity<YachtTourEntity>()
                .Property(yt => yt.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AirportEntity>()
                .HasOne(a => a.City)
                .WithMany(c => c.Airports)
                .HasForeignKey(a => a.CityId);

            base.OnModelCreating(modelBuilder);
        }

    }
}
