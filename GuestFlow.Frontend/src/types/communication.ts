// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

export interface UnifiedCommunicationHistory {
  guestId: number
  guestName: string
  communications: CommunicationItem[]
  summary: CommunicationSummary
}

export interface CommunicationItem {
  id: number
  channel: 'Email' | 'SMS' | 'WhatsApp' | 'InApp'
  direction: 'Inbound' | 'Outbound'
  subject: string
  content: string
  sentDate: string
  deliveredDate?: string
  status: string
  errorMessage?: string
  templateName?: string
  relatedEntityType?: string
  relatedEntityId?: number
  provider?: string
  messageId?: string
  personnelId?: number
  personnelName?: string
  source: 'GuestFlow' | 'PMS'
}

export interface CommunicationSummary {
  totalCommunications: number
  emailCount: number
  smsCount: number
  whatsAppCount: number
  inAppCount: number
  inboundCount: number
  outboundCount: number
  lastCommunicationDate?: string
}

export interface SendMessageRequest {
  channel: 'Email' | 'SMS' | 'WhatsApp'
  subject: string
  content: string
  templateName?: string
  relatedEntityType?: string
  relatedEntityId?: number
}

export type SmartNotificationType =
  | 'PreArrival'
  | 'Arrival'
  | 'DuringStay'
  | 'PreDeparture'
  | 'SpecialOccasion'
