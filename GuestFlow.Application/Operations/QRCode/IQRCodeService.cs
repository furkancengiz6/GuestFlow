using GuestFlow.Application.Types;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.QRCode
{
    /// <summary>
    /// QR kod servisi interface'i
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// QR kod oluşturur ve base64 string olarak döndürür
        /// </summary>
        Task<ServiceMessage<QRCodeResult>> GenerateQRCodeAsync(string data, int size = 300);

        /// <summary>
        /// Transfer için QR kod oluşturur
        /// </summary>
        Task<ServiceMessage<QRCodeResult>> GenerateTransferQRCodeAsync(int transferId);

        /// <summary>
        /// İtinerary için QR kod oluşturur
        /// </summary>
        Task<ServiceMessage<QRCodeResult>> GenerateItineraryQRCodeAsync(int itineraryId);

        /// <summary>
        /// Restoran rezervasyonu için QR kod oluşturur
        /// </summary>
        Task<ServiceMessage<QRCodeResult>> GenerateRestaurantReservationQRCodeAsync(int reservationId);

        /// <summary>
        /// QR kod içeriğini okur (decode)
        /// </summary>
        Task<ServiceMessage<string>> DecodeQRCodeAsync(byte[] qrCodeImage);
    }

    public class QRCodeResult
    {
        public string Base64Image { get; set; } = string.Empty;
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
        public string Data { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/png";
    }
}

