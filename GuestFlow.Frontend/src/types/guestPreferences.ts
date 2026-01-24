export interface GuestPreferences {
  id: number
  guestId: number

  // Oda Tercihleri
  preferredRoomType?: string
  roomSpecialRequests?: string
  bedPreference?: string
  smokingPreference?: string

  // Yemek Tercihleri
  dietaryPreferences?: string
  foodAllergies?: string
  specialFoodRequests?: string

  // Aktivite Tercihleri
  activityPreferences?: string
  interests?: string

  // İletişim Tercihleri
  prefersEmail: boolean
  prefersSMS: boolean
  prefersWhatsApp: boolean
  prefersPhone: boolean
  preferredLanguage?: string

  // Genel
  notes?: string
  source: string
}

export interface UpsertGuestPreferences {
  guestId: number

  // Oda Tercihleri
  preferredRoomType?: string
  roomSpecialRequests?: string
  bedPreference?: string
  smokingPreference?: string

  // Yemek Tercihleri
  dietaryPreferences?: string
  foodAllergies?: string
  specialFoodRequests?: string

  // Aktivite Tercihleri
  activityPreferences?: string
  interests?: string

  // İletişim Tercihleri
  prefersEmail: boolean
  prefersSMS: boolean
  prefersWhatsApp: boolean
  prefersPhone: boolean
  preferredLanguage?: string

  // Genel
  notes?: string
  source: string
}
