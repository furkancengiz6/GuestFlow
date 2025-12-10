using AutoMapper;
using GuestFlow.Application.Operations.Airport.Dtos;
using GuestFlow.Application.Operations.City.Dtos;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Operations.DailyNote.Dtos;
using GuestFlow.Application.Operations.DailyRevenue.Dtos;
using GuestFlow.Application.Operations.Email.Dtos;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Notification.Dtos;
using GuestFlow.Application.Operations.Payment.Dtos;
using GuestFlow.Application.Operations.Personnel.Dtos;
using GuestFlow.Application.Operations.Sms.Dtos;
using GuestFlow.Application.Operations.Reservation.Dtos;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.Vehicle.Dtos;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using System.Text.Json;

namespace GuestFlow.Application.Mappings
{
    /// <summary>
    /// AutoMapper mapping profili
    /// Tüm entity-DTO mapping'lerini içerir
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Guest Mappings
            CreateMap<GuestEntity, GetGuestDto>();
            CreateMap<AddGuestDto, GuestEntity>();
            CreateMap<UpdateGuestDto, GuestEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCode, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Transfer Mappings
            CreateMap<TransferEntity, GetTransferDto>();
            CreateMap<AddTransferDto, TransferEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Guest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore())
                .ForMember(dest => dest.Airport, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PickupCity, opt => opt.Ignore())
                .ForMember(dest => dest.DropoffCity, opt => opt.Ignore());

            // Transfer Detail Mapping (complex mapping with nested objects)
            CreateMap<TransferEntity, TransferDetailDto>()
                .ForMember(dest => dest.Guest, opt => opt.MapFrom(src => src.Guest != null ? new TransferGuestDto
                {
                    Id = src.Guest.Id,
                    FullName = src.Guest.FullName,
                    GuestCode = src.Guest.GuestCode,
                    Email = src.Guest.Email,
                    PhoneNumber = src.Guest.PhoneNumber,
                    Nationality = src.Guest.Nationality,
                    IsSpecialGuest = src.Guest.IsSpecialGuest
                } : null))
                .ForMember(dest => dest.Personnel, opt => opt.MapFrom(src => src.Personnel != null ? new TransferPersonnelDto
                {
                    Id = src.Personnel.Id,
                    FullName = src.Personnel.FullName,
                    Email = src.Personnel.Email,
                    UserType = src.Personnel.UserType.ToString()
                } : null))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle != null ? new TransferVehicleDto
                {
                    Id = src.Vehicle.Id,
                    VehicleType = src.Vehicle.Type,
                    LicensePlate = src.Vehicle.PlateNumber,
                    Capacity = src.Vehicle.Capacity
                } : null));

            // CityTour Mappings
            CreateMap<CityTourEntity, GetCityTourDto>();
            CreateMap<AddCityTourDto, CityTourEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerGuest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCityTours, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore());

            // CityTour Detail Mapping
            CreateMap<CityTourEntity, CityTourDetailDto>()
                .ForMember(dest => dest.Guest, opt => opt.MapFrom(src => src.OwnerGuest != null ? new GuestFlow.Application.Operations.CityTour.Dtos.TourGuestDto
                {
                    Id = src.OwnerGuest.Id,
                    FullName = src.OwnerGuest.FullName,
                    GuestCode = src.OwnerGuest.GuestCode,
                    Email = src.OwnerGuest.Email,
                    PhoneNumber = src.OwnerGuest.PhoneNumber,
                    Nationality = src.OwnerGuest.Nationality,
                    IsSpecialGuest = src.OwnerGuest.IsSpecialGuest
                } : null))
                .ForMember(dest => dest.Personnel, opt => opt.MapFrom(src => src.Personnel != null ? new GuestFlow.Application.Operations.CityTour.Dtos.TourPersonnelDto
                {
                    Id = src.Personnel.Id,
                    FullName = src.Personnel.FullName,
                    Email = src.Personnel.Email,
                    UserType = src.Personnel.UserType.ToString()
                } : null))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City != null ? new GuestFlow.Application.Operations.CityTour.Dtos.TourCityDto
                {
                    Id = src.City.Id,
                    CityName = src.City.CityName,
                    Country = src.City.Country
                } : null));

            // YachtTour Mappings
            CreateMap<YachtTourEntity, GetYachtTourDto>();
            CreateMap<AddYachtTourDto, YachtTourEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerGuest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.GuestYachtTours, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore());

            // YachtTour Detail Mapping
            CreateMap<YachtTourEntity, YachtTourDetailDto>()
                .ForMember(dest => dest.Guest, opt => opt.MapFrom(src => src.OwnerGuest != null ? new GuestFlow.Application.Operations.YachtTour.Dtos.TourGuestDto
                {
                    Id = src.OwnerGuest.Id,
                    FullName = src.OwnerGuest.FullName,
                    GuestCode = src.OwnerGuest.GuestCode,
                    Email = src.OwnerGuest.Email,
                    PhoneNumber = src.OwnerGuest.PhoneNumber,
                    Nationality = src.OwnerGuest.Nationality,
                    IsSpecialGuest = src.OwnerGuest.IsSpecialGuest
                } : null))
                .ForMember(dest => dest.Personnel, opt => opt.MapFrom(src => src.Personnel != null ? new GuestFlow.Application.Operations.YachtTour.Dtos.TourPersonnelDto
                {
                    Id = src.Personnel.Id,
                    FullName = src.Personnel.FullName,
                    Email = src.Personnel.Email,
                    UserType = src.Personnel.UserType.ToString()
                } : null))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City != null ? new GuestFlow.Application.Operations.YachtTour.Dtos.TourCityDto
                {
                    Id = src.City.Id,
                    CityName = src.City.CityName,
                    Country = src.City.Country
                } : null));

            // Invoice Mappings
            CreateMap<InvoicesEntity, GetInvoiceDto>();
            CreateMap<InvoicesEntity, InvoiceDetailDto>()
                .ForMember(dest => dest.Guest, opt => opt.MapFrom(src => src.Guest != null ? new InvoiceGuestDto
                {
                    Id = src.Guest.Id,
                    FullName = src.Guest.FullName,
                    GuestCode = src.Guest.GuestCode,
                    Email = src.Guest.Email,
                    PhoneNumber = src.Guest.PhoneNumber
                } : null))
                .ForMember(dest => dest.Personnel, opt => opt.MapFrom(src => src.Personnel != null ? new InvoicePersonnelDto
                {
                    Id = src.Personnel.Id,
                    FullName = src.Personnel.FullName,
                    Email = src.Personnel.Email
                } : null));

            // Email Queue Mappings
            CreateMap<EmailQueueEntity, EmailQueueDto>()
                .ForMember(dest => dest.TemplateVariables, opt => opt.MapFrom((src, dest, destMember, context) =>
                    !string.IsNullOrEmpty(src.TemplateVariables)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(src.TemplateVariables)
                        : null))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom((src, dest, destMember, context) =>
                    !string.IsNullOrEmpty(src.Attachments)
                        ? src.Attachments.Split(',').ToList()
                        : null));

            // Email History Mappings
            CreateMap<EmailHistoryEntity, EmailHistoryDto>();

            // Notification Mappings
            CreateMap<NotificationEntity, NotificationDto>();

            // Personnel Mappings
            CreateMap<PersonnelEntity, PersonnelInfoDto>();

            // Reservation Mappings
            CreateMap<ReservationEntity, GetReservationDto>();
            CreateMap<AddReservationDto, ReservationEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CancelledDate, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
                .ForMember(dest => dest.Guest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore());

            CreateMap<UpdateReservationDto, ReservationEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CancelledDate, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
                .ForMember(dest => dest.Guest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore());

            CreateMap<ReservationEntity, ReservationDetailDto>()
                .ForMember(dest => dest.Guest, opt => opt.MapFrom(src => src.Guest != null ? new ReservationGuestDto
                {
                    Id = src.Guest.Id,
                    FullName = src.Guest.FullName,
                    GuestCode = src.Guest.GuestCode,
                    Email = src.Guest.Email,
                    PhoneNumber = src.Guest.PhoneNumber,
                    Nationality = src.Guest.Nationality,
                    IsSpecialGuest = src.Guest.IsSpecialGuest
                } : null))
                .ForMember(dest => dest.Personnel, opt => opt.MapFrom(src => src.Personnel != null ? new ReservationPersonnelDto
                {
                    Id = src.Personnel.Id,
                    FullName = src.Personnel.FullName,
                    Email = src.Personnel.Email,
                    UserType = src.Personnel.UserType.ToString()
                } : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReservationStatusHelper.ToString(src.Status)));

            // Payment Mappings
            CreateMap<PaymentEntity, GetPaymentDto>()
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : 0))
                .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : string.Empty))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => PaymentMethodHelper.ToString(src.PaymentMethod)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatusHelper.ToString(src.Status)));
            CreateMap<AddPaymentDto, PaymentEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionId, opt => opt.Ignore())
                .ForMember(dest => dest.GatewayResponse, opt => opt.Ignore())
                .ForMember(dest => dest.RefundDate, opt => opt.Ignore())
                .ForMember(dest => dest.RefundReason, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Guest, opt => opt.Ignore());

            CreateMap<UpdatePaymentDto, PaymentEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.GuestId, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.GatewayResponse, opt => opt.Ignore())
                .ForMember(dest => dest.RefundDate, opt => opt.Ignore())
                .ForMember(dest => dest.RefundReason, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Guest, opt => opt.Ignore());

            CreateMap<PaymentEntity, PaymentDetailDto>()
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : 0))
                .ForMember(dest => dest.InvoiceAmount, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.TotalAmount : 0))
                .ForMember(dest => dest.InvoiceCurrency, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.Currency : string.Empty))
                .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : string.Empty))
                .ForMember(dest => dest.GuestEmail, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.Email : string.Empty))
                .ForMember(dest => dest.GuestPhoneNumber, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.PhoneNumber : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatusHelper.ToString(src.Status)))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => PaymentMethodHelper.ToString(src.PaymentMethod)));

            // SMS Mappings
            CreateMap<SmsHistoryEntity, GetSmsHistoryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SmsStatusHelper.ToString(src.Status)))
                .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FullName : null))
                .ForMember(dest => dest.PersonnelName, opt => opt.MapFrom(src => src.Personnel != null ? src.Personnel.FullName : null));

            // Vehicle Mappings
            CreateMap<VehicleEntity, GetVehicleDto>();
            CreateMap<AddVehicleDto, VehicleEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Transfers, opt => opt.Ignore());
            CreateMap<UpdateVehicleDto, VehicleEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Transfers, opt => opt.Ignore());

            // Airport Mappings
            CreateMap<AirportEntity, GetAirportDto>();
            CreateMap<AddAirportDto, AirportEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore());
            CreateMap<UpdateAirportDto, AirportEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore());

            // City Mappings
            CreateMap<CityEntity, GetCityDto>();
            CreateMap<AddCityDto, CityEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Airports, opt => opt.Ignore());
            CreateMap<UpdateCityDto, CityEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Airports, opt => opt.Ignore());

            // DailyRevenue Mappings
            CreateMap<DailyRevenueEntity, GetDailyRevenueDto>();
            CreateMap<AddDailyRevenueDto, DailyRevenueEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
            CreateMap<UpdateDailyRevenueDto, DailyRevenueEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // DailyNote Mappings
            CreateMap<DailyNoteEntity, GetDailyNoteDto>();
            CreateMap<AddDailyNoteDto, DailyNoteEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore());
            CreateMap<UpdateDailyNoteDto, DailyNoteEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore());

            // Update DTO Mappings
            CreateMap<UpdateCityTourDto, CityTourEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerGuest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCityTours, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore());

            CreateMap<UpdateYachtTourDto, YachtTourEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerGuest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.GuestYachtTours, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore());

            CreateMap<UpdateTransferDto, TransferEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Guest, opt => opt.Ignore())
                .ForMember(dest => dest.Personnel, opt => opt.Ignore())
                .ForMember(dest => dest.Airport, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PickupCity, opt => opt.Ignore())
                .ForMember(dest => dest.DropoffCity, opt => opt.Ignore());
        }
    }
}
