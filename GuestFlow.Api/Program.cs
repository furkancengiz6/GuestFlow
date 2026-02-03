// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Middleware;
using GuestFlow.Api.Middlewares;
using GuestFlow.Api.Filters;
using GuestFlow.Api.Extensions;
using Serilog;
using Serilog.Events;
using GuestFlow.Domain.DataProtection;
using GuestFlow.Application.Operations.Cache;
using Microsoft.AspNetCore.Http;
using GuestFlow.Application.Operations.Airport;
using GuestFlow.Application.Operations.City;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.DailyNote;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Hotel;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Personnel;
using GuestFlow.Application.Operations.Restaurant;
using GuestFlow.Application.Operations.Itinerary;
using GuestFlow.Application.Operations.RestaurantReservation;
using GuestFlow.Application.Operations.ServicePackage;
using GuestFlow.Application.Operations.TransferRecommendation;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.GoogleMaps;
using GuestFlow.Application.Operations.QRCode;
using GuestFlow.Application.Operations.Setting;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Application.Operations.Vehicle;
using GuestFlow.Application.Operations.YachtTour;
using GuestFlow.Application.Operations.File;
using GuestFlow.Application.Operations.Auth;
using GuestFlow.Application.Operations.Password;
using GuestFlow.Application.Operations.Reports;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.Analytics;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Operations.Supplier;
using GuestFlow.Application.Operations.Profitability;
using GuestFlow.Application.Operations.Finance.Pricing;
using GuestFlow.Application.Operations.Finance.Revenue;
using GuestFlow.Application.Operations.OTA;
using GuestFlow.Application.Operations.OTA.BookingDotCom;
using GuestFlow.Application.Operations.OTA.Expedia;
using GuestFlow.Application.Operations.Reservation;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Sms;
using GuestFlow.Application.Operations.Localization;
using GuestFlow.Application.Operations.Export;
using GuestFlow.Application.Operations.Import;
using GuestFlow.Application.Operations.Calendar;
using GuestFlow.Application.Operations.Common;
using GuestFlow.Application.Configuration;
using GuestFlow.Application.Operations.Intelligence.Graph;
using GuestFlow.Application.Operations.Intelligence.Sentiment;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using GuestFlow.Application.Operations.Intelligence.Proactive;
using GuestFlow.Application.Operations.Configuration;
using GuestFlow.Application.Operations.Review;
using GuestFlow.Api.Hubs;
using GuestFlow.Api.Services;
using GuestFlow.Api.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.Repositories;
using GuestFlow.Persistence.UnitOfWork;
using GuestFlow.Persistence.MultiTenancy;
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
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/guestflow-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq(serverUrl: builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Configure background service exception behavior to not stop the host in development
builder.Services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Add services to the container.

// AutoMapper
builder.Services.AddAutoMapper(typeof(GuestFlow.Application.Mappings.MappingProfile).Assembly);

builder.Services.AddControllers(options =>
{
    // Global olarak ValidationActionFilter ekle
    options.Filters.Add<ValidationActionFilter>();
    options.Filters.Add<TenantFilter>();
})
    .AddJsonOptions(options =>
    {
        // JSON serialization'ı camelCase'e çevir (Frontend uyumluluğu için)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<AddGuestRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// CORS yapılandırması - Frontend için
// SECURITY: Enhanced CORS configuration with strict validation
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var allowedMethods = builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        var allowedHeaders = builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new[] { "Content-Type", "Authorization", "X-Requested-With", "x-signalr-user-agent" };
        var allowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials", false);
        var maxAge = builder.Configuration.GetValue<int>("Cors:MaxAge", 86400);

        // Production safety: require CORS origins to be explicitly configured
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            if (builder.Environment.IsProduction())
            {
                throw new InvalidOperationException("CORS AllowedOrigins must be configured in production. Add Cors:AllowedOrigins to appsettings.Production.json");
            }

            // Development fallback with clear warning
            allowedOrigins = new[]
            {
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:3000"
            };

            Log.Warning("Using default CORS origins for development. Configure Cors:AllowedOrigins in production.");
        }

        // SECURITY: Validate origins are HTTPS in production
        if (builder.Environment.IsProduction())
        {
            foreach (var origin in allowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                {
                    throw new InvalidOperationException($"CORS origin '{origin}' must use HTTPS in production environment.");
                }
            }
        }

        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders)
              .WithExposedHeaders("X-Total-Count", "X-Page-Count")
              .SetPreflightMaxAge(TimeSpan.FromSeconds(maxAge));

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
        else
        {
            policy.DisallowCredentials();
        }
    });
});

