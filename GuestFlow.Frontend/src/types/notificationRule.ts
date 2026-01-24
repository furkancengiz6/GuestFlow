// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

export interface NotificationRule {
  id: number
  name: string
  description?: string
  category: string
  ruleType: string
  conditions: string
  notificationChannel: string
  templateName?: string
  recipientType: string
  recipientId?: number
  isActive: boolean
  priority: number
  checkIntervalMinutes: number
  lastCheckedAt?: string
  lastTriggeredAt?: string
  triggerCount: number
  parameters?: string
  createdDate: string
  modifiedDate?: string
}

export interface UpsertNotificationRule {
  name: string
  description?: string
  category: string
  ruleType: string
  conditions: string
  notificationChannel: string
  templateName?: string
  recipientType: string
  recipientId?: number
  isActive: boolean
  priority: number
  checkIntervalMinutes: number
  parameters?: string
}

export interface RuleExecutionResult {
  ruleId: number
  ruleName: string
  triggered: boolean
  matchedEntitiesCount: number
  notificationsSent: number
  errorMessage?: string
  executedAt: string
}

export interface RuleCondition {
  entityType: string
  field: string
  operator: string
  value?: any
  conditionExpression?: string
}

// Rule type constants
export const RULE_TYPES = {
  OVERDUE_PAYMENT: 'OverduePayment',
  UPCOMING_SERVICE: 'UpcomingService',
  UNASSIGNED_DRIVER: 'UnassignedDriver',
  LOW_INVENTORY: 'LowInventory',
} as const

export const RULE_CATEGORIES = {
  PAYMENT: 'Payment',
  SERVICE: 'Service',
  ASSIGNMENT: 'Assignment',
  INVENTORY: 'Inventory',
} as const

export const NOTIFICATION_CHANNELS = {
  EMAIL: 'Email',
  SMS: 'SMS',
  IN_APP: 'InApp',
  ALL: 'All',
} as const

export const RECIPIENT_TYPES = {
  GUEST: 'Guest',
  PERSONNEL: 'Personnel',
  ADMIN: 'Admin',
  ALL: 'All',
} as const
