using System;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Guest
{
    public class GuestManager : IGuestService
    {
        // Bu üç değişkeni sınıfın içinde kullanıyoruz.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyoruz örneğin, transaction başlatma, kaydetme.
        // _guestRepository: Misafirlerle ilgili veritabanı işlemlerini yapmak için kullanıyoruz.
        // _logger: Hataları veya bilgileri loglamak kaydetmek için kullanıyoruz.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly ILogger<GuestManager> _logger;

        // Constructor Bu sınıf oluşturulurken dependency buradan alıyoruz.
        public GuestManager(
            IUnitOfWork unitOfWork,
            IRepository<GuestEntity> guestRepository,
            ILogger<GuestManager> logger)
        {
            _unitOfWork = unitOfWork;
            _guestRepository = guestRepository;
            _logger = logger;
        }

        // Bu metod, yeni bir misafir ekliyor.
        public async Task<ServiceMessage> AddGuest(AddGuestDto guest)
        {
            try
            {
                // Veritabanında transaction başlatıyoruz. Eğer bir hata olursa, tüm değişiklikleri geri alacağız.
                await _unitOfWork.BeginTransactionAsync();

                // Eğer misafir özel bir misafir değilse IsSpecialGuest false ise, bazı bilgileri kontrol etmemiz lazım.
                if (!guest.IsSpecialGuest)
                {
                    // Email boş mu ya da sadece boşluklardan mı oluşuyor? Eğer öyleyse hata döndürüyoruz.
                    if (string.IsNullOrWhiteSpace(guest.Email))
                        return new ServiceMessage { IsSuccess = false, Message = "Email alanı zorunludur." };

                    // Email'de "@" işareti var mı? Yoksa geçerli bir email değil demektir.
                    if (!guest.Email.Contains("@"))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçerli bir e-posta adresi giriniz (örneğin, user@example.com)." };

                    // Email'in uzunluğu 5'ten küçük veya 100'den büyük mü? Öyleyse hata veriyoruz.
                    if (guest.Email.Length < 5 || guest.Email.Length > 100)
                        return new ServiceMessage { IsSuccess = false, Message = "Email 5 ila 100 karakter arasında olmalıdır." };

                    // Telefon numarası boş mu ya da sadece boşluklardan mı oluşuyor? Eğer öyleyse hata döndürüyoruz.
                    if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber alanı zorunludur." };

                    // Telefon numarası sadece rakamlar ve "+" işaretinden mi oluşuyor? Değilse hata veriyoruz.
                    if (!guest.PhoneNumber.All(c => char.IsDigit(c) || c == '+'))
                        return new ServiceMessage { IsSuccess = false, Message = "Telefon numarası sadece rakamlardan oluşmalıdır (örneğin, +905551234567)." };

                    // Telefon numarasının uzunluğu 5'ten küçük veya 20'den büyük mü? Öyleyse hata veriyoruz.
                    if (guest.PhoneNumber.Length < 5 || guest.PhoneNumber.Length > 20)
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber 5 ila 20 karakter arasında olmalıdır." };
                }

                // Eğer misafir özel bir misafirse IsSpecialGuest true ise, email ve telefon numarası boşsa varsayılan değerler atıyoruz.
                if (guest.IsSpecialGuest)
                {
                    guest.Email ??= "special@guestflow.com"; // Email boşsa, bu varsayılan değeri atıyoruz.
                    guest.PhoneNumber ??= "+9000000000"; // Telefon numarası boşsa, bu varsayılan değeri atıyoruz.
                }

                // Misafire özel bir kod (GuestCode) oluşturuyoruz. Bu kod, her misafir için benzersiz olmalı.
                string guestCode = await GenerateGuestCodeAsync();

                // Bu GuestCode ile başka bir misafir var mı diye kontrol ediyoruz.
                var hasGuest = await _guestRepository.GetAll(x => x.GuestCode == guestCode).AnyAsync();
                if (hasGuest)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu GuestCode ile bir misafir zaten mevcut." };

                // Yeni bir misafir nesnesi oluşturuyoruz ve DTO'dan gelen bilgileri buraya aktarıyoruz.
                var newGuest = new GuestEntity
                {
                    FullName = guest.FullName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Nationality = guest.Nationality,
                    GuestCode = guestCode,
                    IsSpecialGuest = guest.IsSpecialGuest
                };

                // Yeni misafiri veritabanına ekliyoruz.
                await _guestRepository.AddAsync(newGuest);
                // Değişiklikleri veritabanına kaydediyoruz.
                await _unitOfWork.SaveChangesAsync();
                // İşlem başarılıysa transaction'ı tamamlıyoruz 
                await _unitOfWork.CommitTransactionAsync();

                // Başarılı bir şekilde misafir eklendiğini logluyoru
                _logger.LogInformation($"Misafir eklendi: {guest.FullName}");
                // Başarı mesajı döndürüyoruz.
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, transaction'ı geri alıyoruz (rollback yapıyoruz).
                await _unitOfWork.RollbackTransactionAsync();
                // Hatayı logluyoruz. InnerException varsa onu da ekliyoruz ki daha fazla bilgi alabilelim.
                _logger.LogError(ex, $"Misafir eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                // Hata mesajını oluşturuyoruz. InnerException varsa onu da ekliyoruz.
                string errorMessage = $"Misafir eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                // Hata mesajıyla birlikte başarısız bir sonuç döndürüyoruz.
                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metod, mevcut bir misafiri güncelliyor.
        public async Task<ServiceMessage> UpdateGuest(UpdateGuestDto guest)
        {
            try
            {
                // Veritabanında bir işlem  başlatıyoruz.
                await _unitOfWork.BeginTransactionAsync();

                // Güncellenecek misafiri IDsine göre veritabanından çekiyoruz.
                var existing = await _guestRepository.GetAsync(x => x.Id == guest.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                // Eğer misafir özel bir misafir değilse, bilgileri kontrol ediyoruz 
                if (!guest.IsSpecialGuest)
                {
                    if (string.IsNullOrWhiteSpace(guest.Email))
                        return new ServiceMessage { IsSuccess = false, Message = "Email alanı zorunludur." };

                    if (!guest.Email.Contains("@"))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçerli bir e-posta adresi giriniz (örneğin, user@example.com)." };

                    if (guest.Email.Length < 5 || guest.Email.Length > 100)
                        return new ServiceMessage { IsSuccess = false, Message = "Email 5 ila 100 karakter arasında olmalıdır." };

                    if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber alanı zorunludur." };

                    if (!guest.PhoneNumber.All(c => char.IsDigit(c) || c == '+'))
                        return new ServiceMessage { IsSuccess = false, Message = "Telefon numarası sadece rakamlardan oluşmalıdır (örneğin, +905551234567)." };

                    if (guest.PhoneNumber.Length < 5 || guest.PhoneNumber.Length > 20)
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber 5 ila 20 karakter arasında olmalıdır." };
                }

                // Misafirin bilgilerini güncelliyoruz. Özel misafir için varsayılan değerler atıyoruz.
                existing.FullName = guest.FullName;
                existing.Email = guest.Email ?? "special@guestflow.com";
                existing.PhoneNumber = guest.PhoneNumber ?? "+9000000000";
                existing.Nationality = guest.Nationality;
                existing.IsSpecialGuest = guest.IsSpecialGuest;

                // Güncellenmiş misafiri veritabanına kaydediyoruz.
                await _guestRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Başarılı bir şekilde güncellendiğini logluyoruz.
                _logger.LogInformation($"Misafir güncellendi: {guest.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Misafir güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Misafir güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metod, bir misafiri silmek için kullanılıyor.
        public async Task<ServiceMessage> DeleteGuest(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Veritabanından misafiri silmek için ID'sini kullanıyoruz.
                await _guestRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Misafir silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Misafir silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Misafir silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metod, belirli bir misafiri ID'sine göre getiriyor.
        public async Task<GetGuestDto> GetGuestById(int id)
        {
            try
            {
                // Veritabanından misafiri ID sine göre çektik
                var guest = await _guestRepository.GetByIdAsync(id);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                // Misafiri bir DTO nesnesine çevirip geri döndürdik
                return new GetGuestDto
                {
                    Id = guest.Id,
                    FullName = guest.FullName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Nationality = guest.Nationality,
                    GuestCode = guest.GuestCode,
                    IsSpecialGuest = guest.IsSpecialGuest,
                    CreatedDate = guest.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir getirilirken hata: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metod, tüm misafirleri getiriyor.
        public async Task<List<GetGuestDto>> GetGuests()
        {
            try
            {
                // Veritabanından tüm misafirleri çekiyoruz ve her birini GetGuestDto ya çeviriyoruz.
                return await _guestRepository.GetAll()
                    .Select(g => new GetGuestDto
                    {
                        Id = g.Id,
                        FullName = g.FullName,
                        Email = g.Email,
                        PhoneNumber = g.PhoneNumber,
                        Nationality = g.Nationality,
                        GuestCode = g.GuestCode,
                        IsSpecialGuest = g.IsSpecialGuest,
                        CreatedDate = g.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafirler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metod, her misafir için benzersiz bir GuestCode oluşturuyor.
        private async Task<string> GenerateGuestCodeAsync()
        {
            // Veritabanındaki son misafiri çekiyoruz IDsine göre sıralayıp en sonuncuyu alıyoruz.
            var lastGuest = await _guestRepository.GetAll()
                .OrderByDescending(g => g.Id)
                .FirstOrDefaultAsync();

            // Eğer hiç misafir yoksa, ilk misafir için kod "GUEST-001" olacak.
            int nextNumber = 1;
            if (lastGuest != null)
            {
                // Son misafirin GuestCode'unu alıyoruz 
                string lastCode = lastGuest.GuestCode;
                // GuestCode'un son kısmını alıyoruz (örneğin, "001").
                string lastNumber = lastCode.Split('-').Last();
                // Bu kısmı sayıya çeviriyoruz ve bir artırıyoruz.
                if (int.TryParse(lastNumber, out int number))
                    nextNumber = number + 1;
            }

            // Yeni GuestCode'u oluşturuyoruz. D3 formatı, sayıyı 3 haneli yapıyor örneğin, 1 -> 001.
            return $"GUEST-{nextNumber:D3}";
        }
    }
}