// API Versioning yapılandırması - Enterprise-ready
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),              // URL path: /api/v1/...
        new QueryStringApiVersionReader("version"),     // Query: ?version=1.0
        new HeaderApiVersionReader("api-version"),      // Header: api-version: 1.0
        new MediaTypeApiVersionReader("version")        // Accept: application/json; version=1.0
    );

    // Error responses for unsupported versions
    // options.ErrorResponses = new ApiVersionErrorResponseProvider();
});

// API Version 2.0 Preview (CQRS-enabled)
builder.Services.AddApiVersioning(options =>
{
    // Version 2.0 introduces CQRS patterns
    // Commands and Queries are separate endpoints
    // Domain events are published
    // Enhanced validation and security
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
builder.Services.Configure<Neo4jSettings>(builder.Configuration.GetSection("Neo4j"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimitSettings"));
builder.Services.Configure<SecurityHeadersSettings>(builder.Configuration.GetSection("SecurityHeaders"));

// Memory cache for rate limiting
builder.Services.AddMemoryCache();

// HttpContext accessor for audit logging
builder.Services.AddHttpContextAccessor();

// Audit interceptor for security logging - register as singleton
builder.Services.AddSingleton<GuestFlow.Persistence.Interceptors.AuditInterceptor>();

// Response caching for performance
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
});

// HttpClientFactory for external API calls (Google Maps, etc.)
builder.Services.AddHttpClient();

// Health checks for production readiness
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "sql" })
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "cache", "redis" });

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

// SECURITY: JWT Configuration with enhanced validation
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var minKeyLength = int.Parse(builder.Configuration["Jwt:MinimumKeyLength"] ?? "64");

// SECURITY: Validate JWT secret key exists and meets minimum length
if (string.IsNullOrWhiteSpace(jwtSecretKey))
{
    if (builder.Environment.IsDevelopment())
    {
        // Development fallback: use a default long secret to allow local runs/tests.
        jwtSecretKey = new string('x', Math.Max(minKeyLength, 128));
        builder.Configuration["Jwt:SecretKey"] = jwtSecretKey;
        Log.Warning("JWT SecretKey was not set; using development fallback secret. Do NOT use this in production.");
    }
    else
    {
        throw new InvalidOperationException("JWT SecretKey is required. Set it via environment variable JWT__SecretKey");
    }
}

if (jwtSecretKey.Length < minKeyLength)
{
    if (builder.Environment.IsDevelopment())
    {
        // Extend secret to meet minimum length in development
        jwtSecretKey = jwtSecretKey.PadRight(minKeyLength, 'x');
        builder.Configuration["Jwt:SecretKey"] = jwtSecretKey;
        Log.Warning("JWT SecretKey was shorter than minimum; padded in development.");
    }
    else
    {
        throw new InvalidOperationException($"JWT SecretKey must be at least {minKeyLength} characters long for security. Current length: {jwtSecretKey.Length}");
    }
}

