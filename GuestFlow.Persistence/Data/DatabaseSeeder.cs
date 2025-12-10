using GuestFlow.Domain.DataProtection;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Persistence.Data
{
    public class DatabaseSeeder
    {
        private readonly GuestFlowDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly IDataProtection _dataProtection;

        public DatabaseSeeder(GuestFlowDbContext context, ILogger<DatabaseSeeder> logger, IDataProtection dataProtection)
        {
            _context = context;
            _logger = logger;
            _dataProtection = dataProtection;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Demo veri oluşturma başlatılıyor...");

                // Sıralı seed işlemleri
                var cities = await SeedCitiesAsync();
                var airports = await SeedAirportsAsync(cities);
                var vehicles = await SeedVehiclesAsync();
                var personnel = await SeedPersonnelAsync();
                var guests = await SeedGuestsAsync();
                var transfers = await SeedTransfersAsync(guests, personnel, airports, vehicles, cities);
                var cityTours = await SeedCityToursAsync(guests, personnel, cities);
                var yachtTours = await SeedYachtToursAsync(guests, personnel, cities);
                var invoices = await SeedInvoicesAsync(guests, personnel, transfers, cityTours, yachtTours);
                var payments = await SeedPaymentsAsync(invoices, guests);
                await SeedDailyRevenuesAsync();
                await SeedDailyNotesAsync(personnel);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Demo veri başarıyla oluşturuldu!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo veri oluşturulurken hata oluştu!");
                throw;
            }
        }

        private async Task<List<CityEntity>> SeedCitiesAsync()
        {
            var requiredCities = new List<(string Name, string Country)>
            {
                ("İstanbul", "Türkiye"),
                ("Antalya", "Türkiye"),
                ("Bodrum", "Türkiye"),
                ("Kapadokya", "Türkiye"),
                ("Pamukkale", "Türkiye"),
                ("Fethiye", "Türkiye"),
                ("Marmaris", "Türkiye"),
                ("Alanya", "Türkiye")
            };

            var existingCities = await _context.Cities.ToListAsync();
            var existingCityNames = existingCities.Select(c => c.CityName.ToLowerInvariant()).ToHashSet();
            
            var citiesToAdd = new List<CityEntity>();
            foreach (var (name, country) in requiredCities)
            {
                if (!existingCityNames.Contains(name.ToLowerInvariant()))
                {
                    citiesToAdd.Add(new CityEntity 
                    { 
                        CityName = name, 
                        Country = country, 
                        CreatedDate = DateTime.UtcNow.AddMonths(-6) 
                    });
                }
            }

            if (citiesToAdd.Any())
            {
                await _context.Cities.AddRangeAsync(citiesToAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{citiesToAdd.Count} yeni şehir eklendi.");
            }
            else
            {
                _logger.LogInformation("Tüm gerekli şehirler zaten mevcut.");
            }

            // Tüm şehirleri veritabanından tekrar çek
            var allCities = await _context.Cities.ToListAsync();
            _logger.LogInformation($"Veritabanında toplam {allCities.Count} şehir bulundu. Şehir isimleri: {string.Join(", ", allCities.Select(c => $"'{c.CityName}' (ID: {c.Id})"))}");
            return allCities;
        }

        private async Task<List<AirportEntity>> SeedAirportsAsync(List<CityEntity> cities)
        {
            // Şehirleri veritabanından tekrar çek (ID'lerin atandığından emin ol)
            var citiesFromDb = await _context.Cities.ToListAsync();
            _logger.LogInformation($"Havaalanı oluşturma için {citiesFromDb.Count} şehir bulundu. Şehir isimleri: {string.Join(", ", citiesFromDb.Select(c => $"'{c.CityName}'"))}");
            
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase));
            var antalya = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("Antalya", StringComparison.OrdinalIgnoreCase));
            var bodrum = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("Bodrum", StringComparison.OrdinalIgnoreCase));

            if (istanbul == null || antalya == null || bodrum == null)
            {
                _logger.LogError("Gerekli şehir bulunamadı! İstanbul, Antalya veya Bodrum eksik.");
                _logger.LogError($"İstanbul: {istanbul != null}, Antalya: {antalya != null}, Bodrum: {bodrum != null}");
                throw new Exception("Seed işlemi için gerekli şehirler bulunamadı.");
            }

            var requiredAirports = new List<(string Name, string Code, CityEntity City)>
            {
                ("İstanbul Havalimanı", "IST", istanbul),
                ("Sabiha Gökçen Havalimanı", "SAW", istanbul),
                ("Antalya Havalimanı", "AYT", antalya),
                ("Milas-Bodrum Havalimanı", "BJV", bodrum)
            };

            var existingAirports = await _context.Airports.ToListAsync();
            var existingAirportCodes = existingAirports.Select(a => a.Code.ToUpperInvariant()).ToHashSet();
            
            var airportsToAdd = new List<AirportEntity>();
            foreach (var (name, code, city) in requiredAirports)
            {
                if (!existingAirportCodes.Contains(code.ToUpperInvariant()))
                {
                    airportsToAdd.Add(new AirportEntity 
                    { 
                        Name = name, 
                        Code = code, 
                        CityId = city.Id, 
                        CreatedDate = DateTime.UtcNow.AddMonths(-6) 
                    });
                }
            }

            if (airportsToAdd.Any())
            {
                await _context.Airports.AddRangeAsync(airportsToAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{airportsToAdd.Count} yeni havaalanı eklendi.");
            }
            else
            {
                _logger.LogInformation("Tüm gerekli havaalanları zaten mevcut.");
            }

            // Tüm havaalanlarını veritabanından tekrar çek
            var allAirports = await _context.Airports.ToListAsync();
            _logger.LogInformation($"Veritabanında toplam {allAirports.Count} havaalanı bulundu. Havaalanı kodları: {string.Join(", ", allAirports.Select(a => $"'{a.Code}'"))}");
            return allAirports;
        }

        private async Task<List<VehicleEntity>> SeedVehiclesAsync()
        {
            if (await _context.Vehicles.AnyAsync())
            {
                _logger.LogInformation("Araçlar zaten mevcut, atlanıyor.");
                return await _context.Vehicles.ToListAsync();
            }

            var vehicles = new List<VehicleEntity>
            {
                new VehicleEntity { Type = "SEDAN", PlateNumber = "34ABC123", Capacity = 4, DailyPrice = 1500, CreatedDate = DateTime.UtcNow.AddMonths(-6) },
                new VehicleEntity { Type = "VAN", PlateNumber = "34DEF456", Capacity = 8, DailyPrice = 2500, CreatedDate = DateTime.UtcNow.AddMonths(-6) },
                new VehicleEntity { Type = "MINIBUS", PlateNumber = "34GHI789", Capacity = 16, DailyPrice = 4000, CreatedDate = DateTime.UtcNow.AddMonths(-6) },
                new VehicleEntity { Type = "BUS", PlateNumber = "34JKL012", Capacity = 45, DailyPrice = 8000, CreatedDate = DateTime.UtcNow.AddMonths(-6) },
                new VehicleEntity { Type = "SEDAN", PlateNumber = "07MNO345", Capacity = 4, DailyPrice = 1500, CreatedDate = DateTime.UtcNow.AddMonths(-5) },
                new VehicleEntity { Type = "VAN", PlateNumber = "07PQR678", Capacity = 8, DailyPrice = 2500, CreatedDate = DateTime.UtcNow.AddMonths(-5) },
                new VehicleEntity { Type = "MINIBUS", PlateNumber = "07STU901", Capacity = 16, DailyPrice = 4000, CreatedDate = DateTime.UtcNow.AddMonths(-4) }
            };

            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{vehicles.Count} araç eklendi.");
            return vehicles;
        }


        private async Task<List<PersonnelEntity>> SeedPersonnelAsync()
        {
            var existingPersonnel = await _context.Personnels.ToListAsync();
            
            // Eğer personel varsa, şifrelerini güncelle (DataProtection key değişmiş olabilir)
            if (existingPersonnel.Any())
            {
                _logger.LogInformation("Personel zaten mevcut, şifreler güncelleniyor...");
                
                bool hasChanges = false;
                // Şifreleri güncelle
                foreach (var person in existingPersonnel)
                {
                    string correctPassword = person.Email.ToLower() switch
                    {
                        "ahmet@guestflow.com" => "Admin123!",
                        "ayse@guestflow.com" => "Staff123!",
                        "mehmet@guestflow.com" => "Staff123!",
                        "zeynep@guestflow.com" => "Staff123!",
                        _ => "Staff123!" // Varsayılan şifre
                    };
                    
                    try
                    {
                        // Şifreyi kontrol et, açılamıyorsa güncelle
                        var unprotected = _dataProtection.Unprotect(person.Password);
                        // Şifre açılabiliyor ama yanlış olabilir, her zaman güncelle
                        if (unprotected != correctPassword)
                        {
                            person.Password = _dataProtection.Protect(correctPassword);
                            hasChanges = true;
                            _logger.LogInformation($"Personel şifresi güncellendi: {person.Email}");
                        }
                    }
                    catch
                    {
                        // Şifre açılamıyorsa, yeni key ile güncelle
                        person.Password = _dataProtection.Protect(correctPassword);
                        hasChanges = true;
                        _logger.LogInformation($"Personel şifresi güncellendi (key değişmiş): {person.Email}");
                    }
                }
                
                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                }
                return existingPersonnel;
            }

            var personnel = new List<PersonnelEntity>
            {
                new PersonnelEntity 
                { 
                    FullName = "Ahmet Yılmaz", 
                    Email = "ahmet@guestflow.com", 
                    Password = _dataProtection.Protect("Admin123!"), 
                    UserType = UserType.Admin,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new PersonnelEntity 
                { 
                    FullName = "Ayşe Demir", 
                    Email = "ayse@guestflow.com", 
                    Password = _dataProtection.Protect("Staff123!"), 
                    UserType = UserType.Staff,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new PersonnelEntity 
                { 
                    FullName = "Mehmet Kaya", 
                    Email = "mehmet@guestflow.com", 
                    Password = _dataProtection.Protect("Staff123!"), 
                    UserType = UserType.Staff,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new PersonnelEntity 
                { 
                    FullName = "Zeynep Şahin", 
                    Email = "zeynep@guestflow.com", 
                    Password = _dataProtection.Protect("Staff123!"), 
                    UserType = UserType.Staff,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                }
            };

            await _context.Personnels.AddRangeAsync(personnel);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{personnel.Count} personel eklendi.");
            return personnel;
        }

        private string GenerateGuestCode(int index)
        {
            return $"GF{DateTime.Now.Year}{index:D6}";
        }

        private async Task<List<GuestEntity>> SeedGuestsAsync()
        {
            if (await _context.Guests.AnyAsync())
            {
                _logger.LogInformation("Misafirler zaten mevcut, atlanıyor.");
                return await _context.Guests.ToListAsync();
            }

            var guests = new List<GuestEntity>
            {
                new GuestEntity { FullName = "John Smith", Email = "john.smith@email.com", PhoneNumber = "+1-555-0101", Nationality = "USA", GuestCode = GenerateGuestCode(1), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddMonths(-3) },
                new GuestEntity { FullName = "Emma Johnson", Email = "emma.j@email.com", PhoneNumber = "+44-20-7946-0958", Nationality = "UK", GuestCode = GenerateGuestCode(2), IsSpecialGuest = true, CreatedDate = DateTime.UtcNow.AddMonths(-3) },
                new GuestEntity { FullName = "Hans Müller", Email = "hans.m@email.com", PhoneNumber = "+49-30-12345678", Nationality = "Germany", GuestCode = GenerateGuestCode(3), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddMonths(-2) },
                new GuestEntity { FullName = "Sophie Martin", Email = "sophie.m@email.com", PhoneNumber = "+33-1-2345-6789", Nationality = "France", GuestCode = GenerateGuestCode(4), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddMonths(-2) },
                new GuestEntity { FullName = "Marco Rossi", Email = "marco.r@email.com", PhoneNumber = "+39-02-1234-5678", Nationality = "Italy", GuestCode = GenerateGuestCode(5), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddMonths(-2) },
                new GuestEntity { FullName = "Anna Petrov", Email = "anna.p@email.com", PhoneNumber = "+7-495-123-4567", Nationality = "Russia", GuestCode = GenerateGuestCode(6), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddMonths(-1) },
                new GuestEntity { FullName = "David Lee", Email = "david.l@email.com", PhoneNumber = "+86-10-1234-5678", Nationality = "China", GuestCode = GenerateGuestCode(7), IsSpecialGuest = true, CreatedDate = DateTime.UtcNow.AddMonths(-1) },
                new GuestEntity { FullName = "Sarah Williams", Email = "sarah.w@email.com", PhoneNumber = "+1-555-0202", Nationality = "USA", GuestCode = GenerateGuestCode(8), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddMonths(-1) },
                new GuestEntity { FullName = "Michael Brown", Email = "michael.b@email.com", PhoneNumber = "+1-555-0303", Nationality = "USA", GuestCode = GenerateGuestCode(9), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddDays(-20) },
                new GuestEntity { FullName = "Lisa Anderson", Email = "lisa.a@email.com", PhoneNumber = "+46-8-123-4567", Nationality = "Sweden", GuestCode = GenerateGuestCode(10), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddDays(-15) },
                new GuestEntity { FullName = "James Wilson", Email = "james.w@email.com", PhoneNumber = "+61-2-1234-5678", Nationality = "Australia", GuestCode = GenerateGuestCode(11), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddDays(-10) },
                new GuestEntity { FullName = "Maria Garcia", Email = "maria.g@email.com", PhoneNumber = "+34-91-123-4567", Nationality = "Spain", GuestCode = GenerateGuestCode(12), IsSpecialGuest = false, CreatedDate = DateTime.UtcNow.AddDays(-5) }
            };

            await _context.Guests.AddRangeAsync(guests);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{guests.Count} misafir eklendi.");
            return guests;
        }

        private async Task<List<TransferEntity>> SeedTransfersAsync(
            List<GuestEntity> guests, 
            List<PersonnelEntity> personnel, 
            List<AirportEntity> airports, 
            List<VehicleEntity> vehicles,
            List<CityEntity> cities)
        {
            if (await _context.Transfers.AnyAsync())
            {
                _logger.LogInformation("Transferler zaten mevcut, atlanıyor.");
                return await _context.Transfers.ToListAsync();
            }

            // Şehirleri ve havaalanlarını veritabanından tekrar çek (ID'lerin atandığından emin ol)
            var citiesFromDb = await _context.Cities.ToListAsync();
            var airportsFromDb = await _context.Airports.ToListAsync();
            
            _logger.LogInformation($"Transfer oluşturma için {citiesFromDb.Count} şehir bulundu. Şehir isimleri: {string.Join(", ", citiesFromDb.Select(c => $"'{c.CityName}'"))}");
            _logger.LogInformation($"Transfer oluşturma için {airportsFromDb.Count} havaalanı bulundu. Havaalanı kodları: {string.Join(", ", airportsFromDb.Select(a => $"'{a.Code}'"))}");
            
            // Şehir aramasını case-insensitive yap
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase));
            var antalya = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("Antalya", StringComparison.OrdinalIgnoreCase));
            var bodrum = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("Bodrum", StringComparison.OrdinalIgnoreCase));
            var istAirport = airportsFromDb.FirstOrDefault(a => 
                a.Code.Equals("IST", StringComparison.OrdinalIgnoreCase));
            var aytAirport = airportsFromDb.FirstOrDefault(a => 
                a.Code.Equals("AYT", StringComparison.OrdinalIgnoreCase));

            if (istanbul == null || antalya == null || bodrum == null || istAirport == null || aytAirport == null)
            {
                _logger.LogError("Gerekli şehir veya havaalanı bulunamadı!");
                _logger.LogError($"İstanbul: {istanbul != null}, Antalya: {antalya != null}, Bodrum: {bodrum != null}");
                _logger.LogError($"IST: {istAirport != null}, AYT: {aytAirport != null}");
                
                // Alternatif olarak mevcut şehir ve havaalanıları kullan
                if (citiesFromDb.Count >= 2 && airportsFromDb.Count >= 1)
                {
                    _logger.LogWarning("Alternatif olarak mevcut şehir ve havaalanıları kullanılıyor.");
                    istanbul = citiesFromDb[0];
                    antalya = citiesFromDb.Count > 1 ? citiesFromDb[1] : citiesFromDb[0];
                    bodrum = citiesFromDb.Count > 2 ? citiesFromDb[2] : citiesFromDb.Count > 1 ? citiesFromDb[1] : citiesFromDb[0];
                    istAirport = airportsFromDb[0];
                    aytAirport = airportsFromDb.Count > 1 ? airportsFromDb[1] : airportsFromDb[0];
                }
                else
                {
                    throw new Exception($"Seed işlemi için yeterli şehir veya havaalanı bulunamadı. Şehir sayısı: {citiesFromDb.Count}, Havaalanı sayısı: {airportsFromDb.Count}");
                }
            }

            var transfers = new List<TransferEntity>();
            var random = new Random();
            var statuses = new[] { "Completed", "Pending", "Cancelled" };
            var currencies = new[] { "TRY", "USD", "EUR" };

            for (int i = 0; i < 25; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var vehicle = vehicles[random.Next(vehicles.Count)];
                var airport = random.Next(2) == 0 ? istAirport : aytAirport;
                var pickupCity = random.Next(2) == 0 ? istanbul : antalya;
                var dropoffCity = random.Next(2) == 0 ? istanbul : antalya;
                var transferDate = DateTime.UtcNow.AddDays(-random.Next(60));
                var price = vehicle.DailyPrice * (decimal)(0.5 + random.NextDouble());
                var discount = random.Next(10) < 2 ? (decimal?)(random.Next(5, 20)) : null;
                var finalPrice = discount.HasValue ? price * (1 - discount.Value / 100) : price;
                var currency = currencies[random.Next(currencies.Length)];

                transfers.Add(new TransferEntity
                {
                    PickupAddress = $"Airport Terminal {random.Next(1, 4)}",
                    DropoffAddress = $"{dropoffCity.CityName} Hotel District, Street {random.Next(1, 100)}",
                    TransferDate = transferDate,
                    Price = price,
                    FinalPrice = finalPrice,
                    DiscountPercentage = discount,
                    Currency = currency,
                    Status = statuses[random.Next(statuses.Length)],
                    IsFromAirport = true,
                    Note = random.Next(10) < 3 ? $"Special request: {new[] { "Early check-in", "Wheelchair assistance", "Extra luggage" }[random.Next(3)]}" : null,
                    GuestId = guest.Id,
                    PersonnelId = staff.Id,
                    AirportId = airport.Id,
                    VehicleId = vehicle.Id,
                    PickupCityId = pickupCity.Id,
                    DropoffCityId = dropoffCity.Id,
                    CreatedDate = transferDate.AddDays(-random.Next(1, 5))
                });
            }

            await _context.Transfers.AddRangeAsync(transfers);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{transfers.Count} transfer eklendi.");
            return transfers;
        }

        private async Task<List<CityTourEntity>> SeedCityToursAsync(
            List<GuestEntity> guests, 
            List<PersonnelEntity> personnel, 
            List<CityEntity> cities)
        {
            if (await _context.CityTours.AnyAsync())
            {
                _logger.LogInformation("Şehir turları zaten mevcut, atlanıyor.");
                return await _context.CityTours.ToListAsync();
            }

            // Şehirleri veritabanından tekrar çek (ID'lerin atandığından emin ol)
            var citiesFromDb = await _context.Cities.ToListAsync();
            var istanbul = citiesFromDb.FirstOrDefault(c => c.CityName == "İstanbul");
            var kapadokya = citiesFromDb.FirstOrDefault(c => c.CityName == "Kapadokya");
            var pamukkale = citiesFromDb.FirstOrDefault(c => c.CityName == "Pamukkale");

            if (istanbul == null || kapadokya == null || pamukkale == null)
            {
                _logger.LogError("Gerekli şehir bulunamadı! İstanbul, Kapadokya veya Pamukkale eksik.");
                throw new Exception("Seed işlemi için gerekli şehirler bulunamadı.");
            }

            var tours = new List<CityTourEntity>();
            var random = new Random();
            var languages = new[] { "English", "Turkish", "German", "French", "Spanish" };
            var currencies = new[] { "TRY", "USD", "EUR" };

            for (int i = 0; i < 15; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var city = new[] { istanbul, kapadokya, pamukkale }[random.Next(3)];
                var tourDate = DateTime.UtcNow.AddDays(-random.Next(45));
                var duration = new[] { 4, 6, 8 }[random.Next(3)];
                var price = duration * 500;
                var discount = random.Next(10) < 3 ? (decimal?)(random.Next(5, 15)) : null;
                var finalPrice = discount.HasValue ? price * (1 - discount.Value / 100) : price;
                var currency = currencies[random.Next(currencies.Length)];

                tours.Add(new CityTourEntity
                {
                    TourDate = tourDate,
                    Language = languages[random.Next(languages.Length)],
                    DurationHours = duration,
                    Price = price,
                    FinalPrice = finalPrice,
                    DiscountPercentage = discount,
                    Currency = currency,
                    OwnerGuestId = guest.Id,
                    PersonnelId = staff.Id,
                    CityId = city.Id,
                    CreatedDate = tourDate.AddDays(-random.Next(1, 7))
                });
            }

            await _context.CityTours.AddRangeAsync(tours);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{tours.Count} şehir turu eklendi.");
            return tours;
        }

        private async Task<List<YachtTourEntity>> SeedYachtToursAsync(
            List<GuestEntity> guests, 
            List<PersonnelEntity> personnel, 
            List<CityEntity> cities)
        {
            if (await _context.YachtTours.AnyAsync())
            {
                _logger.LogInformation("Yat turları zaten mevcut, atlanıyor.");
                return await _context.YachtTours.ToListAsync();
            }

            // Şehirleri veritabanından tekrar çek (ID'lerin atandığından emin ol)
            var citiesFromDb = await _context.Cities.ToListAsync();
            var bodrum = citiesFromDb.FirstOrDefault(c => c.CityName == "Bodrum");
            var fethiye = citiesFromDb.FirstOrDefault(c => c.CityName == "Fethiye");
            var marmaris = citiesFromDb.FirstOrDefault(c => c.CityName == "Marmaris");

            if (bodrum == null || fethiye == null || marmaris == null)
            {
                _logger.LogError("Gerekli şehir bulunamadı! Bodrum, Fethiye veya Marmaris eksik.");
                throw new Exception("Seed işlemi için gerekli şehirler bulunamadı.");
            }

            var tours = new List<YachtTourEntity>();
            var random = new Random();
            var yachtNames = new[] { "Blue Dream", "Sea Breeze", "Ocean Star", "Sunset", "Azure", "Crystal", "Pearl", "Dolphin" };
            var currencies = new[] { "TRY", "USD", "EUR" };

            for (int i = 0; i < 12; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var city = new[] { bodrum, fethiye, marmaris }[random.Next(3)];
                var tourDate = DateTime.UtcNow.AddDays(-random.Next(40));
                var numberOfPeople = random.Next(2, 12);
                var price = numberOfPeople * 1500;
                var discount = random.Next(10) < 2 ? (decimal?)(random.Next(10, 25)) : null;
                var finalPrice = discount.HasValue ? price * (1 - discount.Value / 100) : price;
                var currency = currencies[random.Next(currencies.Length)];

                tours.Add(new YachtTourEntity
                {
                    TourDate = tourDate,
                    NumberOfPeople = numberOfPeople,
                    Price = price,
                    FinalPrice = finalPrice,
                    DiscountPercentage = discount,
                    Currency = currency,
                    YachtName = yachtNames[random.Next(yachtNames.Length)],
                    SpecialRequest = random.Next(10) < 4 ? new[] { "Vegetarian meals", "Champagne service", "Music system", "Snorkeling equipment" }[random.Next(4)] : null,
                    OwnerGuestId = guest.Id,
                    PersonnelId = staff.Id,
                    CityId = city.Id,
                    CreatedDate = tourDate.AddDays(-random.Next(1, 10))
                });
            }

            await _context.YachtTours.AddRangeAsync(tours);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{tours.Count} yat turu eklendi.");
            return tours;
        }

        private async Task<List<InvoicesEntity>> SeedInvoicesAsync(
            List<GuestEntity> guests,
            List<PersonnelEntity> personnel,
            List<TransferEntity> transfers,
            List<CityTourEntity> cityTours,
            List<YachtTourEntity> yachtTours)
        {
            if (await _context.Invoices.AnyAsync())
            {
                _logger.LogInformation("Faturalar zaten mevcut, atlanıyor.");
                return await _context.Invoices.ToListAsync();
            }

            var invoices = new List<InvoicesEntity>();
            var random = new Random();
            var invoiceNumber = 1000;
            var currencies = new[] { "TRY", "USD", "EUR" };

            // Transfer faturaları
            foreach (var transfer in transfers.Take(20))
            {
                invoices.Add(new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = transfer.TransferDate.AddDays(random.Next(0, 3)),
                    TotalAmount = transfer.FinalPrice,
                    Currency = transfer.Currency,
                    Notes = $"Transfer invoice for {transfer.PickupAddress} to {transfer.DropoffAddress}",
                    PdfUrl = $"",
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    TransferId = transfer.Id,
                    CreatedDate = transfer.TransferDate
                });
            }

            // Şehir turu faturaları
            foreach (var tour in cityTours.Take(12))
            {
                invoices.Add(new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = tour.TourDate.AddDays(random.Next(-2, 1)),
                    TotalAmount = tour.FinalPrice,
                    Currency = tour.Currency,
                    Notes = $"City tour invoice - {tour.DurationHours} hours",
                    PdfUrl = $"",
                    GuestId = tour.OwnerGuestId,
                    PersonnelId = tour.PersonnelId,
                    CityTourId = tour.Id,
                    CreatedDate = tour.TourDate
                });
            }

            // Yat turu faturaları
            foreach (var tour in yachtTours.Take(10))
            {
                invoices.Add(new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = tour.TourDate.AddDays(random.Next(-2, 1)),
                    TotalAmount = tour.FinalPrice,
                    Currency = tour.Currency,
                    Notes = $"Yacht tour invoice - {tour.YachtName}",
                    PdfUrl = $"",
                    GuestId = tour.OwnerGuestId,
                    PersonnelId = tour.PersonnelId,
                    YachtTourId = tour.Id,
                    CreatedDate = tour.TourDate
                });
            }

            await _context.Invoices.AddRangeAsync(invoices);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{invoices.Count} fatura eklendi.");
            return invoices;
        }

        private string GeneratePaymentNumber(int index)
        {
            return $"PAY{DateTime.Now.Year}{index:D8}";
        }

        private async Task<List<PaymentEntity>> SeedPaymentsAsync(
            List<InvoicesEntity> invoices,
            List<GuestEntity> guests)
        {
            if (await _context.Payments.AnyAsync())
            {
                _logger.LogInformation("Ödemeler zaten mevcut, atlanıyor.");
                return await _context.Payments.ToListAsync();
            }

            var payments = new List<PaymentEntity>();
            var random = new Random();
            var paymentMethods = new[] { PaymentMethod.CreditCard, PaymentMethod.BankTransfer, PaymentMethod.Cash };
            var paymentStatuses = new[] { PaymentStatus.Completed, PaymentStatus.Pending, PaymentStatus.Completed, PaymentStatus.Completed }; // Çoğu tamamlanmış

            int paymentIndex = 1;
            foreach (var invoice in invoices.Take(35))
            {
                var status = paymentStatuses[random.Next(paymentStatuses.Length)];
                var method = paymentMethods[random.Next(paymentMethods.Length)];

                payments.Add(new PaymentEntity
                {
                    PaymentNumber = GeneratePaymentNumber(paymentIndex++),
                    InvoiceId = invoice.Id,
                    GuestId = invoice.GuestId,
                    Amount = invoice.TotalAmount,
                    Currency = invoice.Currency,
                    PaymentMethod = method,
                    Status = status,
                    PaymentDate = invoice.IssueDate.AddDays(random.Next(0, 5)),
                    TransactionId = status == PaymentStatus.Completed ? $"TXN{random.Next(100000, 999999)}" : null,
                    CreatedDate = invoice.IssueDate
                });
            }

            await _context.Payments.AddRangeAsync(payments);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{payments.Count} ödeme eklendi.");
            return payments;
        }

        private async Task SeedDailyRevenuesAsync()
        {
            if (await _context.DailyRevenues.AnyAsync())
            {
                _logger.LogInformation("Günlük gelirler zaten mevcut, atlanıyor.");
                return;
            }

            var revenues = new List<DailyRevenueEntity>();
            var random = new Random();
            var startDate = DateTime.UtcNow.AddDays(-60);

            for (int i = 0; i < 60; i++)
            {
                var date = startDate.AddDays(i);
                var totalRevenue = (decimal)(random.Next(50000, 200000) + random.NextDouble() * 10000);

                revenues.Add(new DailyRevenueEntity
                {
                    Date = date,
                    TotalRevenue = totalRevenue,
                    CreatedDate = date
                });
            }

            await _context.DailyRevenues.AddRangeAsync(revenues);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{revenues.Count} günlük gelir eklendi.");
        }

        private async Task SeedDailyNotesAsync(List<PersonnelEntity> personnel)
        {
            if (await _context.DailyNotes.AnyAsync())
            {
                _logger.LogInformation("Günlük notlar zaten mevcut, atlanıyor.");
                return;
            }

            var notes = new List<DailyNoteEntity>();
            var random = new Random();
            var noteTexts = new[]
            {
                "Guest requested late check-out",
                "Room service completed",
                "Special dietary requirements noted",
                "VIP guest arrival",
                "Maintenance request completed",
                "Guest feedback: Excellent service",
                "Early morning departure",
                "Extra towels provided",
                "Room upgrade requested",
                "Welcome amenities delivered"
            };

            var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList();

            for (int i = 0; i < 30; i++)
            {
                var noteDate = DateTime.UtcNow.AddDays(-random.Next(30));
                var staffMember = staff[random.Next(staff.Count)];

                notes.Add(new DailyNoteEntity
                {
                    NoteDate = noteDate,
                    RoomNumber = random.Next(100, 500),
                    NoteText = noteTexts[random.Next(noteTexts.Length)],
                    PersonnelId = staffMember.Id,
                    CreatedDate = noteDate
                });
            }

            await _context.DailyNotes.AddRangeAsync(notes);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{notes.Count} günlük not eklendi.");
        }
    }
}

