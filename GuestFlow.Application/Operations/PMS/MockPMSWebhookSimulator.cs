// Copyright (c) 2025-2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// Mock PMS Webhook Simulator - Development ve test ortamlarında PMS webhook'larını simüle eder.
    /// </summary>
    public interface IMockPMSWebhookSimulator
    {
        /// <summary>
        /// Yeni check-in webhook'u simüle eder
        /// </summary>
        Task<PMSWebhookPayload> SimulateCheckInAsync(int integrationId);

        /// <summary>
        /// Yeni check-out webhook'u simüle eder
        /// </summary>
        Task<PMSWebhookPayload> SimulateCheckOutAsync(int integrationId);

        /// <summary>
        /// Yeni rezervasyon webhook'u simüle eder
        /// </summary>
        Task<PMSWebhookPayload> SimulateNewReservationAsync(int integrationId);

        /// <summary>
        /// Rezervasyon iptal webhook'u simüle eder
        /// </summary>
        Task<PMSWebhookPayload> SimulateCancelReservationAsync(int integrationId);

        /// <summary>
        /// Oda durumu değişikliği webhook'u simüle eder
        /// </summary>
        Task<PMSWebhookPayload> SimulateRoomStatusChangeAsync(int integrationId, string roomNumber);
    }

    /// <summary>
    /// Mock webhook payload modeli
    /// </summary>
    public class PMSWebhookPayload
    {
        public string EventType { get; set; } = string.Empty;
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Provider { get; set; } = "MOCK";
        public object? Data { get; set; }
    }

    /// <summary>
    /// Mock PMS Webhook Simulator implementasyonu
    /// </summary>
    public class MockPMSWebhookSimulator : IMockPMSWebhookSimulator
    {
        private readonly IPMSWebhookProcessor _webhookProcessor;
        private readonly ILogger<MockPMSWebhookSimulator> _logger;
        private readonly Random _random = new();

        private static readonly string[] _firstNames = { "John", "Emma", "Lucas", "Olivia", "Ahmet", "Ayşe", "Hans", "Marie" };
        private static readonly string[] _lastNames = { "Smith", "Johnson", "Yılmaz", "Kaya", "Müller", "Ivanov" };
        private static readonly string[] _roomNumbers = { "101", "102", "201", "202", "301", "302", "401", "402" };
        private static readonly string[] _roomTypes = { "Standard", "Superior", "Deluxe", "Suite" };

        public MockPMSWebhookSimulator(
            IPMSWebhookProcessor webhookProcessor,
            ILogger<MockPMSWebhookSimulator> logger)
        {
            _webhookProcessor = webhookProcessor;
            _logger = logger;
        }

        public async Task<PMSWebhookPayload> SimulateCheckInAsync(int integrationId)
        {
            _logger.LogInformation("Simulating CHECK_IN webhook for integration: {IntegrationId}", integrationId);

            var payload = new PMSWebhookPayload
            {
                EventType = "guest.checked_in",
                Data = new
                {
                    guestId = $"MOCK-G-{_random.Next(1000, 9999)}",
                    guestName = $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                    roomNumber = _roomNumbers[_random.Next(_roomNumbers.Length)],
                    checkInTime = DateTime.Now,
                    reservationId = $"MOCK-R-{_random.Next(2000, 9999)}"
                }
            };

            // Webhook processor'a gönder
            await _webhookProcessor.ProcessWebhookAsync(
                integrationId,
                System.Text.Json.JsonSerializer.Serialize(payload),
                "mock-signature");

            return payload;
        }

        public async Task<PMSWebhookPayload> SimulateCheckOutAsync(int integrationId)
        {
            _logger.LogInformation("Simulating CHECK_OUT webhook for integration: {IntegrationId}", integrationId);

            var payload = new PMSWebhookPayload
            {
                EventType = "guest.checked_out",
                Data = new
                {
                    guestId = $"MOCK-G-{_random.Next(1000, 9999)}",
                    guestName = $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                    roomNumber = _roomNumbers[_random.Next(_roomNumbers.Length)],
                    checkOutTime = DateTime.Now,
                    reservationId = $"MOCK-R-{_random.Next(2000, 9999)}",
                    totalAmount = _random.Next(500, 5000),
                    currency = "EUR"
                }
            };

            await _webhookProcessor.ProcessWebhookAsync(
                integrationId,
                System.Text.Json.JsonSerializer.Serialize(payload),
                "mock-signature");

            return payload;
        }

        public async Task<PMSWebhookPayload> SimulateNewReservationAsync(int integrationId)
        {
            _logger.LogInformation("Simulating NEW_RESERVATION webhook for integration: {IntegrationId}", integrationId);

            var checkIn = DateTime.Now.AddDays(_random.Next(1, 30));
            var checkOut = checkIn.AddDays(_random.Next(1, 7));

            var payload = new PMSWebhookPayload
            {
                EventType = "reservation.created",
                Data = new
                {
                    reservationId = $"MOCK-R-{_random.Next(2000, 9999)}",
                    guestId = $"MOCK-G-{_random.Next(1000, 9999)}",
                    guestName = $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                    roomType = _roomTypes[_random.Next(_roomTypes.Length)],
                    checkInDate = checkIn,
                    checkOutDate = checkOut,
                    guestCount = _random.Next(1, 4),
                    totalAmount = _random.Next(500, 5000),
                    currency = "EUR",
                    status = "Confirmed"
                }
            };

            await _webhookProcessor.ProcessWebhookAsync(
                integrationId,
                System.Text.Json.JsonSerializer.Serialize(payload),
                "mock-signature");

            return payload;
        }

        public async Task<PMSWebhookPayload> SimulateCancelReservationAsync(int integrationId)
        {
            _logger.LogInformation("Simulating CANCEL_RESERVATION webhook for integration: {IntegrationId}", integrationId);

            var payload = new PMSWebhookPayload
            {
                EventType = "reservation.cancelled",
                Data = new
                {
                    reservationId = $"MOCK-R-{_random.Next(2000, 9999)}",
                    guestId = $"MOCK-G-{_random.Next(1000, 9999)}",
                    cancellationReason = "Guest request",
                    cancellationTime = DateTime.Now,
                    refundAmount = _random.Next(0, 500),
                    currency = "EUR"
                }
            };

            await _webhookProcessor.ProcessWebhookAsync(
                integrationId,
                System.Text.Json.JsonSerializer.Serialize(payload),
                "mock-signature");

            return payload;
        }

        public async Task<PMSWebhookPayload> SimulateRoomStatusChangeAsync(int integrationId, string roomNumber)
        {
            _logger.LogInformation("Simulating ROOM_STATUS_CHANGE webhook for integration: {IntegrationId}, Room: {RoomNumber}", 
                integrationId, roomNumber);

            string[] statuses = { "Available", "Occupied", "OutOfOrder", "Maintenance" };

            var payload = new PMSWebhookPayload
            {
                EventType = "room.status_changed",
                Data = new
                {
                    roomNumber = roomNumber,
                    previousStatus = statuses[_random.Next(statuses.Length)],
                    newStatus = statuses[_random.Next(statuses.Length)],
                    changedAt = DateTime.Now
                }
            };

            await _webhookProcessor.ProcessWebhookAsync(
                integrationId,
                System.Text.Json.JsonSerializer.Serialize(payload),
                "mock-signature");

            return payload;
        }
    }
}
