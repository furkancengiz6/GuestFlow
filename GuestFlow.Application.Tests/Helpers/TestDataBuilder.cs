using GuestFlow.Domain.Entities.Core;
using System;

namespace GuestFlow.Application.Tests.Helpers;

/// <summary>
/// Builder pattern for creating test data entities
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a GuestEntity with default test values
    /// </summary>
    public static GuestEntity CreateGuest(
        int? id = null,
        string? fullName = null,
        string? email = null,
        string? phoneNumber = null,
        string? nationality = null,
        bool? isSpecialGuest = null,
        string? guestCode = null)
    {
        return new GuestEntity
        {
            Id = id ?? 1,
            FullName = fullName ?? "Test Guest",
            Email = email ?? "test@example.com",
            PhoneNumber = phoneNumber ?? "+905551234567",
            Nationality = nationality ?? "TR",
            IsSpecialGuest = isSpecialGuest ?? false,
            GuestCode = guestCode ?? "GUEST001",
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Creates a PersonnelEntity with default test values
    /// </summary>
    public static PersonnelEntity CreatePersonnel(
        int? id = null,
        string? fullName = null,
        string? email = null,
        string? password = null)
    {
        return new PersonnelEntity
        {
            Id = id ?? 1,
            FullName = fullName ?? "Test Personnel",
            Email = email ?? "personnel@example.com",
            Password = password ?? "HashedPassword123",
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Creates a CityEntity with default test values
    /// </summary>
    public static CityEntity CreateCity(
        int? id = null,
        string? cityName = null,
        string? country = null)
    {
        return new CityEntity
        {
            Id = id ?? 1,
            CityName = cityName ?? "Test City",
            Country = country ?? "Turkey",
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Creates a VehicleEntity with default test values
    /// </summary>
    public static VehicleEntity CreateVehicle(
        int? id = null,
        string? plateNumber = null,
        string? type = null,
        int? capacity = null,
        decimal? dailyPrice = null)
    {
        return new VehicleEntity
        {
            Id = id ?? 1,
            PlateNumber = plateNumber ?? "34ABC123",
            Type = type ?? "Sedan",
            Capacity = capacity ?? 4,
            DailyPrice = dailyPrice ?? 100.00m,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Creates an AirportEntity with default test values
    /// </summary>
    public static AirportEntity CreateAirport(
        int? id = null,
        string? name = null,
        string? code = null,
        int? cityId = null)
    {
        return new AirportEntity
        {
            Id = id ?? 1,
            Name = name ?? "Test Airport",
            Code = code ?? "TEST",
            CityId = cityId ?? 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }
}

