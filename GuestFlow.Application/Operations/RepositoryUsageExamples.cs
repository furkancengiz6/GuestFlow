using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations
{
    /// <summary>
    /// Repository Pattern kullanım örnekleri
    /// Bu dosya sadece referans amaçlıdır, gerçek kodda kullanılmaz
    /// </summary>
    public class RepositoryUsageExamples
    {
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;

        public RepositoryUsageExamples(
            IRepository<GuestEntity> guestRepository,
            IRepository<InvoicesEntity> invoiceRepository)
        {
            _guestRepository = guestRepository;
            _invoiceRepository = invoiceRepository;
        }

        #region Temel Kullanım Örnekleri

        /// <summary>
        /// Örnek 1: Basit GetByIdAsync kullanımı (soft delete filtrelenir)
        /// </summary>
        public async Task<GuestEntity> Example1_GetById()
        {
            // Soft delete edilmemiş kayıtları getirir
            var guest = await _guestRepository.GetByIdAsync(1);
            return guest;
        }

        /// <summary>
        /// Örnek 2: GetByIdAsync ile Include kullanımı (eager loading)
        /// </summary>
        public async Task<InvoicesEntity> Example2_GetByIdWithIncludes()
        {
            // Guest ve Personnel navigation property'lerini de yükler
            var invoice = await _invoiceRepository.GetByIdAsync(
                1,
                i => i.Guest,
                i => i.Personnel
            );
            return invoice;
        }

        /// <summary>
        /// Örnek 3: GetAsync ile predicate kullanımı
        /// </summary>
        public async Task<GuestEntity> Example3_GetWithPredicate()
        {
            var guest = await _guestRepository.GetAsync(
                g => g.Email == "test@example.com"
            );
            return guest;
        }

        /// <summary>
        /// Örnek 4: GetAsync ile Include kullanımı
        /// </summary>
        public async Task<InvoicesEntity> Example4_GetWithPredicateAndIncludes()
        {
            var invoice = await _invoiceRepository.GetAsync(
                i => i.InvoiceNumber == 1, // InvoiceNumber int tipinde
                i => i.Guest,
                i => i.Personnel
            );
            return invoice;
        }

        /// <summary>
        /// Örnek 5: GetAll ile filtreleme
        /// </summary>
        public async Task<List<GuestEntity>> Example5_GetAllWithFilter()
        {
            var guests = await _guestRepository.GetAll(
                g => g.IsSpecialGuest == true
            )
            .ToListAsync();
            return guests;
        }

        /// <summary>
        /// Örnek 6: GetAll ile Include kullanımı
        /// </summary>
        public async Task<List<InvoicesEntity>> Example6_GetAllWithIncludes()
        {
            var invoices = await _invoiceRepository.GetAll(
                predicate: null,
                i => i.Guest,
                i => i.Personnel
            )
            .ToListAsync();
            return invoices;
        }

        /// <summary>
        /// Örnek 7: Soft delete edilmiş kayıtları da getir
        /// </summary>
        public async Task<GuestEntity> Example7_IncludeDeleted()
        {
            // includeDeleted = true ile soft delete edilmiş kayıtları da getirir
            var guest = await _guestRepository.GetByIdAsync(1, includeDeleted: true);
            return guest;
        }

        /// <summary>
        /// Örnek 8: CountAsync kullanımı
        /// </summary>
        public async Task<int> Example8_Count()
        {
            // Soft delete edilmemiş misafir sayısı
            var count = await _guestRepository.CountAsync();
            return count;
        }

        /// <summary>
        /// Örnek 9: CountAsync ile predicate
        /// </summary>
        public async Task<int> Example9_CountWithPredicate()
        {
            var count = await _guestRepository.CountAsync(
                g => g.IsSpecialGuest == true
            );
            return count;
        }

        /// <summary>
        /// Örnek 10: AnyAsync kullanımı
        /// </summary>
        public async Task<bool> Example10_Any()
        {
            var exists = await _guestRepository.AnyAsync(
                g => g.Email == "test@example.com"
            );
            return exists;
        }

        #endregion

        #region Specification Pattern Kullanım Örnekleri

        /// <summary>
        /// Örnek 11: Specification pattern ile basit sorgu
        /// </summary>
        public async Task<GuestEntity> Example11_SpecificationSimple()
        {
            var specification = new SpecificationBuilder<GuestEntity>()
                .Where(g => g.Email == "test@example.com")
                .Build();

            var guest = await _guestRepository.GetBySpecificationAsync(specification);
            return guest;
        }

        /// <summary>
        /// Örnek 12: Specification pattern ile Include
        /// </summary>
        public async Task<InvoicesEntity> Example12_SpecificationWithIncludes()
        {
            var specification = new SpecificationBuilder<InvoicesEntity>()
                .Where(i => i.InvoiceNumber == 1) // InvoiceNumber int tipinde
                .Include(i => i.Guest, i => i.Personnel)
                .Build();

            var invoice = await _invoiceRepository.GetBySpecificationAsync(specification);
            return invoice;
        }

        /// <summary>
        /// Örnek 13: Specification pattern ile OrderBy
        /// </summary>
        public async Task<List<GuestEntity>> Example13_SpecificationWithOrderBy()
        {
            var specification = new SpecificationBuilder<GuestEntity>()
                .Where(g => g.IsSpecialGuest == true)
                .OrderBy(g => g.CreatedDate)
                .Build();

            var guests = await _guestRepository.GetAllBySpecificationAsync(specification);
            return guests;
        }

        /// <summary>
        /// Örnek 14: Specification pattern ile sayfalama (pagination)
        /// </summary>
        public async Task<List<GuestEntity>> Example14_SpecificationWithPagination()
        {
            var specification = new SpecificationBuilder<GuestEntity>()
                .OrderByDescending(g => g.CreatedDate)
                .Skip(0)  // İlk sayfa
                .Take(10) // 10 kayıt
                .Build();

            var guests = await _guestRepository.GetAllBySpecificationAsync(specification);
            return guests;
        }

        /// <summary>
        /// Örnek 15: Specification pattern ile soft delete edilmiş kayıtları da getir
        /// </summary>
        public async Task<GuestEntity> Example15_SpecificationIncludeDeleted()
        {
            var specification = new SpecificationBuilder<GuestEntity>()
                .Where(g => g.Id == 1)
                .IncludeDeleted(true) // Soft delete edilmiş kayıtları da getir
                .Build();

            var guest = await _guestRepository.GetBySpecificationAsync(specification);
            return guest;
        }

        /// <summary>
        /// Örnek 16: Specification pattern ile karmaşık sorgu
        /// </summary>
        public async Task<List<InvoicesEntity>> Example16_SpecificationComplex()
        {
            var specification = new SpecificationBuilder<InvoicesEntity>()
                .Where(i => i.TotalAmount > 1000 && i.Currency == "TRY")
                .Include(i => i.Guest, i => i.Personnel)
                .OrderByDescending(i => i.IssueDate)
                .Skip(0)
                .Take(20)
                .Build();

            var invoices = await _invoiceRepository.GetAllBySpecificationAsync(specification);
            return invoices;
        }

        /// <summary>
        /// Örnek 17: Specification pattern ile IQueryable döndürme (daha fazla işlem için)
        /// </summary>
        public async Task<List<GuestEntity>> Example17_SpecificationQueryable()
        {
            var specification = new SpecificationBuilder<GuestEntity>()
                .Where(g => g.IsSpecialGuest == true)
                .OrderBy(g => g.FullName)
                .Build();

            var query = _guestRepository.GetQueryableBySpecification(specification);
            
            // Ek filtreleme veya işlemler yapılabilir
            var result = await query
                .Where(g => g.Email != null)
                .ToListAsync();

            return result;
        }

        #endregion

        #region Gelişmiş Kullanım Örnekleri

        /// <summary>
        /// Örnek 18: GetAll ile manuel Include (EF Core)
        /// </summary>
        public async Task<List<InvoicesEntity>> Example18_ManualInclude()
        {
            var invoices = await _invoiceRepository.GetAll()
                .Include(i => i.Guest)
                .Include(i => i.Personnel)
                .Where(i => i.TotalAmount > 500)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return invoices;
        }

        /// <summary>
        /// Örnek 19: GetAll ile ThenInclude (nested navigation properties)
        /// </summary>
        public async Task<List<InvoicesEntity>> Example19_ThenInclude()
        {
            var invoices = await _invoiceRepository.GetAll()
                .Include(i => i.Guest)
                    .ThenInclude(g => g.Transfers) // Guest'in Transfers'ı
                .Include(i => i.Personnel)
                .ToListAsync();

            return invoices;
        }

        /// <summary>
        /// Örnek 20: Soft delete kontrolü ile güvenli silme
        /// </summary>
        public async Task<bool> Example20_SafeDelete()
        {
            // Önce kaydın var olup olmadığını kontrol et
            var guest = await _guestRepository.GetByIdAsync(1);
            if (guest == null)
            {
                return false; // Kayıt bulunamadı veya zaten silinmiş
            }

            // Soft delete yap
            await _guestRepository.DeleteAsync(guest);
            return true;
        }

        #endregion
    }
}

