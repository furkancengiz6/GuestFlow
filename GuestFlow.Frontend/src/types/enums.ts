export enum UserType {
  Owner = 0,
  Manager = 1,
  Admin = 2,
  Concierge = 3,
  Reception = 4,
  Staff = 5
}

export const UserTypeLabels: Record<UserType, string> = {
  [UserType.Owner]: 'Owner',
  [UserType.Manager]: 'Manager',
  [UserType.Admin]: 'Admin',
  [UserType.Concierge]: 'Concierge',
  [UserType.Reception]: 'Reception',
  [UserType.Staff]: 'Staff'
}

export enum PaymentMethod {
  CreditCard = 1,
  BankTransfer = 2,
  Cash = 3,
  RoomCharge = 4,
  Other = 5
}

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.CreditCard]: 'Kredi Kartı',
  [PaymentMethod.BankTransfer]: 'Banka Havalesi',
  [PaymentMethod.Cash]: 'Nakit',
  [PaymentMethod.RoomCharge]: 'Odaya Charge',
  [PaymentMethod.Other]: 'Diğer'
}

export enum TransferType {
  AirportToHotel = 1,
  HotelToAirport = 2,
  HotelToRestaurant = 3,
  RestaurantToHotel = 4,
  HotelToCity = 5,
  CityToHotel = 6,
  HotelToHotel = 7,
  Custom = 8
}

export const TransferTypeLabels: Record<TransferType, string> = {
  [TransferType.AirportToHotel]: 'Havalimanı → Otel',
  [TransferType.HotelToAirport]: 'Otel → Havalimanı',
  [TransferType.HotelToRestaurant]: 'Otel → Restoran',
  [TransferType.RestaurantToHotel]: 'Restoran → Otel',
  [TransferType.HotelToCity]: 'Otel → Şehir',
  [TransferType.CityToHotel]: 'Şehir → Otel',
  [TransferType.HotelToHotel]: 'Otel → Otel',
  [TransferType.Custom]: 'Özel Transfer'
}

export enum PackageType {
  Standard = 1,
  Premium = 2,
  VIP = 3,
  Custom = 4
}

export const PackageTypeLabels: Record<PackageType, string> = {
  [PackageType.Standard]: 'Standart',
  [PackageType.Premium]: 'Premium',
  [PackageType.VIP]: 'VIP',
  [PackageType.Custom]: 'Özel'
}

export enum ReservationStatus {
  Pending = 1,
  Confirmed = 2,
  Cancelled = 3,
  Completed = 4
}

export const ReservationStatusLabels: Record<ReservationStatus, string> = {
  [ReservationStatus.Pending]: 'Beklemede',
  [ReservationStatus.Confirmed]: 'Onaylandı',
  [ReservationStatus.Cancelled]: 'İptal Edildi',
  [ReservationStatus.Completed]: 'Tamamlandı'
}

export enum TourCategory {
  Daily = 0,
  Sunset = 1
}

export const TourCategoryLabels: Record<TourCategory, string> = {
  [TourCategory.Daily]: 'Daily Tour',
  [TourCategory.Sunset]: 'Sunset Tour'
}

export enum PaymentStatus {
  Pending = 1,
  Completed = 2,
  Failed = 3,
  Refunded = 4,
  Cancelled = 5
}

export const PaymentStatusLabels: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'Beklemede',
  [PaymentStatus.Completed]: 'Tamamlandı',
  [PaymentStatus.Failed]: 'Başarısız',
  [PaymentStatus.Refunded]: 'İade Edildi',
  [PaymentStatus.Cancelled]: 'İptal Edildi'
}
