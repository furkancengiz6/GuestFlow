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

        // Deterministic Data Store
        private static readonly Dictionary<string, PMSGuestProfile> _knownGuests = new();

        static MockPMSAdapter()
        {
            InitializeKnownGuests();
        }

        private static void InitializeKnownGuests()
        {
            // 1. VIP Guest with rich preferences
            var vipGuest = new PMSGuestProfile
            {
                PMSGuestId = "MOCK-G-VIP-001",
                FullName = "Alexander The Great",
                Email = "alex.great@history.com",
                PhoneNumber = "+90 555 111 2233",
                Nationality = "Macedonia",
                GuestCode = "VIP-001",
                IsVIP = true,
                RoomNumber = "501",
                CheckInDate = DateTime.Today.AddDays(-2),
                CheckOutDate = DateTime.Today.AddDays(5),
                SpecialRequests = "Extra pillows, Champagne on arrival",
                Preferences = "{\"dietaryPreferences\":\"Gluten-Free\",\"preferredRoomType\":\"High Floor\",\"bedPreference\":\"King\",\"pillowPreference\":\"Soft\",\"newspaper\":\"Financial Times\",\"interests\":\"History, Finance\"}",
                LastUpdatedAt = DateTime.Now
            };
            _knownGuests.Add(vipGuest.PMSGuestId, vipGuest);

            // 2. Regular Repeat Guest
            var repeatGuest = new PMSGuestProfile
            {
                PMSGuestId = "MOCK-G-REG-002",
                FullName = "Sarah Connor",
                Email = "sarah.connor@sky.net",
                PhoneNumber = "+1 555 999 8877",
                Nationality = "USA",
                GuestCode = "REG-002",
                IsVIP = false,
                RoomNumber = "101",
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(3),
                SpecialRequests = "Quiet room please",
                Preferences = "{\"roomPreference\":\"Quiet\",\"prefersEmail\":true}",
                LastUpdatedAt = DateTime.Now
            };
            _knownGuests.Add(repeatGuest.PMSGuestId, repeatGuest);
        }

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
            _logger.LogDebug("MockPMSAdapter: Fetching guest profile for ID: {GuestId}", pmsGuestId);
            
            if (_knownGuests.TryGetValue(pmsGuestId, out var guest))
            {
                return Task.FromResult<PMSGuestProfile?>(guest);
            }

            var randomGuest = GenerateRandomGuestProfile(pmsGuestId);
            return Task.FromResult<PMSGuestProfile?>(randomGuest);
        }

        public override Task<List<PMSGuestProfile>> GetGuestsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock guest list");
            
            var guests = new List<PMSGuestProfile>();
            
            // Always include known guests
            guests.AddRange(_knownGuests.Values);

            // Add some random guests
            var count = _random.Next(3, 8);
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
            
            var reservations = new List<PMSReservation>();

            // Generate history for known guests
            foreach (var knownGuest in _knownGuests.Values)
            {
                // Past reservation 1
                reservations.Add(new PMSReservation
                {
                    PMSReservationId = $"MOCK-R-{knownGuest.PMSGuestId}-HIST-1",
                    PMSGuestId = knownGuest.PMSGuestId,
                    GuestName = knownGuest.FullName,
                    GuestEmail = knownGuest.Email,
                    CheckInDate = DateTime.Today.AddMonths(-6), // 6 months ago
                    CheckOutDate = DateTime.Today.AddMonths(-6).AddDays(5),
                    RoomNumber = "PreviousRoom",
                    RoomType = "Deluxe",
                    GuestCount = 2,
                    Status = "CheckedOut",
                    TotalAmount = 1500,
                    Currency = "EUR",
                    CreatedAt = DateTime.Today.AddMonths(-7),
                    LastModifiedAt = DateTime.Today.AddMonths(-6)
                });

                // Current reservation (if checked in)
                if (knownGuest.CheckInDate.HasValue)
                {
                    reservations.Add(new PMSReservation
                    {
                        PMSReservationId = $"MOCK-R-{knownGuest.PMSGuestId}-CURR",
                        PMSGuestId = knownGuest.PMSGuestId,
                        GuestName = knownGuest.FullName,
                        GuestEmail = knownGuest.Email,
                        CheckInDate = knownGuest.CheckInDate.Value,
                        CheckOutDate = knownGuest.CheckOutDate ?? knownGuest.CheckInDate.Value.AddDays(1),
                        RoomNumber = knownGuest.RoomNumber,
                        RoomType = "Suite",
                        GuestCount = 1,
                        Status = "CheckedIn",
                        TotalAmount = 2000,
                        Currency = "EUR",
                        CreatedAt = DateTime.Today.AddDays(-10),
                        LastModifiedAt = DateTime.Today
                    });
                }
            }
            
            // Add random reservations
            var count = _random.Next(3, 10);
            for (int i = 0; i < count; i++)
            {
                var reservation = GenerateRandomReservation($"MOCK-R-{2000 + i}");
                var dayOffset = _random.Next(0, (int)((endDate - startDate).TotalDays <= 0 ? 1 : (endDate - startDate).TotalDays));
                reservation.CheckInDate = startDate.AddDays(dayOffset);
                reservation.CheckOutDate = reservation.CheckInDate.AddDays(_random.Next(1, 7));
                reservations.Add(reservation);
            }
            
            return Task.FromResult(reservations);
        }

        public override Task<PMSRoomStatus?> GetRoomStatusAsync(string roomNumber)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock room status for: {RoomNumber}", roomNumber);
            
            // Check if any known guest is in this room
            var knownGuest = _knownGuests.Values.FirstOrDefault(g => g.RoomNumber == roomNumber);
            if (knownGuest != null)
            {
                return Task.FromResult<PMSRoomStatus?>(new PMSRoomStatus
                {
                    RoomNumber = roomNumber,
                    RoomType = "Suite",
                    Status = "Occupied",
                    GuestName = knownGuest.FullName,
                    PMSGuestId = knownGuest.PMSGuestId,
                    CheckInDate = knownGuest.CheckInDate,
                    CheckOutDate = knownGuest.CheckOutDate,
                    LastUpdatedAt = DateTime.Now
                });
            }

            var status = GenerateRandomRoomStatus(roomNumber);
            return Task.FromResult<PMSRoomStatus?>(status);
        }

        public override Task<List<PMSRoomStatus>> GetRoomsStatusAsync(DateTime? date = null)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock rooms status list");
            
            var rooms = new List<PMSRoomStatus>();
            foreach (var roomNumber in _roomNumbers)
            {
                // Key logic: Ensure consistency with Known Guests
                var knownGuest = _knownGuests.Values.FirstOrDefault(g => g.RoomNumber == roomNumber);
                if (knownGuest != null)
                {
                    rooms.Add(new PMSRoomStatus
                    {
                        RoomNumber = roomNumber,
                        RoomType = "Suite",
                        Status = "Occupied",
                        GuestName = knownGuest.FullName,
                        PMSGuestId = knownGuest.PMSGuestId,
                        CheckInDate = knownGuest.CheckInDate,
                        CheckOutDate = knownGuest.CheckOutDate,
                        LastUpdatedAt = DateTime.Now
                    });
                }
                else
                {
                    rooms.Add(GenerateRandomRoomStatus(roomNumber));
                }
            }
            
            return Task.FromResult(rooms);
        }

        public override Task<PMSFolio?> GetFolioAsync(string folioId)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock folio for ID: {FolioId}", folioId);
            
            // Deterministic testing
            if (folioId == "MOCK-F-TEST")
            {
                 // Use a known guest's current reservation
                 var knownGuest = _knownGuests["MOCK-G-REG-002"]; // Sarah Connor
                 var reservationId = $"MOCK-R-{knownGuest.PMSGuestId}-CURR";
                 
                 var folio = GenerateRandomFolio(reservationId);
                 folio.FolioId = folioId;
                 return Task.FromResult<PMSFolio?>(folio);
            }

            var randomReservationId = $"MOCK-R-{_random.Next(2000, 3000)}";
            var randomFolio = GenerateRandomFolio(randomReservationId);
            return Task.FromResult<PMSFolio?>(randomFolio);
        }

        public override Task<List<PMSFolio>> GetFoliosAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("MockPMSAdapter: Generating mock folios for date range: {Start} - {End}", startDate, endDate);
            
            var folios = new List<PMSFolio>();

            // Generate folios for known guests
            foreach (var knownGuest in _knownGuests.Values)
            {
                // Past folio
                 folios.Add(GenerateRandomFolio($"MOCK-R-{knownGuest.PMSGuestId}-HIST-1"));
                 
                 // Current folio
                 if (knownGuest.CheckInDate.HasValue)
                 {
                     folios.Add(GenerateRandomFolio($"MOCK-R-{knownGuest.PMSGuestId}-CURR"));
                 }
            }
            
            // Random folios
            var count = _random.Next(3, 8);
            for (int i = 0; i < count; i++)
            {
                var folio = GenerateRandomFolio($"MOCK-R-{2000 + i}");
                folio.FolioDate = startDate.AddDays(_random.Next(0, (int)((endDate - startDate).TotalDays <= 0 ? 1 : (endDate - startDate).TotalDays)));
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
                Preferences = _random.Next(10) < 3 ? "{\"dietaryPreferences\":\"vegetarian\",\"preferredRoomType\":\"high-floor\"}" : null,
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
