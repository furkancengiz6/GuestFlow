// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

export interface SendWhatsApp {
  phoneNumber: string
  message: string
  guestId?: number
  personnelId?: number
  relatedEntityType?: string
  relatedEntityId?: number
  templateName?: string
  templateParameters?: Record<string, string>
  messageType?: WhatsAppMessageType
  richMessage?: WhatsAppRichMessage
}

export enum WhatsAppMessageType {
  Text = 1,
  Template = 2,
  Interactive = 3,
  Document = 4,
  Image = 5,
  Location = 6,
}

export interface WhatsAppRichMessage {
  headerText?: string
  bodyText?: string
  footerText?: string
  buttons?: WhatsAppButton[]
  documentUrl?: string
  documentName?: string
  imageUrl?: string
  latitude?: number
  longitude?: number
  locationName?: string
}

export interface WhatsAppButton {
  id: string
  text: string
  type: WhatsAppButtonType
  url?: string
  phoneNumber?: string
}

export enum WhatsAppButtonType {
  Reply = 1,
  Url = 2,
  Call = 3,
}

export interface WhatsAppHistory {
  id: number
  phoneNumber: string
  message: string
  status: string
  sentDate: string
  deliveredDate?: string
  readDate?: string
  guestId?: number
  guestName?: string
  personnelId?: number
  personnelName?: string
  relatedEntityType?: string
  relatedEntityId?: number
  messageId?: string
  gatewayResponse?: string
  errorMessage?: string
  messageType: WhatsAppMessageType
}

export interface WhatsAppStatistics {
  totalSent: number
  totalDelivered: number
  totalRead: number
  totalFailed: number
  totalPending: number
  successRate: number
  deliveryRate: number
  readRate: number
  messagesByType: Record<string, number>
  messagesByStatus: Record<string, number>
}
