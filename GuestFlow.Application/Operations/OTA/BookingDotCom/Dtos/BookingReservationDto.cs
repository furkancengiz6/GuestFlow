using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.OTA.BookingDotCom.Dtos
{
    public class BookingWebhookPayloadDto
    {
        public string Event { get; set; } // "reservation_update", "reservation_creation"
        public BookingReservationDto Reservation { get; set; }
    }

    public class BookingReservationDto
    {
        public long Id { get; set; } // Booking.com Reservation ID
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Status { get; set; } // "new", "modified", "cancelled"
        
        public BookingGuestDto Guest { get; set; }
        public BookingStayDto Stay { get; set; }
        public BookingFinancialDto Financial { get; set; }
        
        public List<BookingRoomDto> Rooms { get; set; }
        public string Comments { get; set; }
    }

    public class BookingGuestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CountryCode { get; set; }
        public bool IsGenius { get; set; }
    }

    public class BookingStayDto
    {
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public int NumberOfGuests { get; set; }
    }

    public class BookingFinancialDto
    {
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public bool Prepaid { get; set; }
    }

    public class BookingRoomDto
    {
        public long RoomId { get; set; } // Booking.com Room Id
        public string RoomName { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal Price { get; set; }
    }
}
