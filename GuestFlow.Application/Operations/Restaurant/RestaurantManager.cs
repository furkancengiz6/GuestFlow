using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Restaurant.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Restaurant
{
    public class RestaurantManager : IRestaurantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<RestaurantEntity> _restaurantRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly ILogger<RestaurantManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public RestaurantManager(
            IUnitOfWork unitOfWork,
            IRepository<RestaurantEntity> restaurantRepository,
            IRepository<CityEntity> cityRepository,
            ILogger<RestaurantManager> logger,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _restaurantRepository = restaurantRepository;
            _cityRepository = cityRepository;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<ServiceMessage> AddRestaurant(AddRestaurantDto restaurant)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cityExists = await _cityRepository.GetAll(x => x.Id == restaurant.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen şehir bulunamadı." };

                var restaurantEntity = _mapper.Map<RestaurantEntity>(restaurant);
                await _restaurantRepository.AddAsync(restaurantEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran eklendi: {restaurant.RestaurantName}");
                return new ServiceMessage { IsSuccess = true, Message = "Restoran başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateRestaurant(UpdateRestaurantDto restaurant)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var restaurantEntity = await _restaurantRepository.GetByIdAsync(restaurant.Id);
                if (restaurantEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Restoran bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == restaurant.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen şehir bulunamadı." };

                _mapper.Map(restaurant, restaurantEntity);
                await _restaurantRepository.UpdateAsync(restaurantEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran güncellendi: {restaurant.RestaurantName}");
                return new ServiceMessage { IsSuccess = true, Message = "Restoran başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeleteRestaurant(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var restaurantEntity = await _restaurantRepository.GetByIdAsync(id);
                if (restaurantEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Restoran bulunamadı." };

                await _restaurantRepository.DeleteAsync(restaurantEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Restoran başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran silinirken hata: {ex.Message}" };
            }
        }

        public async Task<GetRestaurantDto> GetRestaurantById(int id)
        {
            var restaurant = await _restaurantRepository.GetAll(x => x.Id == id, x => x.City).FirstOrDefaultAsync();
            if (restaurant == null)
                return null!;

            var dto = _mapper.Map<GetRestaurantDto>(restaurant);
            if (restaurant.City != null)
                dto.CityName = restaurant.City.CityName;

            return dto;
        }

        public async Task<List<GetRestaurantDto>> GetRestaurants()
        {
            var restaurants = await _restaurantRepository.GetAll(x => x.IsActive, x => x.City).ToListAsync();
            return restaurants.Select(r =>
            {
                var dto = _mapper.Map<GetRestaurantDto>(r);
                if (r.City != null)
                    dto.CityName = r.City.CityName;
                return dto;
            }).ToList();
        }

        public async Task<PagedResult<GetRestaurantDto>> GetRestaurantsPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            var query = _restaurantRepository.GetAll(null, x => x.City);

            if (sorting != null && !string.IsNullOrEmpty(sorting.SortBy))
            {
                var sortBy = sorting.SortBy.ToLower();
                var sortOrder = sorting.SortOrder?.ToLower() ?? "asc";
                query = sortBy switch
                {
                    "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                    "restaurantname" or "name" => sortOrder == "desc" ? query.OrderByDescending(x => x.RestaurantName) : query.OrderBy(x => x.RestaurantName),
                    "cityid" or "city" => sortOrder == "desc" ? query.OrderByDescending(x => x.CityId) : query.OrderBy(x => x.CityId),
                    "capacity" => sortOrder == "desc" ? query.OrderByDescending(x => x.Capacity) : query.OrderBy(x => x.Capacity),
                    "createddate" or "created" => sortOrder == "desc" ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                    _ => query.OrderBy(x => x.RestaurantName)
                };
            }
            else
            {
                query = query.OrderBy(x => x.RestaurantName);
            }

            var totalCount = await query.CountAsync();
            var restaurants = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = restaurants.Select(r =>
            {
                var dto = _mapper.Map<GetRestaurantDto>(r);
                if (r.City != null)
                    dto.CityName = r.City.CityName;
                return dto;
            }).ToList();

            return new PagedResult<GetRestaurantDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<GetRestaurantDto>> GetRestaurantsByCityId(int cityId)
        {
            var restaurants = await _restaurantRepository.GetAll(x => x.CityId == cityId && x.IsActive, x => x.City).ToListAsync();
            return restaurants.Select(r =>
            {
                var dto = _mapper.Map<GetRestaurantDto>(r);
                if (r.City != null)
                    dto.CityName = r.City.CityName;
                return dto;
            }).ToList();
        }
    }
}

