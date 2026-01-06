using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Hotel.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Hotel
{
    public class HotelManager : IHotelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<HotelEntity> _hotelRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly ILogger<HotelManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public HotelManager(
            IUnitOfWork unitOfWork,
            IRepository<HotelEntity> hotelRepository,
            IRepository<CityEntity> cityRepository,
            ILogger<HotelManager> logger,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _hotelRepository = hotelRepository;
            _cityRepository = cityRepository;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<ServiceMessage> AddHotel(AddHotelDto hotel)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cityExists = await _cityRepository.GetAll(x => x.Id == hotel.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen şehir bulunamadı." };

                var hotelEntity = _mapper.Map<HotelEntity>(hotel);
                await _hotelRepository.AddAsync(hotelEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Otel eklendi: {hotel.HotelName}");
                return new ServiceMessage { IsSuccess = true, Message = "Otel başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Otel eklenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Otel eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateHotel(UpdateHotelDto hotel)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var hotelEntity = await _hotelRepository.GetByIdAsync(hotel.Id);
                if (hotelEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Otel bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == hotel.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen şehir bulunamadı." };

                _mapper.Map(hotel, hotelEntity);
                await _hotelRepository.UpdateAsync(hotelEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Otel güncellendi: {hotel.HotelName}");
                return new ServiceMessage { IsSuccess = true, Message = "Otel başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Otel güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Otel güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeleteHotel(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var hotelEntity = await _hotelRepository.GetByIdAsync(id);
                if (hotelEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Otel bulunamadı." };

                await _hotelRepository.DeleteAsync(hotelEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Otel silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Otel başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Otel silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Otel silinirken hata: {ex.Message}" };
            }
        }

        public async Task<GetHotelDto> GetHotelById(int id)
        {
            var hotel = await _hotelRepository.GetAll(x => x.Id == id, x => x.City).FirstOrDefaultAsync();
            if (hotel == null)
                return null!;

            var dto = _mapper.Map<GetHotelDto>(hotel);
            if (hotel.City != null)
                dto.CityName = hotel.City.CityName;

            return dto;
        }

        public async Task<List<GetHotelDto>> GetHotels()
        {
            var hotels = await _hotelRepository.GetAll(x => x.IsActive, x => x.City).ToListAsync();
            return hotels.Select(h =>
            {
                var dto = _mapper.Map<GetHotelDto>(h);
                if (h.City != null)
                    dto.CityName = h.City.CityName;
                return dto;
            }).ToList();
        }

        public async Task<PagedResult<GetHotelDto>> GetHotelsPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            var query = _hotelRepository.GetAll(null, x => x.City);

            if (sorting != null && !string.IsNullOrEmpty(sorting.SortBy))
            {
                var sortBy = sorting.SortBy.ToLower();
                var sortOrder = sorting.SortOrder?.ToLower() ?? "asc";
                query = sortBy switch
                {
                    "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                    "hotelname" or "name" => sortOrder == "desc" ? query.OrderByDescending(x => x.HotelName) : query.OrderBy(x => x.HotelName),
                    "cityid" or "city" => sortOrder == "desc" ? query.OrderByDescending(x => x.CityId) : query.OrderBy(x => x.CityId),
                    "starrating" or "rating" => sortOrder == "desc" ? query.OrderByDescending(x => x.StarRating) : query.OrderBy(x => x.StarRating),
                    "createddate" or "created" => sortOrder == "desc" ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                    _ => query.OrderBy(x => x.HotelName)
                };
            }
            else
            {
                query = query.OrderBy(x => x.HotelName);
            }

            var totalCount = await query.CountAsync();
            var hotels = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = hotels.Select(h =>
            {
                var dto = _mapper.Map<GetHotelDto>(h);
                if (h.City != null)
                    dto.CityName = h.City.CityName;
                return dto;
            }).ToList();

            return new PagedResult<GetHotelDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<GetHotelDto>> GetHotelsByCityId(int cityId)
        {
            var hotels = await _hotelRepository.GetAll(x => x.CityId == cityId && x.IsActive, x => x.City).ToListAsync();
            return hotels.Select(h =>
            {
                var dto = _mapper.Map<GetHotelDto>(h);
                if (h.City != null)
                    dto.CityName = h.City.CityName;
                return dto;
            }).ToList();
        }
    }
}

