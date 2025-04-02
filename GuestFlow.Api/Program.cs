using GuestFlow.Api.Middlewares;
using GuestFlow.Application.DataProtection;
using GuestFlow.Application.Operations.Airport;
using GuestFlow.Application.Operations.City;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Personnel;
using GuestFlow.Application.Operations.Setting;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Application.Operations.Vehicle;
using GuestFlow.Application.Operations.YachtTour;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Interfaces;
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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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
});

builder.Services.AddScoped<IDataProtection, DataProtection>();

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
           // ValidateIssuerSigningKey = true,
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
builder.Services.AddScoped<ICityService, CityManager>();
builder.Services.AddScoped<DailyRevenueJob>();
builder.Services.AddHostedService<DailyRevenueBackgroundService>(); //

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseGlobalExceptionHandler();// Global Exception Handler Middleware'
app.UseMantenanceMode();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
