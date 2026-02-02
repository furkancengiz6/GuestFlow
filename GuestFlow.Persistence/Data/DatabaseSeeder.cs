using GuestFlow.Domain.DataProtection;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Persistence.Data
{
    public class DatabaseSeeder
    {
        private readonly GuestFlowDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly IDataProtection _dataProtection;
        private readonly IConfiguration _configuration;

        public DatabaseSeeder(
            GuestFlowDbContext context,
            ILogger<DatabaseSeeder> logger,
            IDataProtection dataProtection,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _dataProtection = dataProtection;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if demo data seeding is enabled
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["DOTNET_ENVIRONMENT"] ?? "Production";
                var seedDemoDataString = _configuration["SeedDemoData"];
                var seedDemoData = string.Equals(seedDemoDataString, "true", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(seedDemoDataString, "1", StringComparison.OrdinalIgnoreCase);

                if (environment != "Development" || !seedDemoData)
                {
                    _logger.LogInformation("Demo data seeding skipped. Environment: {Environment}, SeedDemoData: {SeedDemoData}", environment, seedDemoData);
                    _logger.LogInformation("To seed demo data locally, set: ASPNETCORE_ENVIRONMENT=Development AND SeedDemoData=true");
                    return;
                }

                _logger.LogWarning("🚨 SEEDING DEMO DATA IN DEVELOPMENT ENVIRONMENT 🚨");
                _logger.LogWarning("This will create demo users with randomly generated credentials.");
                _logger.LogWarning("Check the application logs for the generated credentials to login.");

                // Sıralı seed işlemleri
                var cities = await SeedCitiesAsync();
                var airports = await SeedAirportsAsync(cities);
                var vehicles = await SeedVehiclesAsync();
                var personnel = await SeedPersonnelAsync();
                var hotels = await SeedHotelsAsync(cities);
                var restaurants = await SeedRestaurantsAsync(cities);
                var guests = await SeedGuestsAsync(hotels);
                var roomAssignments = await SeedRoomAssignmentsAsync(guests, hotels);
                var tours = await SeedToursAsync(cities);
                var transfers = await SeedTransfersAsync(guests, personnel, airports, vehicles, cities, hotels, restaurants);
                var cityTours = await SeedCityToursAsync(guests, personnel, cities, tours, vehicles, hotels);
                var yachtTours = await SeedYachtToursAsync(guests, personnel, cities, hotels);
                var invoices = await SeedInvoicesAsync(guests, personnel, transfers, cityTours, yachtTours);
                var restaurantReservations = await SeedRestaurantReservationsAsync(guests, personnel, restaurants, hotels, vehicles);
                var itineraries = await SeedItinerariesAsync(guests, personnel, transfers, cityTours, yachtTours, restaurantReservations);
                var servicePackages = await SeedServicePackagesAsync(transfers, cityTours, yachtTours, restaurantReservations);
                var payments = await SeedPaymentsAsync(invoices, guests); // PAYMENTS MUST BE LAST
                await SeedDailyRevenuesAsync();
                await SeedDailyNotesAsync(personnel);
                await SeedGuestReviewsAsync(guests, hotels, restaurants, tours);
                await SeedGuestStaffInteractionsAsync(guests, personnel);

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


        /// <summary>
        /// Generates a random secure password for demo users
        /// </summary>
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one of each type
            password.Append(chars[random.Next(26)]); // uppercase
            password.Append(chars[random.Next(26, 52)]); // lowercase
            password.Append(chars[random.Next(52, 62)]); // number
            password.Append(chars[random.Next(62)]); // special char

            // Fill the rest randomly
            for (int i = 4; i < 12; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        /// <summary>
        /// Generates a demo email that's clearly not real
        /// </summary>
        private string GenerateDemoEmail(string name, string role)
        {
            var cleanName = name.ToLower().Replace(" ", ".").Replace("ş", "s").Replace("ğ", "g")
                           .Replace("ü", "u").Replace("ö", "o").Replace("ı", "i").Replace("ç", "c");
            return $"{cleanName}.demo.{role}@guestflow.local";
        }

        private async Task<List<PersonnelEntity>> SeedPersonnelAsync()
        {
            var existingPersonnel = await _context.Personnels.ToListAsync();

            // Eğer personel varsa, şifrelerini güncelle (DataProtection key değişmiş olabilir)
            if (existingPersonnel.Any())
            {
                _logger.LogInformation("Personel zaten mevcut, şifreler güncelleniyor...");

                bool hasChanges = false;
                // Şifreleri güncelle - demo veriler için rastgele şifreler kullan
                foreach (var person in existingPersonnel)
                {
                    // Demo verilerde şifre kontrolü yapma, her zaman rastgele şifre üret
                    if (person.Email.Contains(".demo.") || person.Email.Contains("@guestflow.local"))
                    {
                        var newPassword = GenerateRandomPassword();
                        person.Password = _dataProtection.Protect(newPassword);
                        hasChanges = true;
                        _logger.LogWarning($"🚨 DEMO USER PASSWORD GENERATED 🚨");
                        _logger.LogWarning($"Email: {person.Email}");
                        _logger.LogWarning($"Password: {newPassword}");
                        _logger.LogWarning($"Role: {person.UserType}");
                        _logger.LogWarning($"Save this password to login!");
                    }
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                }
                return existingPersonnel;
            }

            // Generate random passwords for demo users
            var adminPassword = GenerateRandomPassword();
            var staffPassword1 = GenerateRandomPassword();
            var staffPassword2 = GenerateRandomPassword();
            var staffPassword3 = GenerateRandomPassword();

            var personnel = new List<PersonnelEntity>
            {
                new PersonnelEntity
                {
                    FullName = "Demo Admin User",
                    Email = GenerateDemoEmail("Demo Admin", "admin"),
                    Password = _dataProtection.Protect(adminPassword),
                    UserType = UserType.Admin,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new PersonnelEntity
                {
                    FullName = "Demo Staff User 1",
                    Email = GenerateDemoEmail("Demo Staff 1", "staff"),
                    Password = _dataProtection.Protect(staffPassword1),
                    UserType = UserType.Staff,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new PersonnelEntity
                {
                    FullName = "Demo Staff User 2",
                    Email = GenerateDemoEmail("Demo Staff 2", "staff"),
                    Password = _dataProtection.Protect(staffPassword2),
                    UserType = UserType.Staff,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new PersonnelEntity
                {
                    FullName = "Demo Staff User 3",
                    Email = GenerateDemoEmail("Demo Staff 3", "staff"),
                    Password = _dataProtection.Protect(staffPassword3),
                    UserType = UserType.Staff,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                }
            };

            await _context.Personnels.AddRangeAsync(personnel);
            await _context.SaveChangesAsync();

            _logger.LogWarning("🚨 DEMO USERS CREATED - SAVE THESE CREDENTIALS! 🚨");
            _logger.LogWarning($"Admin: {personnel[0].Email} / {adminPassword}");
            _logger.LogWarning($"Staff1: {personnel[1].Email} / {staffPassword1}");
            _logger.LogWarning($"Staff2: {personnel[2].Email} / {staffPassword2}");
            _logger.LogWarning($"Staff3: {personnel[3].Email} / {staffPassword3}");
            _logger.LogWarning("These passwords are randomly generated and logged only once!");

            _logger.LogInformation($"{personnel.Count} demo personel eklendi.");
            return personnel;
        }

        private string GenerateGuestCode(int index)
        {
            return $"GF{DateTime.Now.Year}{index:D6}";
        }

        private async Task<List<GuestEntity>> SeedGuestsAsync(List<HotelEntity> hotels)
        {
            if (await _context.Guests.AnyAsync())
            {
                _logger.LogInformation("Misafirler zaten mevcut, atlanıyor.");
                return await _context.Guests.ToListAsync();
            }

            var random = new Random();
            var guests = new List<GuestEntity>
            {
                new GuestEntity { FullName = "John Smith", Email = "john.smith@email.com", PhoneNumber = "+1-555-0101", Nationality = "USA", GuestCode = GenerateGuestCode(1), IsSpecialGuest = false, RoomNumber = "101", CheckInDate = DateTime.UtcNow.AddDays(-5), CheckOutDate = DateTime.UtcNow.AddDays(2), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-3) },
                new GuestEntity { FullName = "Emma Johnson", Email = "emma.j@email.com", PhoneNumber = "+44-20-7946-0958", Nationality = "UK", GuestCode = GenerateGuestCode(2), IsSpecialGuest = true, RoomNumber = "201", CheckInDate = DateTime.UtcNow.AddDays(-3), CheckOutDate = DateTime.UtcNow.AddDays(4), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-3) },
                new GuestEntity { FullName = "Hans Müller", Email = "hans.m@email.com", PhoneNumber = "+49-30-12345678", Nationality = "Germany", GuestCode = GenerateGuestCode(3), IsSpecialGuest = false, RoomNumber = "102", CheckInDate = DateTime.UtcNow.AddDays(-7), CheckOutDate = DateTime.UtcNow.AddDays(1), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-2) },
                new GuestEntity { FullName = "Sophie Martin", Email = "sophie.m@email.com", PhoneNumber = "+33-1-2345-6789", Nationality = "France", GuestCode = GenerateGuestCode(4), IsSpecialGuest = false, RoomNumber = "103", CheckInDate = DateTime.UtcNow.AddDays(-2), CheckOutDate = DateTime.UtcNow.AddDays(5), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-2) },
                new GuestEntity { FullName = "Marco Rossi", Email = "marco.r@email.com", PhoneNumber = "+39-02-1234-5678", Nationality = "Italy", GuestCode = GenerateGuestCode(5), IsSpecialGuest = false, RoomNumber = "104", CheckInDate = DateTime.UtcNow.AddDays(-10), CheckOutDate = DateTime.UtcNow.AddDays(-2), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-2) },
                new GuestEntity { FullName = "Anna Petrov", Email = "anna.p@email.com", PhoneNumber = "+7-495-123-4567", Nationality = "Russia", GuestCode = GenerateGuestCode(6), IsSpecialGuest = false, RoomNumber = "105", CheckInDate = DateTime.UtcNow.AddDays(-1), CheckOutDate = DateTime.UtcNow.AddDays(6), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-1) },
                new GuestEntity { FullName = "David Lee", Email = "david.l@email.com", PhoneNumber = "+86-10-1234-5678", Nationality = "China", GuestCode = GenerateGuestCode(7), IsSpecialGuest = true, RoomNumber = "301", CheckInDate = DateTime.UtcNow.AddDays(-4), CheckOutDate = DateTime.UtcNow.AddDays(3), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-1) },
                new GuestEntity { FullName = "Sarah Williams", Email = "sarah.w@email.com", PhoneNumber = "+1-555-0202", Nationality = "USA", GuestCode = GenerateGuestCode(8), IsSpecialGuest = false, RoomNumber = "106", CheckInDate = DateTime.UtcNow.AddDays(-6), CheckOutDate = DateTime.UtcNow.AddDays(1), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddMonths(-1) },
                new GuestEntity { FullName = "Michael Brown", Email = "michael.b@email.com", PhoneNumber = "+1-555-0303", Nationality = "USA", GuestCode = GenerateGuestCode(9), IsSpecialGuest = false, RoomNumber = "107", CheckInDate = DateTime.UtcNow.AddDays(-8), CheckOutDate = DateTime.UtcNow.AddDays(-1), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-20) },
                new GuestEntity { FullName = "Lisa Anderson", Email = "lisa.a@email.com", PhoneNumber = "+46-8-123-4567", Nationality = "Sweden", GuestCode = GenerateGuestCode(10), IsSpecialGuest = false, RoomNumber = "108", CheckInDate = DateTime.UtcNow.AddDays(-12), CheckOutDate = DateTime.UtcNow.AddDays(-5), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-15) },
                new GuestEntity { FullName = "James Wilson", Email = "james.w@email.com", PhoneNumber = "+61-2-1234-5678", Nationality = "Australia", GuestCode = GenerateGuestCode(11), IsSpecialGuest = false, RoomNumber = "109", CheckInDate = DateTime.UtcNow.AddDays(-9), CheckOutDate = DateTime.UtcNow.AddDays(-3), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-10) },
                new GuestEntity { FullName = "Yuki Tanaka", Email = "yuki.t@email.com", PhoneNumber = "+81-3-1234-5678", Nationality = "Japan", GuestCode = GenerateGuestCode(21), IsSpecialGuest = true, RoomNumber = "601", CheckInDate = DateTime.UtcNow.AddDays(-2), CheckOutDate = DateTime.UtcNow.AddDays(5), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-2) },
                new GuestEntity { FullName = "Zoe Chen", Email = "zoe.c@email.com", PhoneNumber = "+852-2123-4567", Nationality = "Hong Kong", GuestCode = GenerateGuestCode(22), IsSpecialGuest = false, RoomNumber = "117", CheckInDate = DateTime.UtcNow.AddDays(-4), CheckOutDate = DateTime.UtcNow.AddDays(3), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-10) },
                new GuestEntity { FullName = "Lars Nilson", Email = "lars.n@email.com", PhoneNumber = "+47-22-123456", Nationality = "Norway", GuestCode = GenerateGuestCode(23), IsSpecialGuest = false, RoomNumber = "118", CheckInDate = DateTime.UtcNow.AddDays(-1), CheckOutDate = DateTime.UtcNow.AddDays(6), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-1) },
                new GuestEntity { FullName = "Elena Rodriguez", Email = "elena.r@email.com", PhoneNumber = "+52-55-1234-5678", Nationality = "Mexico", GuestCode = GenerateGuestCode(24), IsSpecialGuest = false, RoomNumber = "119", CheckInDate = DateTime.UtcNow.AddDays(-6), CheckOutDate = DateTime.UtcNow.AddDays(1), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-20) },
                new GuestEntity { FullName = "Aarav Gupta", Email = "aarav.g@email.com", PhoneNumber = "+91-22-1234-5678", Nationality = "India", GuestCode = GenerateGuestCode(25), IsSpecialGuest = false, RoomNumber = "120", CheckInDate = DateTime.UtcNow.AddDays(0), CheckOutDate = DateTime.UtcNow.AddDays(7), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-5) },
                new GuestEntity { FullName = "Isabella Silva", Email = "isabella.s@email.com", PhoneNumber = "+55-11-1234-5678", Nationality = "Brazil", GuestCode = GenerateGuestCode(26), IsSpecialGuest = true, RoomNumber = "701", CheckInDate = DateTime.UtcNow.AddDays(-3), CheckOutDate = DateTime.UtcNow.AddDays(4), HotelId = hotels.Any() ? hotels[random.Next(hotels.Count)].Id : null, CreatedDate = DateTime.UtcNow.AddDays(-3) },
                new GuestEntity { FullName = "Alexander 'VIP' Demo", Email = "alexander.demo@guestflow.local", PhoneNumber = "+90-555-DEMO-01", Nationality = "Demo Land", GuestCode = "GF-DEMO-2026", IsSpecialGuest = true, RoomNumber = "999", CheckInDate = DateTime.UtcNow.AddDays(-2), CheckOutDate = DateTime.UtcNow.AddDays(5), HotelId = hotels.FirstOrDefault()?.Id, CreatedDate = DateTime.UtcNow.AddMonths(-1) }
            };

            await _context.Guests.AddRangeAsync(guests);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{guests.Count} misafir eklendi.");
            return guests;
        }

        private async Task<List<TourEntity>> SeedToursAsync(List<CityEntity> cities)
        {
            if (await _context.Tours.AnyAsync())
            {
                _logger.LogInformation("Turlar zaten mevcut, atlanıyor.");
                return await _context.Tours.ToListAsync();
            }

            // Şehirleri veritabanından tekrar çek
            var citiesFromDb = await _context.Cities.ToListAsync();
            var kapadokya = citiesFromDb.FirstOrDefault(c => c.CityName == "Kapadokya");
            var pamukkale = citiesFromDb.FirstOrDefault(c => c.CityName == "Pamukkale");
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase));

            if (kapadokya == null || pamukkale == null || istanbul == null)
            {
                _logger.LogWarning("Gerekli şehir bulunamadı, alternatif şehirler kullanılıyor.");
                if (citiesFromDb.Count >= 3)
                {
                    kapadokya = citiesFromDb[0];
                    pamukkale = citiesFromDb[1];
                    istanbul = citiesFromDb[2];
                }
                else if (citiesFromDb.Count >= 1)
                {
                    kapadokya = citiesFromDb[0];
                    pamukkale = citiesFromDb[0];
                    istanbul = citiesFromDb[0];
                }
            }

            var tours = new List<TourEntity>
            {
                new TourEntity { Name = "Kapadokya Turu", Description = "Kapadokya bölgesini kapsayan günlük tur", CityId = kapadokya?.Id ?? 1, IsActive = true, CreatedDate = DateTime.UtcNow.AddMonths(-6) },
                new TourEntity { Name = "Pamukkale Turu", Description = "Pamukkale travertenleri ve Hierapolis antik kenti turu", CityId = pamukkale?.Id ?? 1, IsActive = true, CreatedDate = DateTime.UtcNow.AddMonths(-6) },
                new TourEntity { Name = "İstanbul Şehir Turu", Description = "İstanbul'un tarihi ve kültürel yerlerini kapsayan tur", CityId = istanbul?.Id ?? 1, IsActive = true, CreatedDate = DateTime.UtcNow.AddMonths(-5) },
                new TourEntity { Name = "Kapadokya Balon Turu", Description = "Kapadokya'da sıcak hava balonu turu", CityId = kapadokya?.Id ?? 1, IsActive = true, CreatedDate = DateTime.UtcNow.AddMonths(-5) },
                new TourEntity { Name = "Pamukkale ve Hierapolis Turu", Description = "Pamukkale ve Hierapolis antik kenti detaylı turu", CityId = pamukkale?.Id ?? 1, IsActive = true, CreatedDate = DateTime.UtcNow.AddMonths(-4) }
            };

            await _context.Tours.AddRangeAsync(tours);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{tours.Count} tur eklendi.");
            return tours;
        }

        private async Task<List<TransferEntity>> SeedTransfersAsync(
            List<GuestEntity> guests, 
            List<PersonnelEntity> personnel, 
            List<AirportEntity> airports, 
            List<VehicleEntity> vehicles,
            List<CityEntity> cities,
            List<HotelEntity> hotels,
            List<RestaurantEntity> restaurants)
        {
            if (await _context.Transfers.AnyAsync())
            {
                _logger.LogInformation("Transferler zaten mevcut, atlanıyor.");
                // Include'ları yapmadan önce null kontrolü yapıyoruz
                var existingTransfers = await _context.Transfers
                    .Where(t => t.PickupAddress != null && t.DropoffAddress != null)
                    .ToListAsync();
                
                // Include'ları manuel olarak yüklüyoruz
                foreach (var transfer in existingTransfers)
                {
                    if (transfer.GuestId > 0)
                        await _context.Entry(transfer).Reference(t => t.Guest).LoadAsync();
                    if (transfer.PersonnelId > 0)
                        await _context.Entry(transfer).Reference(t => t.Personnel).LoadAsync();
                    if (transfer.AirportId > 0)
                        await _context.Entry(transfer).Reference(t => t.Airport).LoadAsync();
                    if (transfer.VehicleId > 0)
                        await _context.Entry(transfer).Reference(t => t.Vehicle).LoadAsync();
                    if (transfer.PickupCityId > 0)
                        await _context.Entry(transfer).Reference(t => t.PickupCity).LoadAsync();
                    if (transfer.DropoffCityId > 0)
                        await _context.Entry(transfer).Reference(t => t.DropoffCity).LoadAsync();
                }
                
                return existingTransfers;
            }

            // Şehirleri ve havaalanlarını veritabanından tekrar çek (ID'lerin atandığından emin ol)
            var citiesFromDb = await _context.Cities
                .Where(c => c.CityName != null)
                .ToListAsync();
            var airportsFromDb = await _context.Airports
                .Where(a => a.Code != null && a.Name != null)
                .ToListAsync();
            
            _logger.LogInformation($"Transfer oluşturma için {citiesFromDb.Count} şehir bulundu. Şehir isimleri: {string.Join(", ", citiesFromDb.Where(c => !string.IsNullOrEmpty(c.CityName)).Select(c => $"'{c.CityName}'"))}");
            _logger.LogInformation($"Transfer oluşturma için {airportsFromDb.Count} havaalanı bulundu. Havaalanı kodları: {string.Join(", ", airportsFromDb.Where(a => !string.IsNullOrEmpty(a.Code)).Select(a => $"'{a.Code}'"))}");
            
            // Şehir aramasını case-insensitive yap
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                !string.IsNullOrEmpty(c.CityName) && (
                    c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                    c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase)));
            var antalya = citiesFromDb.FirstOrDefault(c => 
                !string.IsNullOrEmpty(c.CityName) && 
                c.CityName.Equals("Antalya", StringComparison.OrdinalIgnoreCase));
            var bodrum = citiesFromDb.FirstOrDefault(c => 
                !string.IsNullOrEmpty(c.CityName) && 
                c.CityName.Equals("Bodrum", StringComparison.OrdinalIgnoreCase));
            var istAirport = airportsFromDb.FirstOrDefault(a => 
                !string.IsNullOrEmpty(a.Code) && 
                a.Code.Equals("IST", StringComparison.OrdinalIgnoreCase));
            var aytAirport = airportsFromDb.FirstOrDefault(a => 
                !string.IsNullOrEmpty(a.Code) && 
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
            var paymentMethods = new[] { PaymentMethod.Cash, PaymentMethod.CreditCard, PaymentMethod.RoomCharge, PaymentMethod.BankTransfer };
            var transferTypes = new[] { TransferType.AirportToHotel, TransferType.HotelToAirport, TransferType.HotelToRestaurant, TransferType.RestaurantToHotel, TransferType.HotelToCity, TransferType.Custom };

            for (int i = 0; i < 100; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var vehicle = random.Next(3) == 0 ? null : vehicles[random.Next(vehicles.Count)]; // %33 ihtimalle araç yok
                var airport = random.Next(3) == 0 ? null : (random.Next(2) == 0 ? istAirport : aytAirport); // %33 ihtimalle havaalanı yok
                var pickupCity = random.Next(3) == 0 ? null : (random.Next(2) == 0 ? istanbul : antalya); // %33 ihtimalle şehir yok
                var dropoffCity = random.Next(3) == 0 ? null : (random.Next(2) == 0 ? istanbul : antalya); // %33 ihtimalle şehir yok
                var transferDate = DateTime.UtcNow.AddDays(-random.Next(120)).AddDays(random.Next(30)); // 4 months history to 1 month future
                var price = vehicle != null ? vehicle.DailyPrice * (decimal)(0.5 + random.NextDouble()) : (decimal)(random.Next(500, 2000));
                var discount = random.Next(10) < 2 ? (decimal?)(random.Next(5, 20)) : null;
                var finalPrice = discount.HasValue ? price * (1 - discount.Value / 100) : price;
                var currency = currencies[random.Next(currencies.Length)];
                var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                var isPaymentReceived = random.Next(10) < 7; // %70 ihtimalle ödeme alındı
                var useExternalVehicle = random.Next(10) < 3; // %30 ihtimalle dışarıdan araç
                
                // Transfer type ve ilişkili entity'leri belirle
                var transferType = transferTypes[random.Next(transferTypes.Length)];
                HotelEntity? hotel = null;
                RestaurantEntity? restaurant = null;
                
                if (hotels.Any() && (transferType == TransferType.AirportToHotel || transferType == TransferType.HotelToAirport || transferType == TransferType.HotelToRestaurant || transferType == TransferType.HotelToCity))
                {
                    hotel = hotels[random.Next(hotels.Count)];
                }
                
                if (restaurants.Any() && (transferType == TransferType.HotelToRestaurant || transferType == TransferType.RestaurantToHotel))
                {
                    restaurant = restaurants[random.Next(restaurants.Count)];
                }

                transfers.Add(new TransferEntity
                {
                    PickupAddress = airport != null ? $"Airport Terminal {random.Next(1, 4)}" : (hotel != null ? $"{hotel.HotelName} - {hotel.Address}" : $"City Center, Street {random.Next(1, 100)}"),
                    DropoffAddress = hotel != null ? $"{hotel.HotelName} - {hotel.Address}" : (restaurant != null ? $"{restaurant.RestaurantName} - {restaurant.Address}" : $"{dropoffCity?.CityName ?? "City"} District, Street {random.Next(1, 100)}"),
                    TransferDate = transferDate,
                    Price = price,
                    FinalPrice = finalPrice,
                    DiscountPercentage = discount,
                    Currency = currency,
                    Status = statuses[random.Next(statuses.Length)],
                    TransferType = transferType,
                    Note = random.Next(10) < 3 ? $"Special request: {new[] { "Early check-in", "Wheelchair assistance", "Extra luggage" }[random.Next(3)]}" : null,
                    GuestId = guest.Id,
                    PersonnelId = staff.Id,
                    DriverId = staff.Id, // Assign staff as driver for now
                    AirportId = airport?.Id,
                    VehicleId = vehicle?.Id,
                    PickupCityId = pickupCity?.Id,
                    DropoffCityId = dropoffCity?.Id,
                    DriverName = random.Next(10) < 2 ? $"Driver {random.Next(1, 100)}" : null,
                    ExternalVehiclePlate = useExternalVehicle ? $"34EXT{random.Next(100, 999)}" : null,
                    ExternalDriverName = useExternalVehicle ? $"External Driver {random.Next(1, 50)}" : null,
                    ExternalDriverPhone = useExternalVehicle ? $"+90-5{random.Next(10, 99)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}" : null,
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
            List<CityEntity> cities,
            List<TourEntity> tours,
            List<VehicleEntity> vehicles,
            List<HotelEntity> hotels)
        {
            if (await _context.CityTours.AnyAsync())
            {
                _logger.LogInformation("Şehir turları zaten mevcut, atlanıyor.");
                return await _context.CityTours.ToListAsync();
            }

            // Şehirleri ve turları veritabanından tekrar çek (ID'lerin atandığından emin ol)
            var citiesFromDb = await _context.Cities.ToListAsync();
            var toursFromDb = await _context.Tours.Where(t => t.IsActive).ToListAsync();
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase));
            var kapadokya = citiesFromDb.FirstOrDefault(c => c.CityName == "Kapadokya");
            var pamukkale = citiesFromDb.FirstOrDefault(c => c.CityName == "Pamukkale");

            if (istanbul == null || kapadokya == null || pamukkale == null)
            {
                _logger.LogError("Gerekli şehir bulunamadı! İstanbul, Kapadokya veya Pamukkale eksik.");
                throw new Exception("Seed işlemi için gerekli şehirler bulunamadı.");
            }

            if (toursFromDb.Count == 0)
            {
                _logger.LogError("Hiç tur bulunamadı!");
                throw new Exception("Seed işlemi için turlar bulunamadı.");
            }

            var cityTours = new List<CityTourEntity>();
            var random = new Random();
            var languages = new[] { "English", "Turkish", "German", "French", "Spanish" };
            var currencies = new[] { "TRY", "USD", "EUR" };

            for (int i = 0; i < 15; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var city = new[] { istanbul, kapadokya, pamukkale }[random.Next(3)];
                var tour = toursFromDb.Where(t => t.CityId == city.Id).FirstOrDefault() ?? toursFromDb[random.Next(toursFromDb.Count)];
                var vehicle = random.Next(3) == 0 ? null : vehicles[random.Next(vehicles.Count)]; // %33 ihtimalle araç yok
                var pickupHotel = hotels.Any() && random.Next(10) < 7 ? hotels[random.Next(hotels.Count)] : null; // %70 ihtimalle otel pickup
                var tourDate = DateTime.UtcNow.AddDays(-random.Next(45));
                var duration = new[] { 4, 6, 8 }[random.Next(3)];
                var price = duration * 500;
                var discount = random.Next(10) < 3 ? (decimal?)(random.Next(5, 15)) : null;
                var finalPrice = discount.HasValue ? price * (1 - discount.Value / 100) : price;
                var currency = currencies[random.Next(currencies.Length)];
                var startTime = new TimeSpan(random.Next(8, 12), random.Next(0, 60), 0);
                var endTime = startTime.Add(TimeSpan.FromHours(duration));
                var useExternalVehicle = random.Next(10) < 2; // %20 ihtimalle dışarıdan araç

                cityTours.Add(new CityTourEntity
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
                    TourId = tour.Id,
                    VehicleId = vehicle?.Id,
                    PickupHotelId = pickupHotel?.Id,
                    PickupLocation = pickupHotel != null ? $"{pickupHotel.HotelName} - {pickupHotel.Address}" : $"City Center, {city.CityName}",
                    DropoffLocation = pickupHotel != null ? $"{pickupHotel.HotelName} - {pickupHotel.Address}" : $"City Center, {city.CityName}",
                    DriverName = random.Next(10) < 3 ? $"Driver {random.Next(1, 100)}" : null,
                    GuideName = random.Next(10) < 5 ? $"Guide {random.Next(1, 50)}" : null,
                    GuidePhone = random.Next(10) < 5 ? $"+90-5{random.Next(10, 99)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}" : null,
                    ExternalVehiclePlate = useExternalVehicle ? $"34EXT{random.Next(100, 999)}" : null,
                    ExternalDriverName = useExternalVehicle ? $"External Driver {random.Next(1, 50)}" : null,
                    ExternalDriverPhone = useExternalVehicle ? $"+90-5{random.Next(10, 99)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}" : null,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedDate = tourDate.AddDays(-random.Next(1, 7))
                });
            }

            await _context.CityTours.AddRangeAsync(cityTours);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{cityTours.Count} şehir turu eklendi.");
            return cityTours;
        }

        private async Task<List<YachtTourEntity>> SeedYachtToursAsync(
            List<GuestEntity> guests, 
            List<PersonnelEntity> personnel, 
            List<CityEntity> cities,
            List<HotelEntity> hotels)
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

            var yachtTours = new List<YachtTourEntity>();
            var random = new Random();
            var yachtNames = new[] { "Blue Dream", "Sea Breeze", "Ocean Star", "Sunset", "Azure", "Crystal", "Pearl", "Dolphin" };
            var currencies = new[] { "TRY", "USD", "EUR" };
            var pierNames = new[] { "Main Pier", "Yacht Club Pier", "Marina Pier", "Harbor Pier", "Port Pier" };

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
                var startTime = new TimeSpan(random.Next(9, 11), random.Next(0, 60), 0);
                var endTime = startTime.Add(TimeSpan.FromHours(random.Next(4, 8)));

                yachtTours.Add(new YachtTourEntity
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
                    PickupPier = $"{city.CityName} {pierNames[random.Next(pierNames.Length)]}",
                    DropoffPier = $"{city.CityName} {pierNames[random.Next(pierNames.Length)]}",
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedDate = tourDate.AddDays(-random.Next(1, 10))
                });
            }

            await _context.YachtTours.AddRangeAsync(yachtTours);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{yachtTours.Count} yat turu eklendi.");
            return yachtTours;
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
            var invoiceItems = new List<InvoiceItemEntity>();
            var random = new Random();
            var invoiceNumber = 1000;
            var currencies = new[] { "TRY", "USD", "EUR" };

            // Transfer invoices (single service per invoice for compatibility)
            foreach (var transfer in transfers.Take(8))
            {
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = transfer.TransferDate.AddDays(random.Next(0, 3)),
                    TotalAmount = transfer.FinalPrice,
                    Currency = transfer.Currency ?? "TRY",
                    Notes = $"Transfer invoice for {transfer.PickupAddress} to {transfer.DropoffAddress}",
                    PdfUrl = $"",
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    CreatedDate = transfer.TransferDate
                };
                invoices.Add(invoice);

                // Add invoice item with reference for matching
                invoiceItems.Add(new InvoiceItemEntity
                {
                    ServiceType = "Transfer",
                    ServiceId = transfer.Id,
                    Amount = transfer.FinalPrice,
                    Currency = transfer.Currency ?? "TRY",
                    Notes = $"Transfer:{transfer.Id}:service", // Include service ID for matching
                    CreatedDate = transfer.TransferDate
                });
            }

            // City tour invoices (single service per invoice for compatibility)
            foreach (var tour in cityTours.Take(6))
            {
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = tour.TourDate.AddDays(random.Next(-2, 1)),
                    TotalAmount = tour.FinalPrice,
                    Currency = tour.Currency ?? "TRY",
                    Notes = $"City tour invoice - {tour.DurationHours} hours",
                    PdfUrl = $"",
                    GuestId = tour.OwnerGuestId,
                    PersonnelId = tour.PersonnelId,
                    CreatedDate = tour.TourDate
                };
                invoices.Add(invoice);

                // Add invoice item
                invoiceItems.Add(new InvoiceItemEntity
                {
                    ServiceType = "CityTour",
                    ServiceId = tour.Id,
                    Amount = tour.FinalPrice,
                    Currency = tour.Currency ?? "TRY",
                    Notes = $"City tour service - {tour.DurationHours} hours",
                    CreatedDate = tour.TourDate
                });
            }

            // Yacht tour invoices (single service per invoice for compatibility)
            foreach (var tour in yachtTours.Take(5))
            {
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = tour.TourDate.AddDays(random.Next(-2, 1)),
                    TotalAmount = tour.FinalPrice,
                    Currency = tour.Currency ?? "TRY",
                    Notes = $"Yacht tour invoice - {tour.YachtName ?? "Unknown"}",
                    PdfUrl = $"",
                    GuestId = tour.OwnerGuestId,
                    PersonnelId = tour.PersonnelId,
                    CreatedDate = tour.TourDate
                };
                invoices.Add(invoice);

                // Add invoice item
                invoiceItems.Add(new InvoiceItemEntity
                {
                    ServiceType = "YachtTour",
                    ServiceId = tour.Id,
                    Amount = tour.FinalPrice,
                    Currency = tour.Currency ?? "TRY",
                    Notes = $"Yacht tour service - {tour.YachtName ?? "Unknown"}",
                    CreatedDate = tour.TourDate
                });
            }

            // Create one multi-service invoice example
            InvoicesEntity? multiServiceInvoice = null;
            if (transfers.Any() && cityTours.Any())
            {
                multiServiceInvoice = new InvoicesEntity
                {
                    InvoiceNumber = invoiceNumber++,
                    IssueDate = DateTime.UtcNow.AddDays(-5),
                    TotalAmount = 0, // Will be calculated from items
                    Currency = "TRY",
                    Notes = "Multi-service invoice example (checkout batch)",
                    PdfUrl = "",
                    GuestId = transfers.First().GuestId,
                    PersonnelId = transfers.First().PersonnelId,
                    CreatedDate = DateTime.UtcNow.AddDays(-5)
                };
                invoices.Add(multiServiceInvoice);

                // Add multiple services to this invoice
                var transferForMulti = transfers.Where(t => t.GuestId == multiServiceInvoice.GuestId).FirstOrDefault();
                var cityTourForMulti = cityTours.Where(ct => ct.OwnerGuestId == multiServiceInvoice.GuestId).FirstOrDefault();

                if (transferForMulti != null)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        ServiceType = "Transfer",
                        ServiceId = transferForMulti.Id,
                        Amount = transferForMulti.FinalPrice,
                        Currency = transferForMulti.Currency ?? "TRY",
                        Notes = $"Transfer: {transferForMulti.PickupAddress} → {transferForMulti.DropoffAddress}",
                        CreatedDate = multiServiceInvoice.CreatedDate
                    });
                    multiServiceInvoice.TotalAmount += transferForMulti.FinalPrice;
                }

                if (cityTourForMulti != null)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        ServiceType = "CityTour",
                        ServiceId = cityTourForMulti.Id,
                        Amount = cityTourForMulti.FinalPrice,
                        Currency = cityTourForMulti.Currency ?? "TRY",
                        Notes = $"City Tour: {cityTourForMulti.DurationHours} hours",
                        CreatedDate = multiServiceInvoice.CreatedDate
                    });
                    multiServiceInvoice.TotalAmount += cityTourForMulti.FinalPrice;
                }
            }

            await _context.Invoices.AddRangeAsync(invoices);
            await _context.SaveChangesAsync();

            // Now create invoice items with proper InvoiceId references
            var transferInvoices = invoices.Where(i => i.Notes.Contains("Transfer invoice")).ToList();
            var cityTourInvoices = invoices.Where(i => i.Notes.Contains("City tour invoice")).ToList();
            var yachtTourInvoices = invoices.Where(i => i.Notes.Contains("Yacht tour invoice")).ToList();
            // multiServiceInvoice is already declared above

            // Create items for single-service invoices
            foreach (var invoice in transferInvoices)
            {
                // Find the transfer that matches this invoice's guest and date
                var transfer = transfers.FirstOrDefault(t => t.GuestId == invoice.GuestId &&
                                                           Math.Abs((t.TransferDate - invoice.CreatedDate).TotalDays) < 1);
                if (transfer != null)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        InvoiceId = invoice.Id,
                        ServiceType = "Transfer",
                        ServiceId = transfer.Id,
                        Amount = transfer.FinalPrice,
                        Currency = transfer.Currency ?? "TRY",
                        Notes = $"Transfer service",
                        CreatedDate = transfer.TransferDate
                    });
                }
            }

            foreach (var invoice in cityTourInvoices)
            {
                var tour = cityTours.FirstOrDefault(ct => ct.OwnerGuestId == invoice.GuestId &&
                                                        Math.Abs((ct.TourDate - invoice.CreatedDate).TotalDays) < 1);
                if (tour != null)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        InvoiceId = invoice.Id,
                        ServiceType = "CityTour",
                        ServiceId = tour.Id,
                        Amount = tour.FinalPrice,
                        Currency = tour.Currency ?? "TRY",
                        Notes = $"City tour service - {tour.DurationHours} hours",
                        CreatedDate = tour.TourDate
                    });
                }
            }

            foreach (var invoice in yachtTourInvoices)
            {
                var tour = yachtTours.FirstOrDefault(yt => yt.OwnerGuestId == invoice.GuestId &&
                                                         Math.Abs((yt.TourDate - invoice.CreatedDate).TotalDays) < 1);
                if (tour != null)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        InvoiceId = invoice.Id,
                        ServiceType = "YachtTour",
                        ServiceId = tour.Id,
                        Amount = tour.FinalPrice,
                        Currency = tour.Currency ?? "TRY",
                        Notes = $"Yacht tour service - {tour.YachtName ?? "Unknown"}",
                        CreatedDate = tour.TourDate
                    });
                }
            }

            // Create items for multi-service invoice
            if (multiServiceInvoice != null)
            {
                var guestTransfers = transfers.Where(t => t.GuestId == multiServiceInvoice.GuestId).Take(1);
                var guestCityTours = cityTours.Where(ct => ct.OwnerGuestId == multiServiceInvoice.GuestId).Take(1);

                foreach (var transfer in guestTransfers)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        InvoiceId = multiServiceInvoice.Id,
                        ServiceType = "Transfer",
                        ServiceId = transfer.Id,
                        Amount = transfer.FinalPrice,
                        Currency = transfer.Currency ?? "TRY",
                        Notes = $"Transfer: {transfer.PickupAddress} → {transfer.DropoffAddress}",
                        CreatedDate = multiServiceInvoice.CreatedDate
                    });
                }

                foreach (var cityTour in guestCityTours)
                {
                    invoiceItems.Add(new InvoiceItemEntity
                    {
                        InvoiceId = multiServiceInvoice.Id,
                        ServiceType = "CityTour",
                        ServiceId = cityTour.Id,
                        Amount = cityTour.FinalPrice,
                        Currency = cityTour.Currency ?? "TRY",
                        Notes = $"City Tour: {cityTour.DurationHours} hours",
                        CreatedDate = multiServiceInvoice.CreatedDate
                    });
                }
            }

            await _context.InvoiceItems.AddRangeAsync(invoiceItems);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"{invoices.Count} fatura ve {invoiceItems.Count} fatura öğesi eklendi.");
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
                if (invoice == null) continue;
                
                var status = paymentStatuses[random.Next(paymentStatuses.Length)];
                var method = paymentMethods[random.Next(paymentMethods.Length)];

                payments.Add(new PaymentEntity
                {
                    PaymentNumber = GeneratePaymentNumber(paymentIndex++),
                    InvoiceId = invoice.Id,
                    GuestId = invoice.GuestId,
                    Amount = invoice.TotalAmount,
                    Currency = invoice.Currency ?? "TRY",
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

        private async Task<List<HotelEntity>> SeedHotelsAsync(List<CityEntity> cities)
        {
            if (await _context.Hotels.AnyAsync())
            {
                _logger.LogInformation("Oteller zaten mevcut, atlanıyor.");
                return await _context.Hotels.ToListAsync();
            }

            var citiesFromDb = await _context.Cities.ToListAsync();
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase));
            var antalya = citiesFromDb.FirstOrDefault(c => c.CityName.Equals("Antalya", StringComparison.OrdinalIgnoreCase));
            var bodrum = citiesFromDb.FirstOrDefault(c => c.CityName.Equals("Bodrum", StringComparison.OrdinalIgnoreCase));

            if (istanbul == null || antalya == null || bodrum == null)
            {
                _logger.LogWarning("Gerekli şehir bulunamadı, alternatif şehirler kullanılıyor.");
                if (citiesFromDb.Count >= 3)
                {
                    istanbul = citiesFromDb[0];
                    antalya = citiesFromDb[1];
                    bodrum = citiesFromDb[2];
                }
            }

            var hotels = new List<HotelEntity>
            {
                new HotelEntity
                {
                    HotelName = "Grand Istanbul Hotel",
                    Address = "Sultanahmet Square, Fatih",
                    CityId = istanbul?.Id ?? citiesFromDb[0].Id,
                    Phone = "+90-212-555-0101",
                    Email = "info@grandistanbul.com",
                    StarRating = 5,
                    CheckInTime = new TimeSpan(14, 0, 0),
                    CheckOutTime = new TimeSpan(12, 0, 0),
                    RoomTypes = "Single, Double, Suite, Deluxe",
                    Amenities = "WiFi, Pool, Spa, Gym, Restaurant, Bar",
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new HotelEntity
                {
                    HotelName = "Antalya Beach Resort",
                    Address = "Lara Beach, Antalya",
                    CityId = antalya?.Id ?? citiesFromDb[1].Id,
                    Phone = "+90-242-555-0202",
                    Email = "info@antalyabeach.com",
                    StarRating = 5,
                    CheckInTime = new TimeSpan(15, 0, 0),
                    CheckOutTime = new TimeSpan(11, 0, 0),
                    RoomTypes = "Standard, Sea View, Family Room, Villa",
                    Amenities = "WiFi, Private Beach, Pool, Spa, Kids Club, Restaurant",
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new HotelEntity
                {
                    HotelName = "Bodrum Marina Hotel",
                    Address = "Marina District, Bodrum",
                    CityId = bodrum?.Id ?? citiesFromDb[2].Id,
                    Phone = "+90-252-555-0303",
                    Email = "info@bodrummarina.com",
                    StarRating = 4,
                    CheckInTime = new TimeSpan(14, 0, 0),
                    CheckOutTime = new TimeSpan(12, 0, 0),
                    RoomTypes = "Standard, Marina View, Suite",
                    Amenities = "WiFi, Pool, Restaurant, Bar, Marina Access",
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new HotelEntity
                {
                    HotelName = "Istanbul City Center Hotel",
                    Address = "Taksim Square, Beyoğlu",
                    CityId = istanbul?.Id ?? citiesFromDb[0].Id,
                    Phone = "+90-212-555-0404",
                    Email = "info@istanbulcity.com",
                    StarRating = 4,
                    CheckInTime = new TimeSpan(14, 0, 0),
                    CheckOutTime = new TimeSpan(12, 0, 0),
                    RoomTypes = "Single, Double, Triple",
                    Amenities = "WiFi, Gym, Restaurant, Business Center",
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new HotelEntity
                {
                    HotelName = "Antalya Old Town Hotel",
                    Address = "Kaleiçi, Antalya",
                    CityId = antalya?.Id ?? citiesFromDb[1].Id,
                    Phone = "+90-242-555-0505",
                    Email = "info@oldtownantalya.com",
                    StarRating = 3,
                    CheckInTime = new TimeSpan(14, 0, 0),
                    CheckOutTime = new TimeSpan(11, 0, 0),
                    RoomTypes = "Standard, Deluxe",
                    Amenities = "WiFi, Restaurant, Terrace",
                    CreatedDate = DateTime.UtcNow.AddMonths(-4)
                }
            };

            await _context.Hotels.AddRangeAsync(hotels);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{hotels.Count} otel eklendi.");
            return hotels;
        }

        private async Task<List<RestaurantEntity>> SeedRestaurantsAsync(List<CityEntity> cities)
        {
            if (await _context.Restaurants.AnyAsync())
            {
                _logger.LogInformation("Restoranlar zaten mevcut, atlanıyor.");
                return await _context.Restaurants.ToListAsync();
            }

            var citiesFromDb = await _context.Cities.ToListAsync();
            var istanbul = citiesFromDb.FirstOrDefault(c => 
                c.CityName.Equals("İstanbul", StringComparison.OrdinalIgnoreCase) || 
                c.CityName.Equals("Istanbul", StringComparison.OrdinalIgnoreCase));
            var antalya = citiesFromDb.FirstOrDefault(c => c.CityName.Equals("Antalya", StringComparison.OrdinalIgnoreCase));
            var bodrum = citiesFromDb.FirstOrDefault(c => c.CityName.Equals("Bodrum", StringComparison.OrdinalIgnoreCase));

            if (istanbul == null || antalya == null || bodrum == null)
            {
                _logger.LogWarning("Gerekli şehir bulunamadı, alternatif şehirler kullanılıyor.");
                if (citiesFromDb.Count >= 3)
                {
                    istanbul = citiesFromDb[0];
                    antalya = citiesFromDb[1];
                    bodrum = citiesFromDb[2];
                }
            }

            var restaurants = new List<RestaurantEntity>
            {
                new RestaurantEntity
                {
                    RestaurantName = "Sultan's Palace",
                    Address = "Sultanahmet, Fatih",
                    CityId = istanbul?.Id ?? citiesFromDb[0].Id,
                    Phone = "+90-212-555-1001",
                    Email = "info@sultanspalace.com",
                    CuisineType = "Turkish",
                    Capacity = 80,
                    OperatingHours = "12:00-23:00",
                    ReservationRequired = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new RestaurantEntity
                {
                    RestaurantName = "Mediterranean Breeze",
                    Address = "Lara Beach, Antalya",
                    CityId = antalya?.Id ?? citiesFromDb[1].Id,
                    Phone = "+90-242-555-1002",
                    Email = "info@medbreeze.com",
                    CuisineType = "Mediterranean",
                    Capacity = 120,
                    OperatingHours = "11:00-24:00",
                    ReservationRequired = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new RestaurantEntity
                {
                    RestaurantName = "Bodrum Seafood House",
                    Address = "Marina District, Bodrum",
                    CityId = bodrum?.Id ?? citiesFromDb[2].Id,
                    Phone = "+90-252-555-1003",
                    Email = "info@bodrumseafood.com",
                    CuisineType = "Seafood",
                    Capacity = 60,
                    OperatingHours = "18:00-23:00",
                    ReservationRequired = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new RestaurantEntity
                {
                    RestaurantName = "Istanbul Grill",
                    Address = "Taksim Square, Beyoğlu",
                    CityId = istanbul?.Id ?? citiesFromDb[0].Id,
                    Phone = "+90-212-555-1004",
                    Email = "info@istanbulgrill.com",
                    CuisineType = "Turkish BBQ",
                    Capacity = 100,
                    OperatingHours = "12:00-23:00",
                    ReservationRequired = false,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new RestaurantEntity
                {
                    RestaurantName = "Antalya Rooftop",
                    Address = "Kaleiçi, Antalya",
                    CityId = antalya?.Id ?? citiesFromDb[1].Id,
                    Phone = "+90-242-555-1005",
                    Email = "info@antalyarooftop.com",
                    CuisineType = "International",
                    Capacity = 50,
                    OperatingHours = "19:00-24:00",
                    ReservationRequired = true,
                    CreatedDate = DateTime.UtcNow.AddMonths(-4)
                }
            };

            await _context.Restaurants.AddRangeAsync(restaurants);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{restaurants.Count} restoran eklendi.");
            return restaurants;
        }

        private async Task<List<RestaurantReservationEntity>> SeedRestaurantReservationsAsync(
            List<GuestEntity> guests,
            List<PersonnelEntity> personnel,
            List<RestaurantEntity> restaurants,
            List<HotelEntity> hotels,
            List<VehicleEntity> vehicles)
        {
            if (await _context.RestaurantReservations.AnyAsync())
            {
                _logger.LogInformation("Restoran rezervasyonları zaten mevcut, atlanıyor.");
                return await _context.RestaurantReservations.ToListAsync();
            }

            var reservations = new List<RestaurantReservationEntity>();
            var random = new Random();
            var statuses = new[] { ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.Completed, ReservationStatus.Cancelled };

            for (int i = 0; i < 15; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var restaurant = restaurants[random.Next(restaurants.Count)];
                var reservationDate = DateTime.UtcNow.AddDays(-random.Next(30));
                var reservationTime = new TimeSpan(random.Next(18, 22), random.Next(0, 60), 0);
                var numberOfGuests = random.Next(2, 8);
                var status = statuses[random.Next(statuses.Length)];
                var createTransfer = random.Next(10) < 5; // %50 ihtimalle transfer oluştur

                TransferEntity? transfer = null;
                TransferEntity? returnTransfer = null;

                if (createTransfer && hotels.Any() && vehicles.Any())
                {
                    var hotel = hotels[random.Next(hotels.Count)];
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var transferDate = reservationDate;
                    var transferTime = reservationTime.Subtract(TimeSpan.FromHours(1));
                    var transferDateTime = transferDate.Date.Add(transferTime);

                    transfer = new TransferEntity
                    {
                        PickupAddress = $"{hotel.HotelName} - {hotel.Address}",
                        DropoffAddress = $"{restaurant.RestaurantName} - {restaurant.Address}",
                        TransferDate = transferDateTime,
                        Price = vehicle.DailyPrice * 0.3m,
                        FinalPrice = vehicle.DailyPrice * 0.3m,
                        Currency = "TRY",
                        Status = "Completed",
                        TransferType = TransferType.HotelToRestaurant,
                        GuestId = guest.Id,
                        PersonnelId = staff.Id,
                        DriverId = staff.Id,
                        VehicleId = vehicle.Id,
                        CreatedDate = transferDateTime.AddDays(-random.Next(1, 3))
                    };

                    // Return transfer
                    var returnTime = reservationTime.Add(TimeSpan.FromHours(2));
                    var returnDateTime = reservationDate.Date.Add(returnTime);

                    returnTransfer = new TransferEntity
                    {
                        PickupAddress = $"{restaurant.RestaurantName} - {restaurant.Address}",
                        DropoffAddress = $"{hotel.HotelName} - {hotel.Address}",
                        TransferDate = returnDateTime,
                        Price = vehicle.DailyPrice * 0.3m,
                        FinalPrice = vehicle.DailyPrice * 0.3m,
                        Currency = "TRY",
                        Status = "Pending",
                        TransferType = TransferType.RestaurantToHotel,
                        GuestId = guest.Id,
                        PersonnelId = staff.Id,
                        DriverId = staff.Id,
                        VehicleId = vehicle.Id,
                        CreatedDate = returnDateTime.AddDays(-random.Next(1, 3))
                    };

                    await _context.Transfers.AddRangeAsync(new[] { transfer, returnTransfer });
                    await _context.SaveChangesAsync(); // ID'lerin atanması için
                }

                var confirmationNumber = $"RES{DateTime.Now.Year}{random.Next(100000, 999999)}";

                reservations.Add(new RestaurantReservationEntity
                {
                    RestaurantId = restaurant.Id,
                    GuestId = guest.Id,
                    PersonnelId = staff.Id,
                    ReservationDate = reservationDate,
                    ReservationTime = reservationTime,
                    NumberOfGuests = numberOfGuests,
                    TableNumber = random.Next(10) < 6 ? $"T{random.Next(1, 20)}" : null,
                    SpecialRequests = random.Next(10) < 3 ? new[] { "Window seat", "Vegetarian options", "Birthday celebration", "Quiet table" }[random.Next(4)] : null,
                    Status = status,
                    ConfirmationNumber = confirmationNumber,
                    TransferId = transfer?.Id,
                    ReturnTransferId = returnTransfer?.Id,
                    Notes = random.Next(10) < 4 ? $"Reservation notes: {new[] { "VIP guest", "Regular customer", "First time visitor" }[random.Next(3)]}" : null,
                    CreatedDate = reservationDate.AddDays(-random.Next(1, 7))
                });
            }

            await _context.RestaurantReservations.AddRangeAsync(reservations);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{reservations.Count} restoran rezervasyonu eklendi.");
            return reservations;
        }

        private async Task<List<ItineraryEntity>> SeedItinerariesAsync(
            List<GuestEntity> guests,
            List<PersonnelEntity> personnel,
            List<TransferEntity> transfers,
            List<CityTourEntity> cityTours,
            List<YachtTourEntity> yachtTours,
            List<RestaurantReservationEntity> restaurantReservations)
        {
            if (await _context.Itineraries.AnyAsync())
            {
                _logger.LogInformation("İtineraryler zaten mevcut, atlanıyor.");
                return await _context.Itineraries.ToListAsync();
            }

            var itineraries = new List<ItineraryEntity>();
            var random = new Random();
            var statuses = new[] { ItineraryStatus.Draft, ItineraryStatus.Confirmed, ItineraryStatus.InProgress, ItineraryStatus.Completed };
            var currencies = new[] { "TRY", "USD", "EUR" };

            for (int i = 0; i < 10; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList()[random.Next(personnel.Count(p => p.UserType == UserType.Staff))];
                var startDate = DateTime.UtcNow.AddDays(-random.Next(20));
                var endDate = startDate.AddDays(random.Next(3, 7));
                var status = statuses[random.Next(statuses.Length)];
                var currency = currencies[random.Next(currencies.Length)];
                var itineraryNumber = $"ITN{DateTime.Now.Year}{random.Next(100000, 999999)}";

                // İtinerary items oluştur
                var items = new List<ItineraryItemEntity>();
                var order = 1;

                // Transfer ekle
                if (transfers.Any())
                {
                    var transfer = transfers[random.Next(transfers.Count)];
                    items.Add(new ItineraryItemEntity
                    {
                        ItemType = ItineraryItemType.Transfer,
                        ServiceId = transfer.Id,
                        ScheduledDateTime = transfer.TransferDate,
                        Order = order++,
                        Status = transfer.Status,
                        Notes = $"Transfer from {transfer.PickupAddress} to {transfer.DropoffAddress}",
                        CreatedDate = transfer.CreatedDate
                    });
                }

                // City tour ekle
                if (cityTours.Any() && random.Next(10) < 7)
                {
                    var cityTour = cityTours[random.Next(cityTours.Count)];
                    items.Add(new ItineraryItemEntity
                    {
                        ItemType = ItineraryItemType.CityTour,
                        ServiceId = cityTour.Id,
                        ScheduledDateTime = cityTour.TourDate,
                        Order = order++,
                        Status = "Completed",
                        Notes = $"City tour - {cityTour.DurationHours} hours",
                        CreatedDate = cityTour.CreatedDate
                    });
                }

                // Yacht tour ekle
                if (yachtTours.Any() && random.Next(10) < 5)
                {
                    var yachtTour = yachtTours[random.Next(yachtTours.Count)];
                    items.Add(new ItineraryItemEntity
                    {
                        ItemType = ItineraryItemType.YachtTour,
                        ServiceId = yachtTour.Id,
                        ScheduledDateTime = yachtTour.TourDate,
                        Order = order++,
                        Status = "Completed",
                        Notes = $"Yacht tour - {yachtTour.YachtName}",
                        CreatedDate = yachtTour.CreatedDate
                    });
                }

                // Restaurant reservation ekle
                if (restaurantReservations.Any() && random.Next(10) < 6)
                {
                    var reservation = restaurantReservations[random.Next(restaurantReservations.Count)];
                    var reservationDateTime = reservation.ReservationDate.Date.Add(reservation.ReservationTime);
                    items.Add(new ItineraryItemEntity
                    {
                        ItemType = ItineraryItemType.RestaurantReservation,
                        ServiceId = reservation.Id,
                        ScheduledDateTime = reservationDateTime,
                        Order = order++,
                        Status = reservation.Status.ToString(),
                        Notes = $"Restaurant reservation - {reservation.NumberOfGuests} guests",
                        CreatedDate = reservation.CreatedDate
                    });
                }

                // Total cost hesapla
                var totalCost = items.Sum(item =>
                {
                    return item.ItemType switch
                    {
                        ItineraryItemType.Transfer => transfers.FirstOrDefault(t => t.Id == item.ServiceId)?.FinalPrice ?? 0,
                        ItineraryItemType.CityTour => cityTours.FirstOrDefault(ct => ct.Id == item.ServiceId)?.FinalPrice ?? 0,
                        ItineraryItemType.YachtTour => yachtTours.FirstOrDefault(yt => yt.Id == item.ServiceId)?.FinalPrice ?? 0,
                        ItineraryItemType.RestaurantReservation => 0, // Restaurant reservations don't have price in this context
                        _ => 0
                    };
                });

                var itinerary = new ItineraryEntity
                {
                    GuestId = guest.Id,
                    PersonnelId = staff.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    TotalCost = totalCost,
                    Currency = currency,
                    ItineraryNumber = itineraryNumber,
                    Notes = random.Next(10) < 4 ? $"Itinerary notes: {new[] { "VIP guest", "Special requests", "Group booking" }[random.Next(3)]}" : null,
                    CreatedDate = startDate.AddDays(-random.Next(1, 5))
                };

                await _context.Itineraries.AddAsync(itinerary);
                await _context.SaveChangesAsync();

                // Items'ı itinerary'ye bağla
                foreach (var item in items)
                {
                    item.ItineraryId = itinerary.Id;
                }

                await _context.ItineraryItems.AddRangeAsync(items);
                await _context.SaveChangesAsync();

                itineraries.Add(itinerary);
            }

            _logger.LogInformation($"{itineraries.Count} itinerary eklendi.");
            return itineraries;
        }

        private async Task<List<ServicePackageEntity>> SeedServicePackagesAsync(
            List<TransferEntity> transfers,
            List<CityTourEntity> cityTours,
            List<YachtTourEntity> yachtTours,
            List<RestaurantReservationEntity> restaurantReservations)
        {
            if (await _context.ServicePackages.AnyAsync())
            {
                _logger.LogInformation("Servis paketleri zaten mevcut, atlanıyor.");
                return await _context.ServicePackages.ToListAsync();
            }

            var packages = new List<ServicePackageEntity>();
            var random = new Random();
            var packageTypes = new[] { PackageType.Standard, PackageType.Premium, PackageType.VIP };
            var currencies = new[] { "TRY", "USD", "EUR" };

            for (int i = 0; i < 5; i++)
            {
                var packageType = packageTypes[random.Next(packageTypes.Length)];
                var currency = currencies[random.Next(currencies.Length)];
                var startDate = DateTime.UtcNow.AddDays(-random.Next(10));
                var endDate = startDate.AddDays(random.Next(3, 7));
                var discountPercentage = random.Next(10) < 4 ? (decimal?)(random.Next(10, 25)) : null;

                // Paket içeriği seç
                var selectedTransfers = transfers.OrderBy(x => random.Next()).Take(random.Next(1, 4)).ToList();
                var selectedCityTours = cityTours.OrderBy(x => random.Next()).Take(random.Next(1, 3)).ToList();
                var selectedYachtTours = yachtTours.OrderBy(x => random.Next()).Take(random.Next(0, 2)).ToList();
                var selectedReservations = restaurantReservations.OrderBy(x => random.Next()).Take(random.Next(0, 3)).ToList();

                var totalPrice = selectedTransfers.Sum(t => t.FinalPrice) +
                                selectedCityTours.Sum(ct => ct.FinalPrice) +
                                selectedYachtTours.Sum(yt => yt.FinalPrice);
                var finalPrice = discountPercentage.HasValue 
                    ? totalPrice * (1 - discountPercentage.Value / 100) 
                    : totalPrice;

                var package = new ServicePackageEntity
                {
                    PackageName = $"{packageType} Package {i + 1}",
                    Description = $"Comprehensive {packageType} package including transfers, tours, and restaurant reservations",
                    PackageType = packageType,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = totalPrice,
                    DiscountPercentage = discountPercentage,
                    FinalPrice = finalPrice,
                    Currency = currency,
                    IsActive = true,
                    PackageContent = JsonSerializer.Serialize(new
                    {
                        transfers = selectedTransfers.Select(t => t.Id).ToList(),
                        cityTours = selectedCityTours.Select(ct => ct.Id).ToList(),
                        yachtTours = selectedYachtTours.Select(yt => yt.Id).ToList(),
                        restaurantReservations = selectedReservations.Select(r => r.Id).ToList()
                    }),
                    Notes = random.Next(10) < 5 ? $"Package notes: {new[] { "Popular package", "Seasonal offer", "Group discount" }[random.Next(3)]}" : null,
                    CreatedDate = startDate.AddDays(-random.Next(1, 5))
                };

                await _context.ServicePackages.AddAsync(package);
                await _context.SaveChangesAsync();

                // Package relationships
                foreach (var transfer in selectedTransfers)
                {
                    await _context.PackageTransfers.AddAsync(new Domain.Entities.Operations.PackageTransferEntity
                    {
                        PackageId = package.Id,
                        TransferId = transfer.Id
                    });
                }

                foreach (var cityTour in selectedCityTours)
                {
                    await _context.PackageCityTours.AddAsync(new Domain.Entities.Operations.PackageCityTourEntity
                    {
                        PackageId = package.Id,
                        CityTourId = cityTour.Id
                    });
                }

                foreach (var yachtTour in selectedYachtTours)
                {
                    await _context.PackageYachtTours.AddAsync(new Domain.Entities.Operations.PackageYachtTourEntity
                    {
                        PackageId = package.Id,
                        YachtTourId = yachtTour.Id
                    });
                }

                foreach (var reservation in selectedReservations)
                {
                    await _context.PackageRestaurantReservations.AddAsync(new Domain.Entities.Operations.PackageRestaurantReservationEntity
                    {
                        PackageId = package.Id,
                        RestaurantReservationId = reservation.Id
                    });
                }

                await _context.SaveChangesAsync();
                packages.Add(package);
            }

            _logger.LogInformation($"{packages.Count} servis paketi eklendi.");
            return packages;
        }

        private async Task<List<RoomAssignmentEntity>> SeedRoomAssignmentsAsync(List<GuestEntity> guests, List<HotelEntity> hotels)
        {
            var roomAssignments = new List<RoomAssignmentEntity>();
            var random = new Random(42);

            foreach (var guest in guests)
            {
                // Create room assignment based on guest's RoomNumber and stay dates
                if (!string.IsNullOrEmpty(guest.RoomNumber) && guest.CheckInDate.HasValue)
                {
                    var assignment = new RoomAssignmentEntity
                    {
                        GuestId = guest.Id,
                        HotelId = guest.HotelId,
                        RoomNumber = guest.RoomNumber,
                        StartDate = guest.CheckInDate.Value,
                        EndDate = guest.CheckOutDate,
                        Source = "Demo Seed",
                        Notes = $"Demo room assignment for {guest.FullName}",
                        CreatedDate = DateTime.UtcNow.AddMonths(-3),
                        CreatedByPersonnelId = 1 // Admin personnel
                    };

                    await _context.RoomAssignments.AddAsync(assignment);
                    roomAssignments.Add(assignment);

                    // For some guests, simulate room changes during stay
                    if (random.Next(100) < 30 && guest.CheckOutDate.HasValue) // 30% chance
                    {
                        var daysInStay = (guest.CheckOutDate.Value - guest.CheckInDate.Value).Days;
                        if (daysInStay > 3)
                        {
                            // Simulate room change midway through stay
                            var changeDate = guest.CheckInDate.Value.AddDays(daysInStay / 2);

                            // Close the first assignment
                            assignment.EndDate = changeDate.AddDays(-1);

                            // Create new assignment with different room
                            var newRoomNumber = $"{int.Parse(guest.RoomNumber) + 100}";
                            var newAssignment = new RoomAssignmentEntity
                            {
                                GuestId = guest.Id,
                                HotelId = guest.HotelId,
                                RoomNumber = newRoomNumber,
                                StartDate = changeDate,
                                EndDate = guest.CheckOutDate,
                                Source = "Demo Seed - Room Change",
                                Notes = $"Room changed from {guest.RoomNumber} to {newRoomNumber}",
                                CreatedDate = DateTime.UtcNow.AddMonths(-3),
                                CreatedByPersonnelId = 1
                            };

                            await _context.RoomAssignments.AddAsync(newAssignment);
                            roomAssignments.Add(newAssignment);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"{roomAssignments.Count} oda ataması eklendi.");
            return roomAssignments;
        }

        private async Task SeedGuestReviewsAsync(List<GuestEntity> guests, List<HotelEntity> hotels, List<RestaurantEntity> restaurants, List<TourEntity> tours)
        {
            if (await _context.GuestReviews.AnyAsync())
            {
                _logger.LogInformation("Yorumlar zaten mevcut, atlanıyor.");
                return;
            }

            var reviews = new List<GuestReview>();
            var random = new Random();
            var sentiments = new[] { "Positive", "Neutral", "Negative" };
            
            var reviewTemplates = new Dictionary<string, (string[] Positive, string[] Neutral, string[] Negative)>
            {
                { "Hotel", ( 
                    new[] { "Odamız harikaydı, temizlik kusursuz.", "Personel çok ilgiliydi, kahvaltı muhteşem.", "Deniz manzarası büyüleyici, tekrar geleceğiz." },
                    new[] { "Konum güzel ama odalar biraz eski kalmış.", "Fiyat performans açısından idare eder.", "Gürültülü bir odaydı ama personel yardımcı oldu." },
                    new[] { "Beklediğim gibi değildi, servis çok yavaş.", "Rezervasyon hatası yaşadık, profesyonel değiller.", "Temizlik konusunda ciddi sıkıntılar var." }
                )},
                { "Restaurant", (
                    new[] { "Zengin menü ve harika lezzetler.", "Garsonlar çok nazik, ambiyans süper.", "Hayatımda yediğim en iyi deniz ürünleriydi." },
                    new[] { "Yemekler orta şekerli, porsiyonlar biraz küçük.", "Fiyatlar biraz yüksek ama atmosfer güzel.", "Servis biraz yavaştı ama yemekler sıcaktı." },
                    new[] { "Yemeklerden hiç memnun kalmadık.", "Çok gürültülüydü, sohbet bile edemedik.", "Rezervasyonumuz olmasına rağmen yarım saat bekletildik." }
                )},
                { "Tour", (
                    new[] { "Rehberimiz çok bilgiliydi, tur çok keyifli geçti.", "Kapadokya balon turu ömrümde bir kez yaşanacak bir deneyimdi.", "Organizasyon kusursuzdu, her kuruşuna değdi." },
                    new[] { "Tur güzeldi ama yolda çok vakit kaybettik.", "Gidilen yerler güzel ama mola yerleri pahalıydı.", "Fena değildi ama daha detaylı anlatım olabilirdi." },
                    new[] { "Rehber çok ilgisizdi.", "Araç çok rahatsızdı ve klimalar çalışmıyordu.", "Tura geç başlandı ve planlanan yerlerin yarısına gidilemedi." }
                )}
            };

            // Seed Hotel Reviews
            foreach (var guest in guests.Take(10))
            {
                var hotel = hotels[random.Next(hotels.Count)];
                var sentiment = sentiments[random.Next(sentiments.Length)];
                var template = reviewTemplates["Hotel"];
                var comments = sentiment == "Positive" ? template.Positive : (sentiment == "Neutral" ? template.Neutral : template.Negative);
                
                reviews.Add(new GuestReview { 
                    GuestId = guest.Id, 
                    ServiceType = "Hotel", 
                    Rating = sentiment == "Positive" ? random.Next(4,6) : (sentiment == "Neutral" ? random.Next(3,5) : random.Next(1,3)),
                    Comment = comments[random.Next(comments.Length)],
                    CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                });
            }

            // Seed Restaurant Reviews
            foreach (var guest in guests.Skip(5).Take(5))
            {
                var res = restaurants[random.Next(restaurants.Count)];
                var sentiment = sentiments[random.Next(sentiments.Length)];
                var template = reviewTemplates["Restaurant"];
                var comments = sentiment == "Positive" ? template.Positive : (sentiment == "Neutral" ? template.Neutral : template.Negative);

                reviews.Add(new GuestReview { 
                    GuestId = guest.Id, 
                    ServiceType = "Restaurant", 
                    Rating = sentiment == "Positive" ? random.Next(4,6) : (sentiment == "Neutral" ? random.Next(3,5) : random.Next(1,3)),
                    Comment = comments[random.Next(comments.Length)],
                    CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 45))
                });
            }

            await _context.GuestReviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{reviews.Count} yorum eklendi.");
        }

        private async Task SeedGuestStaffInteractionsAsync(List<GuestEntity> guests, List<PersonnelEntity> personnel)
        {
            if (await _context.GuestStaffInteractions.AnyAsync())
            {
                _logger.LogInformation("Etkileşimler zaten mevcut, atlanıyor.");
                return;
            }

            var interactions = new List<GuestStaffInteractionEntity>();
            var random = new Random();
            var interactionTypes = new[] { "Information Request", "Maintenance Call", "Service Request", "Compliment", "Complaint" };
            var staff = personnel.Where(p => p.UserType == UserType.Staff).ToList();

            for (int i = 0; i < 20; i++)
            {
                var guest = guests[random.Next(guests.Count)];
                var staffMember = staff[random.Next(staff.Count)];
                var date = DateTime.UtcNow.AddDays(-random.Next(1, 30));

                interactions.Add(new GuestStaffInteractionEntity
                {
                    GuestId = guest.Id,
                    StaffId = staffMember.Id,
                    InteractionType = interactionTypes[random.Next(interactionTypes.Length)],
                    SatisfactionScore = random.Next(3, 6),
                    InteractionDate = date,
                    CreatedDate = date
                });
            }

            await _context.GuestStaffInteractions.AddRangeAsync(interactions);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{interactions.Count} etkileşim eklendi.");
        }
    }
}