// SECURITY: Additional JWT validation parameters
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // No tolerance for token expiration
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
        
        // SignalR için JWT token doğrulama
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                
                // SignalR hub'ları için token'ı query string'den veya header'dan al
                if (path.StartsWithSegments("/hubs"))
                {
                    // Önce query string'den kontrol et
                    var accessToken = context.Request.Query["access_token"];
                    
                    // Eğer query string'de yoksa, Authorization header'dan al
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            accessToken = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                }
                
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });


builder.Services.AddDbContext<GuestFlowDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(cs, x => x.MigrationsAssembly("GuestFlow.Persistence"));
    // Add audit interceptor for security logging (resolve from DI)
    var auditInterceptor = serviceProvider.GetRequiredService<GuestFlow.Persistence.Interceptors.AuditInterceptor>();
    options.AddInterceptors(auditInterceptor);
});
// Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));//generic typeof

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();


builder.Services.AddScoped<IVehicleService, VehicleManager>();
builder.Services.AddScoped<IAirportService, AirportManager>();
builder.Services.AddScoped<IPersonnelService, PersonnelManager>();
builder.Services.AddScoped<IGuestService, GuestManager>();
builder.Services.AddScoped<IGuestPreferencesService, GuestPreferencesService>();
builder.Services.AddScoped<IGuestPreferenceAnalysisService, GuestPreferenceAnalysisService>();
builder.Services.AddScoped<IRoomAssignmentService, RoomAssignmentManager>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Communication.IUnifiedCommunicationService, GuestFlow.Application.Operations.Communication.UnifiedCommunicationService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Communication.ISmartNotificationService, GuestFlow.Application.Operations.Communication.SmartNotificationService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.NotificationRules.INotificationRuleService, GuestFlow.Application.Operations.NotificationRules.NotificationRuleService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Map.IGeocodingService, GuestFlow.Application.Operations.Map.GeocodingService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Map.IMapService, GuestFlow.Application.Operations.Map.MapService>();
builder.Services.AddHttpClient<GuestFlow.Application.Operations.Map.GeocodingService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.WhatsApp.IWhatsAppService, GuestFlow.Application.Operations.WhatsApp.WhatsAppService>();

// Supplier and Profitability services
builder.Services.AddScoped<ISupplierService, SupplierManager>();
builder.Services.AddScoped<IProfitabilityService, ProfitabilityService>();

// OTA Integration services
builder.Services.AddScoped<IOTAAdapterFactory, OTAAdapterFactory>();
builder.Services.AddScoped<IOTAIntegrationService, OTAIntegrationService>();
builder.Services.AddScoped<IOTAChannelManagerService, OTAChannelManagerService>();
builder.Services.AddScoped<IOTAReservationMappingService, OTAReservationMappingService>();
builder.Services.AddScoped<IBookingDotComService, BookingDotComService>();
builder.Services.AddScoped<IExpediaService, ExpediaService>();

// PMS Integration services
builder.Services.AddScoped<GuestFlow.Application.Operations.PMS.IPMSIntegrationService, GuestFlow.Application.Operations.PMS.PMSIntegrationService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.PMS.IPMSSyncService, GuestFlow.Application.Operations.PMS.PMSSyncService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.PMS.IPMSWebhookProcessor, GuestFlow.Application.Operations.PMS.PMSWebhookProcessor>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Communication.ISmartNotificationService, GuestFlow.Application.Operations.Communication.SmartNotificationService>();

// Mock PMS Webhook Simulator (Development only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<GuestFlow.Application.Operations.PMS.IMockPMSWebhookSimulator, GuestFlow.Application.Operations.PMS.MockPMSWebhookSimulator>();
}

// Intelligence Layer - Graph Database (Neo4j)
builder.Services.AddSingleton<INeo4jService, Neo4jService>();
builder.Services.AddScoped<IGraphDataService, GraphDataService>();
builder.Services.AddScoped<IBehavioralTrackingService, BehavioralTrackingService>();
builder.Services.AddScoped<ISentimentAnalysisService, SentimentAnalysisService>();
builder.Services.AddScoped<IRelationshipIntelligenceService, RelationshipIntelligenceService>();
builder.Services.AddScoped<IPredictiveIntelligenceService, PredictiveIntelligenceService>();
builder.Services.AddScoped<IProactiveIntelligenceService, ProactiveIntelligenceService>();
builder.Services.AddScoped<IPredictiveAnalyticsService, PredictiveAnalyticsService>();

