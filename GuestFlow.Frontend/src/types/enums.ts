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

export enum TourCategory {
  Daily = 'Daily',
  Sunset = 'Sunset'
}

export const TourCategoryLabels: Record<TourCategory, string> = {
  [TourCategory.Daily]: 'Daily Tour',
  [TourCategory.Sunset]: 'Sunset Tour'
}

