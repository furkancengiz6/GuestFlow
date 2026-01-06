using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.ServicePackage.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.ServicePackage
{
    public class ServicePackageManager : IServicePackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ServicePackageEntity> _packageRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservationRepository;
        private readonly ILogger<ServicePackageManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GuestFlowDbContext _context;

        public ServicePackageManager(
            IUnitOfWork unitOfWork,
            IRepository<ServicePackageEntity> packageRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<RestaurantReservationEntity> restaurantReservationRepository,
            ILogger<ServicePackageManager> logger,
            IMapper mapper,
            ICacheService cacheService,
            GuestFlowDbContext context)
        {
            _unitOfWork = unitOfWork;
            _packageRepository = packageRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _restaurantReservationRepository = restaurantReservationRepository;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
        }

        public async Task<ServiceMessage<GetServicePackageDto>> AddServicePackage(AddServicePackageDto package)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageEntity = _mapper.Map<ServicePackageEntity>(package);
                packageEntity.TotalPrice = 0; // Başlangıçta 0, item'lar eklendikçe güncellenecek
                packageEntity.FinalPrice = 0;

                await _packageRepository.AddAsync(packageEntity);
                await _unitOfWork.SaveChangesAsync();

                // Transfer'leri ekle
                if (package.TransferIds != null && package.TransferIds.Any())
                {
                    foreach (var transferId in package.TransferIds)
                    {
                        var packageTransfer = new PackageTransferEntity
                        {
                            PackageId = packageEntity.Id,
                            TransferId = transferId
                        };
                        _context.PackageTransfers.Add(packageTransfer);
                    }
                }

                // CityTour'ları ekle
                if (package.CityTourIds != null && package.CityTourIds.Any())
                {
                    foreach (var cityTourId in package.CityTourIds)
                    {
                        var packageCityTour = new PackageCityTourEntity
                        {
                            PackageId = packageEntity.Id,
                            CityTourId = cityTourId
                        };
                        _context.PackageCityTours.Add(packageCityTour);
                    }
                }

                // YachtTour'ları ekle
                if (package.YachtTourIds != null && package.YachtTourIds.Any())
                {
                    foreach (var yachtTourId in package.YachtTourIds)
                    {
                        var packageYachtTour = new PackageYachtTourEntity
                        {
                            PackageId = packageEntity.Id,
                            YachtTourId = yachtTourId
                        };
                        _context.PackageYachtTours.Add(packageYachtTour);
                    }
                }

                // RestaurantReservation'ları ekle
                if (package.RestaurantReservationIds != null && package.RestaurantReservationIds.Any())
                {
                    foreach (var reservationId in package.RestaurantReservationIds)
                    {
                        var packageReservation = new PackageRestaurantReservationEntity
                        {
                            PackageId = packageEntity.Id,
                            RestaurantReservationId = reservationId
                        };
                        _context.PackageRestaurantReservations.Add(packageReservation);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti hesapla
                packageEntity.TotalPrice = await CalculatePackageTotalCost(packageEntity.Id);
                if (packageEntity.DiscountPercentage.HasValue && packageEntity.DiscountPercentage > 0)
                {
                    packageEntity.FinalPrice = packageEntity.TotalPrice * (1 - packageEntity.DiscountPercentage.Value / 100);
                }
                else
                {
                    packageEntity.FinalPrice = packageEntity.TotalPrice;
                }

                await _packageRepository.UpdateAsync(packageEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = await GetServicePackageById(packageEntity.Id);
                _logger.LogInformation($"Servis paketi eklendi: {package.PackageName}");
                return new ServiceMessage<GetServicePackageDto> { IsSuccess = true, Message = "Servis paketi başarıyla oluşturuldu.", Data = result };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Servis paketi eklenirken hata: {ex.Message}");
                return new ServiceMessage<GetServicePackageDto> { IsSuccess = false, Message = $"Servis paketi eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateServicePackage(UpdateServicePackageDto package)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageEntity = await _packageRepository.GetByIdAsync(package.Id);
                if (packageEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Paket bulunamadı." };

                packageEntity.PackageName = package.PackageName;
                packageEntity.Description = package.Description;
                packageEntity.PackageType = package.PackageType;
                packageEntity.StartDate = package.StartDate;
                packageEntity.EndDate = package.EndDate;
                packageEntity.DiscountPercentage = package.DiscountPercentage;
                packageEntity.Currency = package.Currency;
                packageEntity.IsActive = package.IsActive;
                packageEntity.PackageContent = package.PackageContent;
                packageEntity.Notes = package.Notes;

                // Toplam maliyeti yeniden hesapla
                packageEntity.TotalPrice = await CalculatePackageTotalCost(package.Id);
                if (packageEntity.DiscountPercentage.HasValue && packageEntity.DiscountPercentage > 0)
                {
                    packageEntity.FinalPrice = packageEntity.TotalPrice * (1 - packageEntity.DiscountPercentage.Value / 100);
                }
                else
                {
                    packageEntity.FinalPrice = packageEntity.TotalPrice;
                }

                await _packageRepository.UpdateAsync(packageEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Servis paketi güncellendi: {package.PackageName}");
                return new ServiceMessage { IsSuccess = true, Message = "Servis paketi başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Servis paketi güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Servis paketi güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeleteServicePackage(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageEntity = await _packageRepository.GetByIdAsync(id);
                if (packageEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Paket bulunamadı." };

                await _packageRepository.DeleteAsync(packageEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Servis paketi silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Servis paketi başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Servis paketi silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Servis paketi silinirken hata: {ex.Message}" };
            }
        }

        public async Task<GetServicePackageDto> GetServicePackageById(int id)
        {
            var package = await _packageRepository.GetAll(
                x => x.Id == id,
                x => x.PackageTransfers,
                x => x.PackageCityTours,
                x => x.PackageYachtTours,
                x => x.PackageRestaurantReservations)
                .FirstOrDefaultAsync();

            if (package == null)
                return null!;

            var dto = _mapper.Map<GetServicePackageDto>(package);
            dto.TransferIds = package.PackageTransfers.Select(pt => pt.TransferId).ToList();
            dto.CityTourIds = package.PackageCityTours.Select(pct => pct.CityTourId).ToList();
            dto.YachtTourIds = package.PackageYachtTours.Select(pyt => pyt.YachtTourId).ToList();
            dto.RestaurantReservationIds = package.PackageRestaurantReservations.Select(prr => prr.RestaurantReservationId).ToList();

            return dto;
        }

        public async Task<List<GetServicePackageDto>> GetServicePackages()
        {
            var packages = await _packageRepository.GetAll(
                x => x.IsActive,
                x => x.PackageTransfers,
                x => x.PackageCityTours,
                x => x.PackageYachtTours,
                x => x.PackageRestaurantReservations)
                .OrderBy(x => x.PackageName)
                .ToListAsync();

            return packages.Select(p =>
            {
                var dto = _mapper.Map<GetServicePackageDto>(p);
                dto.TransferIds = p.PackageTransfers.Select(pt => pt.TransferId).ToList();
                dto.CityTourIds = p.PackageCityTours.Select(pct => pct.CityTourId).ToList();
                dto.YachtTourIds = p.PackageYachtTours.Select(pyt => pyt.YachtTourId).ToList();
                dto.RestaurantReservationIds = p.PackageRestaurantReservations.Select(prr => prr.RestaurantReservationId).ToList();
                return dto;
            }).ToList();
        }

        public async Task<PagedResult<GetServicePackageDto>> GetServicePackagesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            var query = _packageRepository.GetAll(
                null,
                x => x.PackageTransfers,
                x => x.PackageCityTours,
                x => x.PackageYachtTours,
                x => x.PackageRestaurantReservations);

            if (sorting != null && !string.IsNullOrEmpty(sorting.SortBy))
            {
                var sortBy = sorting.SortBy.ToLower();
                var sortOrder = sorting.SortOrder?.ToLower() ?? "asc";
                query = sortBy switch
                {
                    "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                    "packagename" or "name" => sortOrder == "desc" ? query.OrderByDescending(x => x.PackageName) : query.OrderBy(x => x.PackageName),
                    "packagetype" or "type" => sortOrder == "desc" ? query.OrderByDescending(x => x.PackageType) : query.OrderBy(x => x.PackageType),
                    "totalprice" or "price" => sortOrder == "desc" ? query.OrderByDescending(x => x.TotalPrice) : query.OrderBy(x => x.TotalPrice),
                    "finalprice" => sortOrder == "desc" ? query.OrderByDescending(x => x.FinalPrice) : query.OrderBy(x => x.FinalPrice),
                    "startdate" or "start" => sortOrder == "desc" ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
                    "enddate" or "end" => sortOrder == "desc" ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                    "createddate" or "created" => sortOrder == "desc" ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                    _ => query.OrderBy(x => x.PackageName)
                };
            }
            else
            {
                query = query.OrderBy(x => x.PackageName);
            }

            var totalCount = await query.CountAsync();
            var packages = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = packages.Select(p =>
            {
                var dto = _mapper.Map<GetServicePackageDto>(p);
                dto.TransferIds = p.PackageTransfers.Select(pt => pt.TransferId).ToList();
                dto.CityTourIds = p.PackageCityTours.Select(pct => pct.CityTourId).ToList();
                dto.YachtTourIds = p.PackageYachtTours.Select(pyt => pyt.YachtTourId).ToList();
                dto.RestaurantReservationIds = p.PackageRestaurantReservations.Select(prr => prr.RestaurantReservationId).ToList();
                return dto;
            }).ToList();

            return new PagedResult<GetServicePackageDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ServiceMessage> AddTransferToPackage(int packageId, int transferId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var exists = await _context.PackageTransfers.AnyAsync(x => x.PackageId == packageId && x.TransferId == transferId);
                if (exists)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer zaten pakette mevcut." };

                var packageTransfer = new PackageTransferEntity
                {
                    PackageId = packageId,
                    TransferId = transferId
                };

                _context.PackageTransfers.Add(packageTransfer);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package != null)
                {
                    package.TotalPrice = await CalculatePackageTotalCost(packageId);
                    if (package.DiscountPercentage.HasValue && package.DiscountPercentage > 0)
                    {
                        package.FinalPrice = package.TotalPrice * (1 - package.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        package.FinalPrice = package.TotalPrice;
                    }
                    await _packageRepository.UpdateAsync(package);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ServiceMessage { IsSuccess = true, Message = "Transfer pakete eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer pakete eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Transfer eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> AddCityTourToPackage(int packageId, int cityTourId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var exists = await _context.PackageCityTours.AnyAsync(x => x.PackageId == packageId && x.CityTourId == cityTourId);
                if (exists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu zaten pakette mevcut." };

                var packageCityTour = new PackageCityTourEntity
                {
                    PackageId = packageId,
                    CityTourId = cityTourId
                };

                _context.PackageCityTours.Add(packageCityTour);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package != null)
                {
                    package.TotalPrice = await CalculatePackageTotalCost(packageId);
                    if (package.DiscountPercentage.HasValue && package.DiscountPercentage > 0)
                    {
                        package.FinalPrice = package.TotalPrice * (1 - package.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        package.FinalPrice = package.TotalPrice;
                    }
                    await _packageRepository.UpdateAsync(package);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu pakete eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir turu pakete eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Şehir turu eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> AddYachtTourToPackage(int packageId, int yachtTourId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var exists = await _context.PackageYachtTours.AnyAsync(x => x.PackageId == packageId && x.YachtTourId == yachtTourId);
                if (exists)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu zaten pakette mevcut." };

                var packageYachtTour = new PackageYachtTourEntity
                {
                    PackageId = packageId,
                    YachtTourId = yachtTourId
                };

                _context.PackageYachtTours.Add(packageYachtTour);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package != null)
                {
                    package.TotalPrice = await CalculatePackageTotalCost(packageId);
                    if (package.DiscountPercentage.HasValue && package.DiscountPercentage > 0)
                    {
                        package.FinalPrice = package.TotalPrice * (1 - package.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        package.FinalPrice = package.TotalPrice;
                    }
                    await _packageRepository.UpdateAsync(package);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu pakete eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu pakete eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Yat turu eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> AddRestaurantReservationToPackage(int packageId, int reservationId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var exists = await _context.PackageRestaurantReservations.AnyAsync(x => x.PackageId == packageId && x.RestaurantReservationId == reservationId);
                if (exists)
                    return new ServiceMessage { IsSuccess = false, Message = "Restoran rezervasyonu zaten pakette mevcut." };

                var packageReservation = new PackageRestaurantReservationEntity
                {
                    PackageId = packageId,
                    RestaurantReservationId = reservationId
                };

                _context.PackageRestaurantReservations.Add(packageReservation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ServiceMessage { IsSuccess = true, Message = "Restoran rezervasyonu pakete eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran rezervasyonu pakete eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran rezervasyonu eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> RemoveTransferFromPackage(int packageId, int transferId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageTransfer = await _context.PackageTransfers.FirstOrDefaultAsync(x => x.PackageId == packageId && x.TransferId == transferId);
                if (packageTransfer == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer pakette bulunamadı." };

                // PackageTransferEntity BaseEntity'den türemediği için DbContext üzerinden silme
                _context.PackageTransfers.Remove(packageTransfer);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package != null)
                {
                    package.TotalPrice = await CalculatePackageTotalCost(packageId);
                    if (package.DiscountPercentage.HasValue && package.DiscountPercentage > 0)
                    {
                        package.FinalPrice = package.TotalPrice * (1 - package.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        package.FinalPrice = package.TotalPrice;
                    }
                    await _packageRepository.UpdateAsync(package);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ServiceMessage { IsSuccess = true, Message = "Transfer paketten kaldırıldı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer paketten kaldırılırken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Transfer kaldırılırken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> RemoveCityTourFromPackage(int packageId, int cityTourId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageCityTour = await _context.PackageCityTours.FirstOrDefaultAsync(x => x.PackageId == packageId && x.CityTourId == cityTourId);
                if (packageCityTour == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu pakette bulunamadı." };

                _context.PackageCityTours.Remove(packageCityTour);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package != null)
                {
                    package.TotalPrice = await CalculatePackageTotalCost(packageId);
                    if (package.DiscountPercentage.HasValue && package.DiscountPercentage > 0)
                    {
                        package.FinalPrice = package.TotalPrice * (1 - package.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        package.FinalPrice = package.TotalPrice;
                    }
                    await _packageRepository.UpdateAsync(package);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu paketten kaldırıldı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir turu paketten kaldırılırken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Şehir turu kaldırılırken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> RemoveYachtTourFromPackage(int packageId, int yachtTourId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageYachtTour = await _context.PackageYachtTours.FirstOrDefaultAsync(x => x.PackageId == packageId && x.YachtTourId == yachtTourId);
                if (packageYachtTour == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu pakette bulunamadı." };

                _context.PackageYachtTours.Remove(packageYachtTour);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package != null)
                {
                    package.TotalPrice = await CalculatePackageTotalCost(packageId);
                    if (package.DiscountPercentage.HasValue && package.DiscountPercentage > 0)
                    {
                        package.FinalPrice = package.TotalPrice * (1 - package.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        package.FinalPrice = package.TotalPrice;
                    }
                    await _packageRepository.UpdateAsync(package);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu paketten kaldırıldı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu paketten kaldırılırken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Yat turu kaldırılırken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> RemoveRestaurantReservationFromPackage(int packageId, int reservationId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var packageReservation = await _context.PackageRestaurantReservations.FirstOrDefaultAsync(x => x.PackageId == packageId && x.RestaurantReservationId == reservationId);
                if (packageReservation == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Restoran rezervasyonu pakette bulunamadı." };

                _context.PackageRestaurantReservations.Remove(packageReservation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ServiceMessage { IsSuccess = true, Message = "Restoran rezervasyonu paketten kaldırıldı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran rezervasyonu paketten kaldırılırken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran rezervasyonu kaldırılırken hata: {ex.Message}" };
            }
        }

        public async Task<decimal> CalculatePackageTotalCost(int packageId)
        {
            decimal totalCost = 0;

            // Transfer maliyetlerini hesapla
            var transfers = await _context.PackageTransfers
                .Where(x => x.PackageId == packageId)
                .Select(pt => pt.TransferId)
                .ToListAsync();

            foreach (var transferId in transfers)
            {
                var transfer = await _transferRepository.GetByIdAsync(transferId);
                if (transfer != null)
                    totalCost += transfer.FinalPrice;
            }

            // CityTour maliyetlerini hesapla
            var cityTours = await _context.PackageCityTours
                .Where(x => x.PackageId == packageId)
                .Select(pct => pct.CityTourId)
                .ToListAsync();

            foreach (var cityTourId in cityTours)
            {
                var cityTour = await _cityTourRepository.GetByIdAsync(cityTourId);
                if (cityTour != null)
                    totalCost += cityTour.FinalPrice;
            }

            // YachtTour maliyetlerini hesapla
            var yachtTours = await _context.PackageYachtTours
                .Where(x => x.PackageId == packageId)
                .Select(pyt => pyt.YachtTourId)
                .ToListAsync();

            foreach (var yachtTourId in yachtTours)
            {
                var yachtTour = await _yachtTourRepository.GetByIdAsync(yachtTourId);
                if (yachtTour != null)
                    totalCost += yachtTour.FinalPrice;
            }

            // RestaurantReservation'lar genellikle ücretsiz veya ayrı fiyatlandırılır
            // Şimdilik 0 olarak bırakıyoruz

            return totalCost;
        }
    }
}