// AI Smart Concierge Services
builder.Services.AddScoped<GuestFlow.Application.Operations.AI.ContextRetriever>();
builder.Services.AddScoped<GuestFlow.Application.Operations.AI.IPIIMaskingService, GuestFlow.Application.Operations.AI.PIIMaskingService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.AI.IAICommandHandler, GuestFlow.Application.Operations.AI.AICommandHandler>();
builder.Services.AddScoped<GuestFlow.Application.Operations.AI.IAIAssistantService, GuestFlow.Application.Operations.AI.OpenAIAssistantAdapter>();
builder.Services.AddScoped<GuestFlow.Application.Operations.AI.IAIChatService, GuestFlow.Application.Operations.AI.AIChatService>();

builder.Services.AddHostedService<GuestFlow.Application.Operations.PMS.PMSPollingBackgroundService>();
// Accounting / Journal service
builder.Services.AddScoped<GuestFlow.Application.Operations.Accounting.IJournalService, GuestFlow.Application.Operations.Accounting.JournalService>();
#region Supplier cost service
builder.Services.AddScoped<GuestFlow.Application.Operations.Supplier.ISupplierCostService, GuestFlow.Application.Operations.Supplier.SupplierCostService>();
#endregion
builder.Services.AddScoped<ICityTourService, CityTourManager>(); 
builder.Services.AddScoped<ITransferService, TransferManager>();
builder.Services.AddScoped<IYachtTourService, YachtTourManager>();
builder.Services.AddScoped<IInvoiceService, InvoiceManager>();
builder.Services.AddScoped<ISettingsService, SettingManager>();
builder.Services.AddScoped<INotificationHubService, GuestFlow.Api.Services.NotificationHubService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICityService, CityManager>();
builder.Services.AddScoped<IHotelService, HotelManager>();
builder.Services.AddScoped<IRestaurantService, RestaurantManager>();
builder.Services.AddScoped<IBusinessRuleValidator, BusinessRuleValidator>();
builder.Services.AddScoped<IItineraryService, ItineraryManager>();
builder.Services.AddScoped<IRestaurantReservationService, RestaurantReservationManager>();
builder.Services.AddScoped<IServicePackageService, ServicePackageManager>();
builder.Services.AddScoped<ITransferRecommendationService, TransferRecommendationService>();
builder.Services.AddScoped<IAutomaticNotificationService, AutomaticNotificationService>();
builder.Services.AddScoped<IGoogleMapsService, GoogleMapsService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Auth.ITwoFactorService, GuestFlow.Application.Operations.Auth.TwoFactorService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Auth.IBruteForceProtectionService, GuestFlow.Application.Operations.Auth.BruteForceProtectionService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Auth.ILoginAuditService, GuestFlow.Application.Operations.Auth.LoginAuditService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Privacy.IPIIManagementService, GuestFlow.Application.Operations.Privacy.PIIManagementService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.FeatureFlags.IFeatureFlagService, GuestFlow.Application.Operations.FeatureFlags.FeatureFlagService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Authorization.IPermissionService, GuestFlow.Application.Operations.Authorization.PermissionService>();
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
builder.Services.AddHostedService<ServiceConfirmationBackgroundService>();
builder.Services.AddHostedService<PaymentReminderBackgroundService>();
builder.Services.AddHostedService<GuestFlow.Application.Operations.Communication.SmartNotificationBackgroundService>();
builder.Services.AddHostedService<GuestFlow.Application.Operations.NotificationRules.NotificationRuleBackgroundService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileShareService, FileShareService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IConciergeDashboardService, ConciergeDashboardService>();
builder.Services.AddScoped<IQuickActionService, QuickActionService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Production.IProductionConfigurationValidator, GuestFlow.Application.Operations.Production.ProductionConfigurationValidator>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Production.IMigrationDriftChecker, GuestFlow.Application.Operations.Production.MigrationDriftChecker>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Production.IDependencyVulnerabilityChecker, GuestFlow.Application.Operations.Production.DependencyVulnerabilityChecker>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Production.IDatabaseBackupService, GuestFlow.Application.Operations.Production.DatabaseBackupService>();
builder.Services.AddScoped<DailyOperationsService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IForeignKeyValidationService, ForeignKeyValidationService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IPdfUrlService, PdfUrlService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Tour.ITourService, GuestFlow.Application.Operations.Tour.TourService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentStatusService, PaymentStatusService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IPriceCalculationService, PriceCalculationService>();
builder.Services.AddScoped<IDynamicPricingService, DynamicPricingService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<GuestFlow.Application.Operations.Currency.IExchangeRateService, GuestFlow.Application.Operations.Currency.ExchangeRateService>();
builder.Services.AddScoped<IDateValidationService, DateValidationService>();
builder.Services.AddScoped<IInvoiceCreationService, InvoiceCreationService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<GuestFlow.Application.Operations.Cache.ICacheService, GuestFlow.Application.Operations.Cache.InMemoryCacheService>();
builder.Services.AddScoped<IReviewService, ReviewManager>();
builder.Services.AddScoped<InputValidationService>(); // SECURITY: Input validation service
builder.Services.AddScoped<DailyRevenueJob>();
builder.Services.AddHostedService<DailyRevenueBackgroundService>(); //
builder.Services.AddHostedService<GuestFlow.Application.Operations.OTA.OTAWebhookRetryBackgroundService>(); // OTA Webhook Retry Service

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

