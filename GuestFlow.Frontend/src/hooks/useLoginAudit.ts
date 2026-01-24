import { useQuery } from '@tanstack/react-query'
import { loginAuditService, LoginAttempt, LoginAuditStatistics, FailedLoginSummary } from '../services/loginAuditService'

export const useLoginAttempts = (params?: {
  startDate?: string
  endDate?: string
  email?: string
  ipAddress?: string
  isSuccessful?: boolean
  personnelId?: number
  pageNumber?: number
  pageSize?: number
}) => {
  return useQuery<LoginAttempt[]>({
    queryKey: ['login-attempts', params],
    queryFn: () => loginAuditService.getLoginAttempts(params),
    staleTime: 30000, // 30 seconds
  })
}

export const useLoginAuditStatistics = (params?: {
  startDate?: string
  endDate?: string
}) => {
  return useQuery<LoginAuditStatistics>({
    queryKey: ['login-audit-statistics', params],
    queryFn: () => loginAuditService.getStatistics(params),
    staleTime: 60000, // 1 minute
  })
}

export const useFailedLoginSummary = (params?: {
  startDate?: string
  endDate?: string
  topCount?: number
}) => {
  return useQuery<FailedLoginSummary[]>({
    queryKey: ['failed-login-summary', params],
    queryFn: () => loginAuditService.getFailedLoginSummary(params),
    staleTime: 60000, // 1 minute
  })
}
