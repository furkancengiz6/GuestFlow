// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { notificationRuleService } from '../services/notificationRuleService'
import type {
  NotificationRule,
  UpsertNotificationRule,
  RuleExecutionResult,
} from '../types/notificationRule'

export const useNotificationRules = (isActive?: boolean) => {
  return useQuery<NotificationRule[]>({
    queryKey: ['notificationRules', isActive],
    queryFn: () => notificationRuleService.getAllRules(isActive),
  })
}

export const useNotificationRule = (id: number) => {
  return useQuery<NotificationRule>({
    queryKey: ['notificationRule', id],
    queryFn: () => notificationRuleService.getRuleById(id),
    enabled: !!id,
  })
}

export const useCreateNotificationRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (rule: UpsertNotificationRule) =>
      notificationRuleService.createRule(rule),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notificationRules'] })
    },
  })
}

export const useUpdateNotificationRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      id,
      rule,
    }: {
      id: number
      rule: UpsertNotificationRule
    }) => notificationRuleService.updateRule(id, rule),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['notificationRules'] })
      queryClient.invalidateQueries({
        queryKey: ['notificationRule', variables.id],
      })
    },
  })
}

export const useDeleteNotificationRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => notificationRuleService.deleteRule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notificationRules'] })
    },
  })
}

export const useToggleNotificationRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isActive }: { id: number; isActive: boolean }) =>
      notificationRuleService.toggleRule(id, isActive),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['notificationRules'] })
      queryClient.invalidateQueries({
        queryKey: ['notificationRule', variables.id],
      })
    },
  })
}

export const useExecuteNotificationRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => notificationRuleService.executeRule(id),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['notificationRules'] })
      queryClient.invalidateQueries({ queryKey: ['notificationRule', result.ruleId] })
    },
  })
}
