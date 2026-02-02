/**
 * Revenue Dashboard DTO - Backend RevenueDashboardDto ile eşleşir
 */
export interface RevenueDashboard {
    adr: number;           // Average Daily Rate (Ortalama Günlük Ücret)
    revPar: number;        // Revenue Per Available Room (Mevcut Oda Başına Gelir)
    occupancyRate: number; // Doluluk Oranı (0-1 arası)
    totalRevenue: number;  // Toplam Gelir
    totalRoomsSold: number; // Satılan Toplam Oda/Gece
}
