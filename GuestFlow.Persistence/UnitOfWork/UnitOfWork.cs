using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Persistence.Repositories;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GuestFlowDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;
        // Repository instances
        private readonly IRepository<TransferEntity> _transfers;
        private readonly IRepository<CityTourEntity> _cityTours;
        private readonly IRepository<YachtTourEntity> _yachtTours;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservations;
        private readonly IRepository<Supplier> _suppliers;
        private readonly IRepository<OTAIntegration> _otaIntegrations;
        private readonly IRepository<OTAPriceUpdate> _otaPriceUpdates;
        private readonly IRepository<OTAReservation> _otaReservations;
        private readonly IRepository<InvoicesEntity> _invoices;
        private readonly IRepository<InvoiceItemEntity> _invoiceItems;
        private readonly IRepository<JournalEntry> _journalEntries;
        private readonly IRepository<JournalLine> _journalLines;
        private readonly IRepository<SupplierCost> _supplierCosts;

        public UnitOfWork(GuestFlowDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            // instantiate repositories
            _transfers = new Repository<TransferEntity>(_context);
            _cityTours = new Repository<CityTourEntity>(_context);
            _yachtTours = new Repository<YachtTourEntity>(_context);
            _restaurantReservations = new Repository<RestaurantReservationEntity>(_context);
            _suppliers = new Repository<Supplier>(_context);
            _otaIntegrations = new Repository<OTAIntegration>(_context);
            _otaPriceUpdates = new Repository<OTAPriceUpdate>(_context);
            _otaReservations = new Repository<OTAReservation>(_context);
            _invoices = new Repository<InvoicesEntity>(_context);
            _invoiceItems = new Repository<InvoiceItemEntity>(_context);
            _journalEntries = new Repository<JournalEntry>(_context);
            _journalLines = new Repository<JournalLine>(_context);
            _supplierCosts = new Repository<SupplierCost>(_context);
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
        public IRepository<TransferEntity> Transfers => _transfers;
        public IRepository<CityTourEntity> CityTours => _cityTours;
        public IRepository<YachtTourEntity> YachtTours => _yachtTours;
        public IRepository<RestaurantReservationEntity> RestaurantReservations => _restaurantReservations;
        public IRepository<Supplier> Suppliers => _suppliers;
        public IRepository<OTAIntegration> OTAIntegrations => _otaIntegrations;
        public IRepository<OTAPriceUpdate> OTAPriceUpdates => _otaPriceUpdates;
        public IRepository<OTAReservation> OTAReservations => _otaReservations;
        public IRepository<InvoicesEntity> Invoices => _invoices;
        public IRepository<InvoiceItemEntity> InvoiceItems => _invoiceItems;
        public IRepository<JournalEntry> JournalEntries => _journalEntries;
        public IRepository<JournalLine> JournalLines => _journalLines;
        public IRepository<SupplierCost> SupplierCosts => _supplierCosts;
    }
}