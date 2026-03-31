// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'
import type {
  NotificationRule,
  UpsertNotificationRule,
  RuleExecutionResult,
} from '../types/notificationRule'

export const notificationRuleService = {
  /**
   * Get all notification rules
   */
  getAllRules: async (isActive?: boolean): Promise<NotificationRule[]> => {
    const params = isActive !== undefined ? { isActive } : {}
    const response = await apiClient.get<{
      success: boolean
      data: NotificationRule[]
      message?: string
    }>('/NotificationRules', { params })
    return response.data.data
  },

  /**
   * Get notification rule by ID
   */
  getRuleById: async (id: number): Promise<NotificationRule> => {
    const response = await apiClient.get<{
      success: boolean
      data: NotificationRule
      message?: string
    }>(`/NotificationRules/${id}`)
    return response.data.data
  },

  /**
   * Create a new notification rule
   */
  createRule: async (
    rule: UpsertNotificationRule
  ): Promise<NotificationRule> => {
    const response = await apiClient.post<{
      success: boolean
      data: NotificationRule
      message?: string
    }>('/NotificationRules', rule)
    return response.data.data
  },

  /**
   * Update an existing notification rule
   */
  updateRule: async (
    id: number,
    rule: UpsertNotificationRule
  ): Promise<NotificationRule> => {
    const response = await apiClient.put<{
      success: boolean
      data: NotificationRule
      message?: string
    }>(`/NotificationRules/${id}`, rule)
    return response.data.data
  },

  /**
   * Delete a notification rule
   */
  deleteRule: async (id: number): Promise<boolean> => {
    const response = await apiClient.delete<{
      success: boolean
      data: boolean
      message?: string
    }>(`/NotificationRules/${id}`)
    return response.data.data
  },

  /**
   * Toggle rule active status
   */
  toggleRule: async (id: number, isActive: boolean): Promise<boolean> => {
    const response = await apiClient.patch<{
      success: boolean
      data: boolean
      message?: string
    }>(`/NotificationRules/${id}/toggle`, null, {
      params: { isActive },
    })
    return response.data.data
  },

  /**
   * Execute a rule manually (for testing)
   */
  executeRule: async (id: number): Promise<RuleExecutionResult> => {
    const response = await apiClient.post<{
      success: boolean
      data: RuleExecutionResult
      message?: string
    }>(`/NotificationRules/${id}/execute`)
    return response.data.data
  },
}
