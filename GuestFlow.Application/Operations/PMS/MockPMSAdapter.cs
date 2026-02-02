// Copyright (c) 2025-2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// Mock PMS Adapter - Development ve test ortamları için gerçek PMS bağlantısı olmadan çalışır.
    /// Randomize veri üretir ve webhook simülasyonu yapabilir.
    /// </summary>
    public class MockPMSAdapter : BasePMSAdapter
    {
        private readonly Random _random = new();
        
        // Sample data pools
        private static readonly string[] _firstNames = { "John", "Emma", "Lucas", "Olivia", "Ahmet", "Ayşe", "Mehmet", "Zeynep", "Hans", "Marie", "Dmitri", "Natasha" };
        private static readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Yılmaz", "Kaya", "Demir", "Öztürk", "Müller", "Schmidt", "Ivanov", "Petrov" };
        private static readonly string[] _nationalities = { "USA", "UK", "Turkey", "Germany", "Russia", "France", "Italy", "Spain", "Netherlands", "Japan" };
        private static readonly string[] _roomTypes = { "Standard", "Superior", "Deluxe", "Suite", "Junior Suite", "Presidential Suite" };
        private static readonly string[] _roomNumbers = { "101", "102", "201", "202", "301", "302", "401", "402", "501", "502" };
        private static readonly string[] _roomStatuses = { "Available", "Occupied", "OutOfOrder", "Maintenance" };
        private static readonly string[] _reservationStatuses = { "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };

        public MockPMSAdapter(
            PMSIntegration integration,
            IHttpClientFactory httpClientFactory,
            ILogger<MockPMSAdapter> logger)
            : base(integration, httpClientFactory, logger)
        {
            _logger.LogInformation("MockPMSAdapter initialized for integration: {IntegrationId}", integration.Id);
        }

        protected override void AddAuthenticationHeaders(HttpClient client)
        {
            // Mock - no real authentication needed
            _logger.LogDebug("MockPMSAdapter: Skipping authentication headers");
        }

        public override Task<bool> RefreshAccessTokenAsync()
        {
            _logger.LogDebug("MockPMSAdapter: Simulating token refresh");
            return Task.FromResult(true);
        }

        public override Task<bool> TestConnectionAsync()
        {
            _logger.LogInformation("MockPMSAdapter: Connection test successful (mocked)");
            return Task.FromResult(true);
        }

        public override Task<PMSGuestProfile?> GetGuestProfileAsync(string pmsGuestId)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock guest profile for ID: {GuestId}", pmsGuestId);
            
            var guest = GenerateRandomGuestProfile(pmsGuestId);
            return Task.FromResult<PMSGuestProfile?>(guest);
        }

        public override Task<List<PMSGuestProfile>> GetGuestsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock guest list");
            
            var count = _random.Next(5, 15);
            var guests = new List<PMSGuestProfile>();
            
            for (int i = 0; i < count; i++)
            {
                guests.Add(GenerateRandomGuestProfile($"MOCK-G-{1000 + i}"));
            }
            
            return Task.FromResult(guests);
        }

        public override Task<PMSReservation?> GetReservationAsync(string pmsReservationId)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock reservation for ID: {ReservationId}", pmsReservationId);
            
            var reservation = GenerateRandomReservation(pmsReservationId);
            return Task.FromResult<PMSReservation?>(reservation);
        }

        public override Task<List<PMSReservation>> GetReservationsAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock reservations for date range: {Start} - {End}", startDate, endDate);
            
            var count = _random.Next(3, 10);
            var reservations = new List<PMSReservation>();
            
            for (int i = 0; i < count; i++)
            {
                var reservation = GenerateRandomReservation($"MOCK-R-{2000 + i}");
                // Adjust dates to be within the requested range
                var dayOffset = _random.Next(0, (int)(endDate - startDate).TotalDays);
                reservation.CheckInDate = startDate.AddDays(dayOffset);
                reservation.CheckOutDate = reservation.CheckInDate.AddDays(_random.Next(1, 7));
                reservations.Add(reservation);
            }
            
            return Task.FromResult(reservations);
        }

        public override Task<PMSRoomStatus?> GetRoomStatusAsync(string roomNumber)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock room status for: {RoomNumber}", roomNumber);
            
            var status = GenerateRandomRoomStatus(roomNumber);
            return Task.FromResult<PMSRoomStatus?>(status);
        }

        public override Task<List<PMSRoomStatus>> GetRoomsStatusAsync(DateTime? date = null)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock rooms status list");
            
            var rooms = new List<PMSRoomStatus>();
            foreach (var roomNumber in _roomNumbers)
            {
                rooms.Add(GenerateRandomRoomStatus(roomNumber));
            }
            
            return Task.FromResult(rooms);
        }

        public override Task<PMSFolio?> GetFolioAsync(string reservationId)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock folio for reservation: {ReservationId}", reservationId);
            
            var folio = GenerateRandomFolio(reservationId);
            return Task.FromResult<PMSFolio?>(folio);
        }

        public override Task<List<PMSFolio>> GetFoliosAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock folios for date range: {Start} - {End}", startDate, endDate);
            
            var count = _random.Next(3, 8);
            var folios = new List<PMSFolio>();
            
            for (int i = 0; i < count; i++)
            {
                var folio = GenerateRandomFolio($"MOCK-R-{2000 + i}");
                folio.FolioDate = startDate.AddDays(_random.Next(0, (int)(endDate - startDate).TotalDays));
                folios.Add(folio);
            }
            
            return Task.FromResult(folios);
        }

        public override Task<List<PMSRoomType>> GetRoomTypesAsync()
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock room types");

            var roomTypes = new List<PMSRoomType>();
            int idCounter = 1;

            foreach (var typeName in _roomTypes)
            {
                // Simple logic for base price
                decimal basePrice = 1000m + (idCounter * 500m); 

                roomTypes.Add(new PMSRoomType
                {
                    RoomTypeId = idCounter.ToString(),
                    Name = typeName,
                    BasePrice = basePrice,
                    Currency = "TRY",
                    TotalInventory = _random.Next(5, 20)
                });
                idCounter++;
            }

            return Task.FromResult(roomTypes);
        }

        #region Private Helper Methods

        private PMSGuestProfile GenerateRandomGuestProfile(string guestId)
        {
            var firstName = _firstNames[_random.Next(_firstNames.Length)];
            var lastName = _lastNames[_random.Next(_lastNames.Length)];
            var checkIn = DateTime.Now.AddDays(-_random.Next(0, 5));
            var checkOut = checkIn.AddDays(_random.Next(1, 7));

            return new PMSGuestProfile
            {
                PMSGuestId = guestId,
                FullName = $"{firstName} {lastName}",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
                PhoneNumber = $"+90 555 {_random.Next(100, 999)} {_random.Next(10, 99)} {_random.Next(10, 99)}",
                Nationality = _nationalities[_random.Next(_nationalities.Length)],
                GuestCode = $"GC-{_random.Next(10000, 99999)}",
                IsVIP = _random.Next(10) < 2, // 20% VIP
                RoomNumber = _roomNumbers[_random.Next(_roomNumbers.Length)],
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                SpecialRequests = _random.Next(10) < 3 ? "Late check-out requested" : null,
                Preferences = _random.Next(10) < 3 ? "{\"dietaryRestrictions\":\"vegetarian\",\"roomPreference\":\"high-floor\"}" : null,
                LastUpdatedAt = DateTime.Now
            };
        }

        private PMSReservation GenerateRandomReservation(string reservationId)
        {
            var firstName = _firstNames[_random.Next(_firstNames.Length)];
            var lastName = _lastNames[_random.Next(_lastNames.Length)];
            var checkIn = DateTime.Now.AddDays(_random.Next(-5, 10));
            var checkOut = checkIn.AddDays(_random.Next(1, 7));

            return new PMSReservation
            {
                PMSReservationId = reservationId,
                PMSGuestId = $"MOCK-G-{_random.Next(1000, 1999)}",
                GuestName = $"{firstName} {lastName}",
                GuestEmail = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
                GuestPhone = $"+90 555 {_random.Next(100, 999)} {_random.Next(10, 99)} {_random.Next(10, 99)}",
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                RoomNumber = _roomNumbers[_random.Next(_roomNumbers.Length)],
                RoomType = _roomTypes[_random.Next(_roomTypes.Length)],
                GuestCount = _random.Next(1, 5),
                Status = _reservationStatuses[_random.Next(_reservationStatuses.Length)],
                TotalAmount = _random.Next(500, 5000),
                Currency = "EUR",
                CreatedAt = DateTime.Now.AddDays(-_random.Next(1, 30)),
                LastModifiedAt = DateTime.Now
            };
        }

        private PMSRoomStatus GenerateRandomRoomStatus(string roomNumber)
        {
            var status = _roomStatuses[_random.Next(_roomStatuses.Length)];
            var isOccupied = status == "Occupied";

            return new PMSRoomStatus
            {
                RoomNumber = roomNumber,
                RoomType = _roomTypes[_random.Next(_roomTypes.Length)],
                Status = status,
                GuestName = isOccupied ? $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}" : null,
                PMSGuestId = isOccupied ? $"MOCK-G-{_random.Next(1000, 1999)}" : null,
                CheckInDate = isOccupied ? DateTime.Now.AddDays(-_random.Next(0, 5)) : null,
                CheckOutDate = isOccupied ? DateTime.Now.AddDays(_random.Next(1, 5)) : null,
                LastUpdatedAt = DateTime.Now
            };
        }

        private PMSFolio GenerateRandomFolio(string reservationId)
        {
            var totalAmount = _random.Next(500, 5000);
            var paidAmount = _random.Next(0, totalAmount);

            var folio = new PMSFolio
            {
                FolioId = $"MOCK-F-{_random.Next(3000, 9999)}",
                ReservationId = reservationId,
                GuestName = $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                Balance = totalAmount - paidAmount,
                Currency = "EUR",
                Status = paidAmount >= totalAmount ? "Settled" : "Open",
                FolioDate = DateTime.Now,
                Items = new List<PMSFolioItem>()
            };

            // Add random folio items
            var itemCount = _random.Next(2, 6);
            string[] itemDescriptions = { "Room Charge", "Minibar", "Room Service", "Restaurant", "Spa", "Laundry", "Parking" };
            
            for (int i = 0; i < itemCount; i++)
            {
                folio.Items.Add(new PMSFolioItem
                {
                    Description = itemDescriptions[_random.Next(itemDescriptions.Length)],
                    Amount = _random.Next(20, 500),
                    Category = "Service",
                    TransactionDate = DateTime.Now.AddDays(-_random.Next(0, 3))
                });
            }

            return folio;
        }

        #endregion
    }
}