Console.WriteLine("Application building completed, starting middleware configuration...");

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

// Security middleware - order matters!
app.UseSecurityHeaders();
app.UseHtmlSanitization();

app.UseMantenanceMode();
app.UseHttpsRedirection();

// Response caching middleware (authentication'dan önce)
app.UseResponseCaching();

// Static dosyalar için (PDF'ler için)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub mapping
app.MapHub<GuestFlow.Api.Hubs.NotificationsHub>("/hubs/notifications");
app.MapHub<GuestFlow.Api.Hubs.AIChatHub>("/hubs/ai-chat");

// Development ortamında demo veri oluştur
// Database seeding is controlled by configuration for security
// Only seeds demo data when BOTH conditions are met:
// 1. Environment is Development
// 2. SeedDemoData configuration is true
//
// This prevents accidental demo data creation in production
var seedDemoDataString = app.Configuration["SeedDemoData"];
var seedDemoData = string.Equals(seedDemoDataString, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(seedDemoDataString, "1", StringComparison.OrdinalIgnoreCase);
if (app.Environment.IsDevelopment() && seedDemoData)
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
else if (app.Environment.IsDevelopment() && !seedDemoData)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Demo data seeding skipped. To seed demo data, set SeedDemoData=true in configuration.");
    logger.LogInformation("Demo data seeding is DISABLED by default for security reasons.");
}

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("db") || check.Tags.Contains("cache"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = (_) => false // Always healthy for liveness
});

// Detailed health check for monitoring
app.MapGet("/health/detailed", async (HealthCheckService healthCheckService) =>
{
    var report = await healthCheckService.CheckHealthAsync();
    return new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration,
        timestamp = DateTime.UtcNow,
        entries = report.Entries.ToDictionary(
            e => e.Key,
            e => new
            {
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
    };
});

Console.WriteLine("Starting application...");
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

// Expose Program class for integration tests (WebApplicationFactory)
public partial class Program { }