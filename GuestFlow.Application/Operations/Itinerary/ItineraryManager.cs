using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Itinerary.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Itinerary
{
    public class ItineraryManager : IItineraryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ItineraryEntity> _itineraryRepository;
        private readonly IRepository<ItineraryItemEntity> _itineraryItemRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservationRepository;
        private readonly ILogger<ItineraryManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public ItineraryManager(
            IUnitOfWork unitOfWork,
            IRepository<ItineraryEntity> itineraryRepository,
            IRepository<ItineraryItemEntity> itineraryItemRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<RestaurantReservationEntity> restaurantReservationRepository,
            ILogger<ItineraryManager> logger,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _itineraryRepository = itineraryRepository;
            _itineraryItemRepository = itineraryItemRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _restaurantReservationRepository = restaurantReservationRepository;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<ServiceMessage<GetItineraryDto>> AddItinerary(AddItineraryDto itinerary)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var guest = await _guestRepository.GetByIdAsync(itinerary.GuestId);
                if (guest == null)
                    return new ServiceMessage<GetItineraryDto> { IsSuccess = false, Message = "Misafir bulunamadı." };

                var personnel = await _personnelRepository.GetByIdAsync(itinerary.PersonnelId);
                if (personnel == null)
                    return new ServiceMessage<GetItineraryDto> { IsSuccess = false, Message = "Personel bulunamadı." };

                // Itinerary numarası oluştur
                var itineraryNumber = $"ITN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                var itineraryEntity = new ItineraryEntity
                {
                    GuestId = itinerary.GuestId,
                    PersonnelId = itinerary.PersonnelId,
                    StartDate = itinerary.StartDate,
                    EndDate = itinerary.EndDate,
                    Status = itinerary.Status,
                    Notes = itinerary.Notes,
                    ItineraryNumber = itineraryNumber,
                    TotalCost = 0, // Başlangıçta 0, item'lar eklendikçe güncellenecek
                    Currency = "TRY"
                };

                await _itineraryRepository.AddAsync(itineraryEntity);
                await _unitOfWork.SaveChangesAsync();

                // Item'ları ekle
                if (itinerary.Items != null && itinerary.Items.Any())
                {
                    foreach (var itemDto in itinerary.Items.OrderBy(i => i.Order))
                    {
                        var itemEntity = new ItineraryItemEntity
                        {
                            ItineraryId = itineraryEntity.Id,
                            ItemType = itemDto.ItemType,
                            ServiceId = itemDto.ServiceId,
                            ScheduledDateTime = itemDto.ScheduledDateTime,
                            Order = itemDto.Order,
                            Status = itemDto.Status,
                            Notes = itemDto.Notes
                        };
                        await _itineraryItemRepository.AddAsync(itemEntity);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // Toplam maliyeti hesapla
                itineraryEntity.TotalCost = await CalculateItineraryTotalCost(itineraryEntity.Id);
                await _itineraryRepository.UpdateAsync(itineraryEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = await GetItineraryById(itineraryEntity.Id);
                _logger.LogInformation($"İtinerary eklendi: {itineraryNumber}");
                return new ServiceMessage<GetItineraryDto> { IsSuccess = true, Message = "İtinerary başarıyla oluşturuldu.", Data = result };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary eklenirken hata: {ex.Message}");
                return new ServiceMessage<GetItineraryDto> { IsSuccess = false, Message = $"İtinerary eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateItinerary(UpdateItineraryDto itinerary)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var itineraryEntity = await _itineraryRepository.GetByIdAsync(itinerary.Id);
                if (itineraryEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "İtinerary bulunamadı." };

                itineraryEntity.StartDate = itinerary.StartDate;
                itineraryEntity.EndDate = itinerary.EndDate;
                itineraryEntity.Status = itinerary.Status;
                itineraryEntity.Notes = itinerary.Notes;

                // Toplam maliyeti yeniden hesapla
                itineraryEntity.TotalCost = await CalculateItineraryTotalCost(itinerary.Id);

                await _itineraryRepository.UpdateAsync(itineraryEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"İtinerary güncellendi: {itinerary.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "İtinerary başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"İtinerary güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeleteItinerary(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var itineraryEntity = await _itineraryRepository.GetByIdAsync(id);
                if (itineraryEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "İtinerary bulunamadı." };

                await _itineraryRepository.DeleteAsync(itineraryEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"İtinerary silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "İtinerary başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"İtinerary silinirken hata: {ex.Message}" };
            }
        }

        public async Task<GetItineraryDto> GetItineraryById(int id)
        {
            var itinerary = await _itineraryRepository.GetAll(
                x => x.Id == id,
                x => x.Guest,
                x => x.Personnel,
                x => x.Items)
                .FirstOrDefaultAsync();

            if (itinerary == null)
                return null!;

            var dto = new GetItineraryDto
            {
                Id = itinerary.Id,
                GuestId = itinerary.GuestId,
                GuestName = itinerary.Guest?.FullName ?? string.Empty,
                PersonnelId = itinerary.PersonnelId,
                PersonnelName = itinerary.Personnel?.FullName ?? string.Empty,
                StartDate = itinerary.StartDate,
                EndDate = itinerary.EndDate,
                Status = itinerary.Status,
                TotalCost = itinerary.TotalCost,
                Currency = itinerary.Currency,
                Notes = itinerary.Notes,
                ItineraryNumber = itinerary.ItineraryNumber,
                CreatedDate = itinerary.CreatedDate
            };

            // Item'ları yükle ve servis bilgilerini ekle
            foreach (var item in itinerary.Items.OrderBy(i => i.Order))
            {
                var itemDto = new GetItineraryItemDto
                {
                    Id = item.Id,
                    ItineraryId = item.ItineraryId,
                    ItemType = item.ItemType,
                    ServiceId = item.ServiceId,
                    ScheduledDateTime = item.ScheduledDateTime,
                    Order = item.Order,
                    Status = item.Status,
                    Notes = item.Notes,
                    ServiceName = await GetServiceNameAsync(item.ItemType, item.ServiceId)
                };
                dto.Items.Add(itemDto);
            }

            return dto;
        }

        public async Task<List<GetItineraryDto>> GetItinerariesByGuestId(int guestId)
        {
            var itineraries = await _itineraryRepository.GetAll(
                x => x.GuestId == guestId,
                x => x.Guest,
                x => x.Personnel,
                x => x.Items)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var result = new List<GetItineraryDto>();
            foreach (var itinerary in itineraries)
            {
                var dto = new GetItineraryDto
                {
                    Id = itinerary.Id,
                    GuestId = itinerary.GuestId,
                    GuestName = itinerary.Guest?.FullName ?? string.Empty,
                    PersonnelId = itinerary.PersonnelId,
                    PersonnelName = itinerary.Personnel?.FullName ?? string.Empty,
                    StartDate = itinerary.StartDate,
                    EndDate = itinerary.EndDate,
                    Status = itinerary.Status,
                    TotalCost = itinerary.TotalCost,
                    Currency = itinerary.Currency,
                    Notes = itinerary.Notes,
                    ItineraryNumber = itinerary.ItineraryNumber,
                    CreatedDate = itinerary.CreatedDate
                };

                foreach (var item in itinerary.Items.OrderBy(i => i.Order))
                {
                    var itemDto = new GetItineraryItemDto
                    {
                        Id = item.Id,
                        ItineraryId = item.ItineraryId,
                        ItemType = item.ItemType,
                        ServiceId = item.ServiceId,
                        ScheduledDateTime = item.ScheduledDateTime,
                        Order = item.Order,
                        Status = item.Status,
                        Notes = item.Notes,
                        ServiceName = await GetServiceNameAsync(item.ItemType, item.ServiceId)
                    };
                    dto.Items.Add(itemDto);
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task<PagedResult<GetItineraryDto>> GetItinerariesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            var query = _itineraryRepository.GetAll(null, x => x.Guest, x => x.Personnel, x => x.Items);

            if (sorting != null && !string.IsNullOrEmpty(sorting.SortBy))
            {
                var sortBy = sorting.SortBy.ToLower();
                var sortOrder = sorting.SortOrder?.ToLower() ?? "asc";
                query = sortBy switch
                {
                    "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                    "itinerarynumber" or "number" => sortOrder == "desc" ? query.OrderByDescending(x => x.ItineraryNumber) : query.OrderBy(x => x.ItineraryNumber),
                    "startdate" or "start" => sortOrder == "desc" ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
                    "enddate" or "end" => sortOrder == "desc" ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                    "status" => sortOrder == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "totalcost" or "cost" => sortOrder == "desc" ? query.OrderByDescending(x => x.TotalCost) : query.OrderBy(x => x.TotalCost),
                    "createddate" or "created" => sortOrder == "desc" ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            var totalCount = await query.CountAsync();
            var itineraries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<GetItineraryDto>();
            foreach (var itinerary in itineraries)
            {
                var dto = new GetItineraryDto
                {
                    Id = itinerary.Id,
                    GuestId = itinerary.GuestId,
                    GuestName = itinerary.Guest?.FullName ?? string.Empty,
                    PersonnelId = itinerary.PersonnelId,
                    PersonnelName = itinerary.Personnel?.FullName ?? string.Empty,
                    StartDate = itinerary.StartDate,
                    EndDate = itinerary.EndDate,
                    Status = itinerary.Status,
                    TotalCost = itinerary.TotalCost,
                    Currency = itinerary.Currency,
                    Notes = itinerary.Notes,
                    ItineraryNumber = itinerary.ItineraryNumber,
                    CreatedDate = itinerary.CreatedDate
                };

                foreach (var item in itinerary.Items.OrderBy(i => i.Order))
                {
                    var itemDto = new GetItineraryItemDto
                    {
                        Id = item.Id,
                        ItineraryId = item.ItineraryId,
                        ItemType = item.ItemType,
                        ServiceId = item.ServiceId,
                        ScheduledDateTime = item.ScheduledDateTime,
                        Order = item.Order,
                        Status = item.Status,
                        Notes = item.Notes,
                        ServiceName = await GetServiceNameAsync(item.ItemType, item.ServiceId)
                    };
                    dto.Items.Add(itemDto);
                }

                result.Add(dto);
            }

            return new PagedResult<GetItineraryDto>
            {
                Data = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ItineraryTimelineDto> GetItineraryTimeline(int itineraryId)
        {
            var itinerary = await _itineraryRepository.GetByIdAsync(itineraryId, 
                i => i.Guest, 
                i => i.Items);
            
            if (itinerary == null)
                return null!;

            var timeline = new ItineraryTimelineDto
            {
                ItineraryId = itinerary.Id,
                ItineraryNumber = itinerary.ItineraryNumber,
                GuestName = itinerary.Guest?.FullName ?? string.Empty,
                StartDate = itinerary.StartDate,
                EndDate = itinerary.EndDate,
                Status = itinerary.Status.ToString(),
                TotalCost = itinerary.TotalCost,
                Currency = itinerary.Currency
            };

            foreach (var item in itinerary.Items.OrderBy(i => i.ScheduledDateTime))
            {
                var timelineItem = new TimelineItemDto
                {
                    Id = item.Id,
                    ServiceId = item.ServiceId,
                    ItemType = item.ItemType.ToString(),
                    ItemTypeTurkish = GetItemTypeTurkishName(item.ItemType),
                    ScheduledDateTime = item.ScheduledDateTime,
                    Order = item.Order,
                    Status = item.Status,
                    Notes = item.Notes,
                    Icon = GetItemIcon(item.ItemType),
                    AdditionalInfo = new Dictionary<string, object>()
                };

                // Servis tipine göre detaylı bilgileri çek
                switch (item.ItemType)
                {
                    case ItineraryItemType.Transfer:
                        var transfer = await _transferRepository.GetByIdAsync(item.ServiceId);
                        if (transfer != null)
                        {
                            timelineItem.ServiceName = $"Transfer: {transfer.PickupAddress} → {transfer.DropoffAddress}";
                            timelineItem.Description = $"Transfer servisi";
                            timelineItem.PickupLocation = transfer.PickupAddress;
                            timelineItem.DropoffLocation = transfer.DropoffAddress;
                            timelineItem.Location = $"{transfer.PickupAddress} → {transfer.DropoffAddress}";
                            timelineItem.Price = transfer.FinalPrice;
                            timelineItem.Currency = transfer.Currency;
                            // Transfer süresi tahmini (mesafe bazlı)
                            timelineItem.AdditionalInfo["VehicleType"] = transfer.Vehicle?.Type ?? "Belirtilmemiş";
                            timelineItem.AdditionalInfo["DriverName"] = transfer.DriverName ?? transfer.ExternalDriverName ?? "Belirtilmemiş";
                        }
                        break;

                    case ItineraryItemType.CityTour:
                        var cityTour = await _cityTourRepository.GetByIdAsync(item.ServiceId);
                        if (cityTour != null)
                        {
                            timelineItem.ServiceName = $"Şehir Turu: {cityTour.City?.CityName ?? "Bilinmeyen"}";
                            timelineItem.Description = $"Şehir turu - {cityTour.Language} dilinde";
                            timelineItem.Location = cityTour.City?.CityName ?? "Bilinmeyen";
                            timelineItem.PickupLocation = cityTour.PickupLocation;
                            timelineItem.DropoffLocation = cityTour.DropoffLocation;
                            timelineItem.Price = cityTour.FinalPrice;
                            timelineItem.Currency = cityTour.Currency;
                            timelineItem.Duration = $"{cityTour.DurationHours} saat";
                            timelineItem.AdditionalInfo["Language"] = cityTour.Language ?? "TR";
                            if (cityTour.StartTime.HasValue && cityTour.EndTime.HasValue)
                            {
                                var duration = cityTour.EndTime.Value - cityTour.StartTime.Value;
                                timelineItem.AdditionalInfo["ActualDuration"] = $"{duration.TotalHours:F1} saat";
                            }
                        }
                        break;

                    case ItineraryItemType.YachtTour:
                        var yachtTour = await _yachtTourRepository.GetByIdAsync(item.ServiceId);
                        if (yachtTour != null)
                        {
                            timelineItem.ServiceName = $"Yat Turu: {yachtTour.YachtName ?? "Yat Turu"}";
                            timelineItem.Description = yachtTour.SpecialRequest ?? "Yat turu";
                            var departureLocation = yachtTour.PickupPier ?? yachtTour.City?.CityName ?? "Bilinmeyen";
                            timelineItem.Location = departureLocation;
                            timelineItem.PickupLocation = yachtTour.PickupHotel?.Address ?? yachtTour.PickupPier;
                            timelineItem.Price = yachtTour.FinalPrice;
                            timelineItem.Currency = yachtTour.Currency;
                            if (yachtTour.StartTime.HasValue && yachtTour.EndTime.HasValue)
                            {
                                var duration = yachtTour.EndTime.Value - yachtTour.StartTime.Value;
                                timelineItem.Duration = $"{duration.TotalHours:F1} saat";
                            }
                            timelineItem.AdditionalInfo["NumberOfPeople"] = yachtTour.NumberOfPeople;
                            timelineItem.AdditionalInfo["YachtName"] = yachtTour.YachtName ?? "Belirtilmemiş";
                        }
                        break;

                    case ItineraryItemType.RestaurantReservation:
                        var reservation = await _restaurantReservationRepository.GetByIdAsync(item.ServiceId,
                            r => r.Restaurant);
                        if (reservation != null)
                        {
                            timelineItem.ServiceName = $"Restoran: {reservation.Restaurant?.RestaurantName ?? "Bilinmeyen"}";
                            timelineItem.Description = $"Restoran rezervasyonu";
                            timelineItem.Location = reservation.Restaurant?.Address ?? "Bilinmeyen";
                            timelineItem.Price = null; // Restoran rezervasyonları genelde ücretsiz
                            timelineItem.AdditionalInfo["NumberOfGuests"] = reservation.NumberOfGuests;
                            timelineItem.AdditionalInfo["ReservationTime"] = reservation.ReservationTime.ToString(@"hh\:mm");
                            timelineItem.AdditionalInfo["TableNumber"] = reservation.TableNumber ?? "Atanmadı";
                        }
                        break;
                }

                timeline.Items.Add(timelineItem);
            }

            return timeline;
        }

        private string GetItemIcon(ItineraryItemType itemType)
        {
            return itemType switch
            {
                ItineraryItemType.Transfer => "car",
                ItineraryItemType.CityTour => "map",
                ItineraryItemType.YachtTour => "ship",
                ItineraryItemType.RestaurantReservation => "restaurant",
                _ => "event"
            };
        }

        public async Task<ServiceMessage> AddItineraryItem(int itineraryId, AddItineraryItemDto item)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var itinerary = await _itineraryRepository.GetByIdAsync(itineraryId);
                if (itinerary == null)
                    return new ServiceMessage { IsSuccess = false, Message = "İtinerary bulunamadı." };

                var itemEntity = new ItineraryItemEntity
                {
                    ItineraryId = itineraryId,
                    ItemType = item.ItemType,
                    ServiceId = item.ServiceId,
                    ScheduledDateTime = item.ScheduledDateTime,
                    Order = item.Order,
                    Status = item.Status,
                    Notes = item.Notes
                };

                await _itineraryItemRepository.AddAsync(itemEntity);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                itinerary.TotalCost = await CalculateItineraryTotalCost(itineraryId);
                await _itineraryRepository.UpdateAsync(itinerary);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"İtinerary item eklendi: {itineraryId}");
                return new ServiceMessage { IsSuccess = true, Message = "Item başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary item eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Item eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateItineraryItem(int itineraryId, int itemId, AddItineraryItemDto item)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var itemEntity = await _itineraryItemRepository.GetByIdAsync(itemId);
                if (itemEntity == null || itemEntity.ItineraryId != itineraryId)
                    return new ServiceMessage { IsSuccess = false, Message = "Item bulunamadı." };

                itemEntity.ItemType = item.ItemType;
                itemEntity.ServiceId = item.ServiceId;
                itemEntity.ScheduledDateTime = item.ScheduledDateTime;
                itemEntity.Order = item.Order;
                itemEntity.Status = item.Status;
                itemEntity.Notes = item.Notes;

                await _itineraryItemRepository.UpdateAsync(itemEntity);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var itinerary = await _itineraryRepository.GetByIdAsync(itineraryId);
                if (itinerary != null)
                {
                    itinerary.TotalCost = await CalculateItineraryTotalCost(itineraryId);
                    await _itineraryRepository.UpdateAsync(itinerary);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"İtinerary item güncellendi: {itemId}");
                return new ServiceMessage { IsSuccess = true, Message = "Item başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary item güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Item güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeleteItineraryItem(int itineraryId, int itemId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var itemEntity = await _itineraryItemRepository.GetByIdAsync(itemId);
                if (itemEntity == null || itemEntity.ItineraryId != itineraryId)
                    return new ServiceMessage { IsSuccess = false, Message = "Item bulunamadı." };

                await _itineraryItemRepository.DeleteAsync(itemEntity);
                await _unitOfWork.SaveChangesAsync();

                // Toplam maliyeti güncelle
                var itinerary = await _itineraryRepository.GetByIdAsync(itineraryId);
                if (itinerary != null)
                {
                    itinerary.TotalCost = await CalculateItineraryTotalCost(itineraryId);
                    await _itineraryRepository.UpdateAsync(itinerary);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"İtinerary item silindi: {itemId}");
                return new ServiceMessage { IsSuccess = true, Message = "Item başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary item silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Item silinirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateItineraryStatus(int id, ItineraryStatus status)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var itinerary = await _itineraryRepository.GetByIdAsync(id);
                if (itinerary == null)
                    return new ServiceMessage { IsSuccess = false, Message = "İtinerary bulunamadı." };

                itinerary.Status = status;
                await _itineraryRepository.UpdateAsync(itinerary);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"İtinerary durumu güncellendi: {id} -> {status}");
                return new ServiceMessage { IsSuccess = true, Message = "İtinerary durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"İtinerary durumu güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Durum güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<decimal> CalculateItineraryTotalCost(int itineraryId)
        {
            var items = await _itineraryItemRepository.GetAll(x => x.ItineraryId == itineraryId).ToListAsync();
            decimal totalCost = 0;

            foreach (var item in items)
            {
                switch (item.ItemType)
                {
                    case ItineraryItemType.Transfer:
                        var transfer = await _transferRepository.GetByIdAsync(item.ServiceId);
                        if (transfer != null)
                            totalCost += transfer.FinalPrice;
                        break;

                    case ItineraryItemType.CityTour:
                        var cityTour = await _cityTourRepository.GetByIdAsync(item.ServiceId);
                        if (cityTour != null)
                            totalCost += cityTour.FinalPrice;
                        break;

                    case ItineraryItemType.YachtTour:
                        var yachtTour = await _yachtTourRepository.GetByIdAsync(item.ServiceId);
                        if (yachtTour != null)
                            totalCost += yachtTour.FinalPrice;
                        break;

                    case ItineraryItemType.RestaurantReservation:
                        // Restoran rezervasyonları genellikle ücretsiz veya ayrı fiyatlandırılır
                        // Şimdilik 0 olarak bırakıyoruz
                        break;
                }
            }

            return totalCost;
        }

        private async Task<string> GetServiceNameAsync(ItineraryItemType itemType, int serviceId)
        {
            return itemType switch
            {
                ItineraryItemType.Transfer => await GetTransferNameAsync(serviceId),
                ItineraryItemType.CityTour => await GetCityTourNameAsync(serviceId),
                ItineraryItemType.YachtTour => await GetYachtTourNameAsync(serviceId),
                ItineraryItemType.RestaurantReservation => await GetRestaurantReservationNameAsync(serviceId),
                _ => "Bilinmeyen Servis"
            };
        }

        private async Task<string> GetTransferNameAsync(int transferId)
        {
            var transfer = await _transferRepository.GetByIdAsync(transferId);
            return transfer != null ? $"{transfer.PickupAddress} → {transfer.DropoffAddress}" : "Transfer";
        }

        private async Task<string> GetCityTourNameAsync(int cityTourId)
        {
            var cityTour = await _cityTourRepository.GetAll(x => x.Id == cityTourId, x => x.City).FirstOrDefaultAsync();
            return cityTour != null ? $"Şehir Turu - {cityTour.City?.CityName ?? "Bilinmeyen"}" : "Şehir Turu";
        }

        private async Task<string> GetYachtTourNameAsync(int yachtTourId)
        {
            var yachtTour = await _yachtTourRepository.GetAll(x => x.Id == yachtTourId, x => x.City).FirstOrDefaultAsync();
            return yachtTour != null ? $"Yat Turu - {yachtTour.City?.CityName ?? "Bilinmeyen"}" : "Yat Turu";
        }

        private async Task<string> GetRestaurantReservationNameAsync(int reservationId)
        {
            var reservation = await _restaurantReservationRepository.GetAll(x => x.Id == reservationId, x => x.Restaurant).FirstOrDefaultAsync();
            return reservation != null ? $"Restoran - {reservation.Restaurant?.RestaurantName ?? "Bilinmeyen"}" : "Restoran Rezervasyonu";
        }

        private string GetItemTypeTurkishName(ItineraryItemType itemType)
        {
            return itemType switch
            {
                ItineraryItemType.Transfer => "Transfer",
                ItineraryItemType.CityTour => "Şehir Turu",
                ItineraryItemType.YachtTour => "Yat Turu",
                ItineraryItemType.RestaurantReservation => "Restoran Rezervasyonu",
                _ => "Diğer"
            };
        }
    }
}

