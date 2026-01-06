using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.QRCode
{
    /// <summary>
    /// QR kod servisi implementasyonu
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Domain.Entities.Core.TransferEntity> _transferRepository;
        private readonly IRepository<Domain.Entities.Core.ItineraryEntity> _itineraryRepository;
        private readonly IRepository<Domain.Entities.Core.RestaurantReservationEntity> _restaurantReservationRepository;
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(
            IUnitOfWork unitOfWork,
            IRepository<Domain.Entities.Core.TransferEntity> transferRepository,
            IRepository<Domain.Entities.Core.ItineraryEntity> itineraryRepository,
            IRepository<Domain.Entities.Core.RestaurantReservationEntity> restaurantReservationRepository,
            ILogger<QRCodeService> logger)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _itineraryRepository = itineraryRepository;
            _restaurantReservationRepository = restaurantReservationRepository;
            _logger = logger;
        }

        public async Task<ServiceMessage<QRCodeResult>> GenerateQRCodeAsync(string data, int size = 300)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCoder.QRCode(qrCodeData);
                using var qrCodeImage = qrCode.GetGraphic(20);

                // Resmi yeniden boyutlandır
                using var resizedImage = new Bitmap(qrCodeImage, new Size(size, size));

                // Memory stream'e kaydet
                using var ms = new MemoryStream();
                resizedImage.Save(ms, ImageFormat.Png);
                var imageBytes = ms.ToArray();
                var base64String = Convert.ToBase64String(imageBytes);

                return new ServiceMessage<QRCodeResult>
                {
                    IsSuccess = true,
                    Message = "QR kod başarıyla oluşturuldu.",
                    Data = new QRCodeResult
                    {
                        Base64Image = base64String,
                        ImageBytes = imageBytes,
                        Data = data,
                        ContentType = "image/png"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"QR kod oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<QRCodeResult>
                {
                    IsSuccess = false,
                    Message = $"QR kod oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<QRCodeResult>> GenerateTransferQRCodeAsync(int transferId)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null)
                    return new ServiceMessage<QRCodeResult>
                    {
                        IsSuccess = false,
                        Message = "Transfer bulunamadı."
                    };

                // QR kod içeriği - JSON formatında transfer bilgileri
                var qrData = $@"{{
                    ""type"": ""transfer"",
                    ""id"": {transfer.Id},
                    ""number"": ""TRF-{transfer.Id}"",
                    ""date"": ""{transfer.TransferDate:yyyy-MM-dd HH:mm}"",
                    ""guest"": ""{transfer.Guest?.FullName ?? ""}"",
                    ""pickup"": ""{transfer.PickupAddress}"",
                    ""dropoff"": ""{transfer.DropoffAddress}""
                }}";

                return await GenerateQRCodeAsync(qrData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer QR kodu oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<QRCodeResult>
                {
                    IsSuccess = false,
                    Message = $"Transfer QR kodu oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<QRCodeResult>> GenerateItineraryQRCodeAsync(int itineraryId)
        {
            try
            {
                var itinerary = await _itineraryRepository.GetAll()
                    .Include(i => i.Guest)
                    .FirstOrDefaultAsync(i => i.Id == itineraryId);

                if (itinerary == null)
                    return new ServiceMessage<QRCodeResult>
                    {
                        IsSuccess = false,
                        Message = "İtinerary bulunamadı."
                    };

                // QR kod içeriği - JSON formatında itinerary bilgileri
                var qrData = $@"{{
                    ""type"": ""itinerary"",
                    ""id"": {itinerary.Id},
                    ""number"": ""{itinerary.ItineraryNumber}"",
                    ""guest"": ""{itinerary.Guest?.FullName ?? ""}"",
                    ""startDate"": ""{itinerary.StartDate:yyyy-MM-dd}"",
                    ""endDate"": ""{itinerary.EndDate:yyyy-MM-dd}"",
                    ""status"": ""{itinerary.Status}""
                }}";

                return await GenerateQRCodeAsync(qrData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"İtinerary QR kodu oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<QRCodeResult>
                {
                    IsSuccess = false,
                    Message = $"İtinerary QR kodu oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<QRCodeResult>> GenerateRestaurantReservationQRCodeAsync(int reservationId)
        {
            try
            {
                var reservation = await _restaurantReservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Restaurant)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                if (reservation == null)
                    return new ServiceMessage<QRCodeResult>
                    {
                        IsSuccess = false,
                        Message = "Restoran rezervasyonu bulunamadı."
                    };

                // QR kod içeriği - JSON formatında rezervasyon bilgileri
                var qrData = $@"{{
                    ""type"": ""restaurant_reservation"",
                    ""id"": {reservation.Id},
                    ""confirmationNumber"": ""{reservation.ConfirmationNumber ?? ""}"",
                    ""restaurant"": ""{reservation.Restaurant?.RestaurantName ?? ""}"",
                    ""guest"": ""{reservation.Guest?.FullName ?? ""}"",
                    ""date"": ""{reservation.ReservationDate:yyyy-MM-dd}"",
                    ""time"": ""{reservation.ReservationTime:hh\:mm}"",
                    ""guests"": {reservation.NumberOfGuests}
                }}";

                return await GenerateQRCodeAsync(qrData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Restoran rezervasyon QR kodu oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<QRCodeResult>
                {
                    IsSuccess = false,
                    Message = $"Restoran rezervasyon QR kodu oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<string>> DecodeQRCodeAsync(byte[] qrCodeImage)
        {
            try
            {
                // QR kod okuma için ZXing.Net veya benzeri bir kütüphane kullanılabilir
                // Bu basit bir implementasyon, gerçek projede ZXing.Net kullanılmalı
                return new ServiceMessage<string>
                {
                    IsSuccess = false,
                    Message = "QR kod okuma özelliği henüz implemente edilmedi. ZXing.Net kütüphanesi eklenmelidir."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"QR kod okunurken hata: {ex.Message}");
                return new ServiceMessage<string>
                {
                    IsSuccess = false,
                    Message = $"QR kod okunurken hata: {ex.Message}"
                };
            }
        }
    }
}

