using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.AI
{
    /// <summary>
    /// AI'ya beslenecek misafir bağlamını (context) veritabanlarından toplayan servis
    /// </summary>
    public class ContextRetriever
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContextRetriever> _logger;

        public ContextRetriever(IUnitOfWork unitOfWork, ILogger<ContextRetriever> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Misafir için tüm ilişkili verileri metin formatında döner (RAG context)
        /// </summary>
        public async Task<string> GetGuestContextAsync(int guestId)
        {
            _logger.LogInformation("Retrieving AI context for Guest {Id}", guestId);
            
            var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
            if (guest == null) return "Misafir bulunamadı.";

            var sb = new StringBuilder();
            sb.AppendLine($"Misafir Adı: {guest.FullName}");
            sb.AppendLine($"VIP Durumu: {(guest.IsSpecialGuest ? "Evet (Önemli Misafir)" : "Hayır")}");
            sb.AppendLine($"Oda: {guest.RoomNumber ?? "Atanmamış"}");
            sb.AppendLine($"Milliyet: {guest.Nationality ?? "Belirtilmemiş"}");
            
            // Rezervasyonlar
            var reservations = await _unitOfWork.Reservations.GetAll(r => r.GuestId == guestId).ToListAsync();
            if (reservations.Any())
            {
                sb.AppendLine("Aktif/Gelecek Rezervasyonlar:");
                foreach (var res in reservations)
                {
                    sb.AppendLine($"- {res.ServiceType}: Durum {res.Status}, Tarih {res.ReservationDate:dd.MM.yyyy}");
                }
            }

            // Tur Geçmişi
            var yachtTours = await _unitOfWork.YachtTours.GetAll(t => t.OwnerGuestId == guestId).ToListAsync();
            if (yachtTours.Any())
            {
                sb.AppendLine("Geçmiş Yat Turu Deneyimleri:");
                foreach (var tour in yachtTours)
                {
                    sb.AppendLine($"- {tour.YachtName ?? "Özel Yat"} ({tour.TourDate:dd.MM.yyyy})");
                }
            }

            var cityTours = await _unitOfWork.CityTours.GetAll(t => t.OwnerGuestId == guestId).ToListAsync();
            if (cityTours.Any())
            {
                sb.AppendLine("Geçmiş Şehir Turu Deneyimleri:");
                foreach (var tour in cityTours)
                {
                    sb.AppendLine($"- Şehir Turu (Rehber: {tour.GuideName ?? "Belirtilmemiş"}) - {tour.TourDate:dd.MM.yyyy}");
                }
            }

            // Tercihler
            var preferences = await _unitOfWork.GuestPreferences.GetAll(p => p.GuestId == guestId).ToListAsync();
            var pref = preferences.FirstOrDefault();
            if (pref != null)
            {
                sb.AppendLine("Misafir Tercihleri ve İlgi Alanları:");
                if (!string.IsNullOrEmpty(pref.DietaryPreferences)) sb.AppendLine($"- Diyet Tercihleri: {pref.DietaryPreferences}");
                if (!string.IsNullOrEmpty(pref.FoodAllergies)) sb.AppendLine($"- Alerjiler: {pref.FoodAllergies}");
                if (!string.IsNullOrEmpty(pref.Interests)) sb.AppendLine($"- İlgi Alanları: {pref.Interests}");
                if (!string.IsNullOrEmpty(pref.PreferredLanguage)) sb.AppendLine($"- Tercih Edilen Dil: {pref.PreferredLanguage}");
                if (!string.IsNullOrEmpty(pref.RoomSpecialRequests)) sb.AppendLine($"- Oda Tercihleri: {pref.RoomSpecialRequests}");
                if (!string.IsNullOrEmpty(pref.PreferredRoomType)) sb.AppendLine($"- Tercih Edilen Oda Tipi: {pref.PreferredRoomType}");
            }

            // Son Davranışlar (Behavioral Context)
            var behaviors = await _unitOfWork.GuestBehaviors.GetAll(b => b.GuestId == guestId)
                .OrderByDescending(b => b.BehaviorDate)
                .Take(5)
                .ToListAsync();

            if (behaviors.Any())
            {
                sb.AppendLine("Son Aktiviteler:");
                foreach (var b in behaviors)
                {
                    sb.AppendLine($"- {b.BehaviorDate:dd.MM HH:mm}: {b.BehaviorType} ({b.Category})");
                }
            }
            
            return sb.ToString();
        }
    }
}
