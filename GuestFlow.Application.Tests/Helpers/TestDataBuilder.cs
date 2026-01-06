using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
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

    /// <summary>
    /// Creates a TransferEntity with default test values
    /// </summary>
    public static TransferEntity CreateTransferEntity(
        int? id = null,
        int? guestId = null,
        DateTime? transferDate = null,
        string? status = null,
        decimal? price = null,
        int? driverId = null,
        int? vehicleId = null,
        TransferPriority? priority = null,
        bool? isVip = null,
        int? groupSize = null)
    {
        return new TransferEntity
        {
            Id = id ?? 1,
            GuestId = guestId ?? 1,
            TransferDate = transferDate ?? DateTime.UtcNow.AddDays(1),
            PickupAddress = "Test Pickup Address",
            DropoffAddress = "Test Dropoff Address",
            Status = status ?? "Pending",
            Price = price ?? 100.00m,
            FinalPrice = price ?? 100.00m,
            Currency = "TRY",
            DriverId = driverId,
            VehicleId = vehicleId,
            Priority = priority ?? TransferPriority.Normal,
            IsVip = isVip ?? false,
            GroupSize = groupSize ?? 2,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>
    /// Creates an AddTransferDto with default test values
    /// </summary>
    public static AddTransferDto CreateAddTransferDto(
        int? guestId = null,
        DateTime? transferDate = null,
        string? pickupAddress = null,
        string? dropoffAddress = null,
        decimal? price = null,
        int? driverId = null,
        int? vehicleId = null,
        TransferPriority? priority = null,
        bool? isVip = null,
        int? groupSize = null,
        string? specialHandlingNotes = null)
    {
        return new AddTransferDto
        {
            GuestId = guestId ?? 1,
            TransferDate = transferDate ?? DateTime.UtcNow.AddDays(1),
            PickupAddress = pickupAddress ?? "Test Pickup Address",
            DropoffAddress = dropoffAddress ?? "Test Dropoff Address",
            Price = price ?? 100.00m,
            Currency = "TRY",
            DriverId = driverId,
            VehicleId = vehicleId,
            Priority = priority ?? TransferPriority.Normal,
            IsVip = isVip ?? false,
            GroupSize = groupSize ?? 2,
            SpecialHandlingNotes = specialHandlingNotes,
            CreateInvoice = false
        };
    }

    /// <summary>
    /// Creates an UpdateTransferDto with default test values
    /// </summary>
    public static UpdateTransferDto CreateUpdateTransferDto(
        DateTime? transferDate = null,
        string? pickupAddress = null,
        string? dropoffAddress = null,
        decimal? price = null,
        int? driverId = null,
        int? vehicleId = null,
        TransferPriority? priority = null,
        bool? isVip = null,
        int? groupSize = null)
    {
        return new UpdateTransferDto
        {
            TransferDate = transferDate ?? DateTime.UtcNow.AddDays(1),
            PickupAddress = pickupAddress ?? "Updated Pickup Address",
            DropoffAddress = dropoffAddress ?? "Updated Dropoff Address",
            Price = price ?? 150.00m,
            Currency = "TRY",
            DriverId = driverId,
            VehicleId = vehicleId,
            Priority = priority ?? TransferPriority.Normal,
            IsVip = isVip ?? false,
            GroupSize = groupSize ?? 2
        };
    }
}

