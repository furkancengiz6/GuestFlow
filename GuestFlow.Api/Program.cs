using GuestFlow.Api.Middlewares;
using GuestFlow.Api.Filters;
using GuestFlow.Api.Extensions;
using GuestFlow.Domain.DataProtection;
using Microsoft.AspNetCore.Http;
using GuestFlow.Application.Operations.Airport;
using GuestFlow.Application.Operations.City;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.DailyNote;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Personnel;
using GuestFlow.Application.Operations.Setting;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Application.Operations.Vehicle;
using GuestFlow.Application.Operations.YachtTour;
using GuestFlow.Application.Operations.File;
using GuestFlow.Application.Operations.Auth;
using GuestFlow.Application.Operations.Password;
using GuestFlow.Application.Operations.Reports;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Reservation;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Sms;
using GuestFlow.Application.Operations.Localization;
using GuestFlow.Application.Operations.Export;
using GuestFlow.Application.Operations.Import;
using GuestFlow.Application.Operations.Calendar;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Operations.Common;
using GuestFlow.Application.Configuration;
using GuestFlow.Application.Operations.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.Repositories;
using GuestFlow.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using GuestFlow.Api.Validators;
using GuestFlow.Application.Mappings;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using GuestFlow.Api.Configuration;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// AutoMapper
builder.Services.AddAutoMapper(typeof(GuestFlow.Application.Mappings.MappingProfile).Assembly);

builder.Services.AddControllers(options =>
{
    // Global olarak ValidationActionFilter ekle
    options.Filters.Add<ValidationActionFilter>();
})
    .AddJsonOptions(options =>
    {
        // JSON serialization'ı camelCase'e çevir (Frontend uyumluluğu için)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    })
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<AddGuestRequestValidator>();
        fv.AutomaticValidationEnabled = true;
        fv.ImplicitlyValidateChildProperties = true;
    });

// CORS yapılandırması - Frontend için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                             new[]
                             {
                                 "http://localhost:5173",
                                 "http://localhost:5174",
                                 "http://localhost:5175",
                                 "http://localhost:3000",
                                 "https://app.guestflow.com" // prod origin
                             };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// API Versioning yapılandırması
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),              // URL path: /api/v1/...
        new QueryStringApiVersionReader("version"),     // Query: ?version=1.0
        new HeaderApiVersionReader("api-version")      // Header: api-version: 1.0
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// File upload API description provider - IFormFile parametrelerini kaldır
// Bu, Swashbuckle'ın parametre okuma aşamasındaki hatayı önler
// ÖNEMLİ: AddEndpointsApiExplorer'dan ÖNCE eklenmelidir
builder.Services.AddSingleton<IApiDescriptionProvider, FileUploadApiDescriptionProvider>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Swagger yapılandırması - Versiyonlama desteği ile
builder.Services.AddSwaggerGen(options =>
{
    // XML dokümantasyon entegrasyonu
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Name = "Jwt Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer Token on Texbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }


    };
    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {jwtSecurityScheme, Array.Empty<string>() }
    });
    
    // File upload desteği için - önce parameter filter, sonra operation filter
    options.ParameterFilter<FileUploadParameterFilter>();
    options.OperationFilter<FileUploadOperationFilter>();
    
    // IFormFile için schema mapping - Swagger'ın parametre okuma aşamasında hatayı önler
    options.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
    
    options.MapType<IFormFile[]>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "array",
        Items = new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        }
    });
});

// Swagger'ı versiyonlama ile entegre et
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddScoped<GuestFlow.Domain.DataProtection.IDataProtection, GuestFlow.Application.DataProtection.DataProtection>();

// Configuration bindings (Options Pattern)
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PdfSettings>(builder.Configuration.GetSection("PdfSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));
builder.Services.Configure<CurrencySettings>(builder.Configuration.GetSection("CurrencySettings"));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.Configure<LocalizationSettings>(builder.Configuration.GetSection("LocalizationSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimitSettings"));

// Memory cache for rate limiting
builder.Services.AddMemoryCache();

// Localization yapılandırması
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var localizationSettings = builder.Configuration.GetSection("LocalizationSettings").Get<LocalizationSettings>();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = localizationSettings?.SupportedCultures?.Select(c => new CultureInfo(c)).ToArray() 
        ?? new[] { new CultureInfo("tr-TR"), new CultureInfo("en-US") };
    
    options.DefaultRequestCulture = new RequestCulture(
        localizationSettings?.DefaultCulture ?? "tr-TR"
    );
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Insert(1, new AcceptLanguageHeaderRequestCultureProvider());
});

var keysDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data/Keys"));

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirectory)
    .SetApplicationName("GuestFlow");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
        
    });


builder.Services.AddDbContext<GuestFlowDbContext>(options =>
    options.UseSqlServer(cs, x => x.MigrationsAssembly("GuestFlow.Persistence")));
// Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));//generic typeof

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IVehicleService, VehicleManager>();
builder.Services.AddScoped<IAirportService, AirportManager>();
builder.Services.AddScoped<IPersonnelService, PersonnelManager>();
builder.Services.AddScoped<IGuestService, GuestManager>();
builder.Services.AddScoped<ICityTourService, CityTourManager>(); 
builder.Services.AddScoped<ITransferService, TransferManager>();
builder.Services.AddScoped<IYachtTourService, YachtTourManager>();
builder.Services.AddScoped<IInvoiceService, InvoiceManager>();
builder.Services.AddScoped<ISettingsService, SettingManager>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICityService, CityManager>();
builder.Services.AddScoped<IDailyNoteService, DailyNoteManager>();
builder.Services.AddScoped<IDailyRevenueService, DailyRevenueManager>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailHistoryService, EmailHistoryService>();
builder.Services.AddScoped<IEmailStatisticsService, EmailStatisticsService>();
builder.Services.AddHostedService<EmailQueueBackgroundService>();
builder.Services.AddHostedService<RefreshTokenCleanupBackgroundService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileShareService, FileShareService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IForeignKeyValidationService, ForeignKeyValidationService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IPdfUrlService, PdfUrlService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Tour.ITourService, GuestFlow.Application.Operations.Tour.TourService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IPriceCalculationService, PriceCalculationService>();
builder.Services.AddScoped<IDateValidationService, DateValidationService>();
builder.Services.AddScoped<IInvoiceCreationService, InvoiceCreationService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<DailyRevenueJob>();
builder.Services.AddHostedService<DailyRevenueBackgroundService>(); //

var app = builder.Build();

// Localization middleware
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    
    // Swagger UI'da versiyon seçimi için
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"GuestFlow API {description.GroupName.ToUpperInvariant()}");
        }
    });
}
app.UseGlobalExceptionHandler();// Global Exception Handler Middleware'

// CORS middleware (EN ÖNCE - Preflight request'ler için)
app.UseCors("AllowFrontend");

// Rate limiting middleware (authentication'dan önce)
app.UseMiddleware<RateLimitMiddleware>();

app.UseMantenanceMode();
app.UseHttpsRedirection();

// Security headers (basic hardening)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["X-XSS-Protection"] = "0";
    await next();
});

// Static dosyalar için (PDF'ler için)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Development ortamında demo veri oluştur
if (app.Environment.IsDevelopment())
{
    try
    {
        await app.SeedDatabaseAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Demo veri oluşturulurken hata oluştu!");
    }
}

app.Run();
