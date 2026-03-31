/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import api from './api'

// Intelligence Layer Types
export interface ProactiveRecommendation {
  guestId?: number
  recommendationType: string
  title: string
  description: string
  priority: number
  recommendedAction?: string
  recommendedDate?: string
  context?: Record<string, any>
}

export interface ProblemPreventionAlert {
  guestId?: number
  alertType: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  title: string
  description: string
  recommendedIntervention?: string
  alertDate?: string
  riskFactors?: Record<string, any>
}

export interface PersonalizationSuggestion {
  guestId?: number
  suggestionType: string
  title: string
  description: string
  confidence: number
  suggestedAction?: string
  context?: Record<string, any>
}

export interface EarlyWarningSignal {
  guestId?: number
  signalType: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  message: string
  detectedAt: string
  indicators?: Record<string, any>
}

export interface AutomaticAction {
  guestId?: number
  actionType?: string
  title: string
  description: string
  canExecuteAutomatically: boolean
  executionDetails?: string
  confidence: number
}

export interface GuestIntelligenceAction {
  id: number
  guestId: number
  actionType: string
  title: string
  description: string
  isAutomatic: boolean
  status: string
  executionDetails?: string
  confidence: number
  executionDate: string
}

export interface SentimentAnalysisResult {
  sentiment: 'Positive' | 'Neutral' | 'Negative'
  score: number
  confidence: number
  language?: string
}

export interface StaffMatchResult {
  staffId: number
  staffName: string
  compatibilityScore: number
  averageSatisfaction: number
  interactionCount: number
}

export interface ServiceMatchResult {
  serviceId: number
  serviceName: string
  serviceType: string
  matchScore: number
  recommendationReason: string
}

export interface GuestMatchResult {
  guestId: number
  guestName: string
  compatibilityScore: number
  relationshipStrength: number
  interactionCount: number
  averageSatisfaction: number
  matchReason: string
}

export interface NetworkNode {
  id: string
  type: string
  name: string
  properties?: Record<string, any>
}

export interface NetworkEdge {
  sourceId: string
  targetId: string
  relationshipType: string
  weight: number
  properties?: Record<string, any>
}

export interface RelationshipNetwork {
  guestNode: NetworkNode
  staffNodes: NetworkNode[]
  serviceNodes: NetworkNode[]
  edges: NetworkEdge[]
}

// Intelligence Service
export const intelligenceService = {
  // Sentiment Analysis
  analyzeSentiment: async (text: string, language?: string): Promise<SentimentAnalysisResult> => {
    const response = await api.post('/Intelligence/analyze-sentiment', {
      text,
      language,
    })
    return response.data.data
  },

  getGuestSentimentTrends: async (guestId: number, startDate?: string, endDate?: string) => {
    const params = new URLSearchParams()
    if (startDate) params.append('startDate', startDate)
    if (endDate) params.append('endDate', endDate)
    const response = await api.get(`/Intelligence/guests/${guestId}/sentiment-trends?${params}`)
    return response.data.data
  },

  // Behavioral Tracking
  trackGuestBehavior: async (guestId: number, behaviorType: string, data: any) => {
    const response = await api.post(`/Intelligence/guests/${guestId}/track-behavior`, {
      behaviorType,
      ...data,
    })
    return response.data.data
  },

  getGuestBehaviorPatterns: async (guestId: number, startDate?: string, endDate?: string) => {
    const params = new URLSearchParams()
    if (startDate) params.append('startDate', startDate)
    if (endDate) params.append('endDate', endDate)
    const response = await api.get(`/Intelligence/guests/${guestId}/behavior-patterns?${params}`)
    return response.data.data
  },

  getStaffBehaviorPatterns: async (staffId: number, startDate?: string, endDate?: string) => {
    const params = new URLSearchParams()
    if (startDate) params.append('startDate', startDate)
    if (endDate) params.append('endDate', endDate)
    const response = await api.get(`/Intelligence/staff/${staffId}/behavior-patterns?${params}`)
    return response.data.data
  },

  // Relationship Intelligence
  findBestStaffMatches: async (guestId: number, limit: number = 5): Promise<StaffMatchResult[]> => {
    const response = await api.get(`/Intelligence/guests/${guestId}/best-staff-matches?limit=${limit}`)
    return response.data.data
  },

  findBestGuestMatches: async (staffId: number, limit: number = 5): Promise<GuestMatchResult[]> => {
    const response = await api.get(`/Intelligence/staff/${staffId}/best-guest-matches?limit=${limit}`)
    return response.data.data
  },

  findBestServiceMatches: async (guestId: number, serviceType?: string, limit: number = 10): Promise<ServiceMatchResult[]> => {
    const params = new URLSearchParams()
    if (serviceType) params.append('serviceType', serviceType)
    params.append('limit', limit.toString())
    const response = await api.get(`/Intelligence/guests/${guestId}/best-service-matches?${params}`)
    return response.data.data
  },

  getGuestPreferencePatterns: async (guestId: number) => {
    const response = await api.get(`/Intelligence/guests/${guestId}/preference-patterns`)
    return response.data.data
  },

  getGuestRelationshipNetwork: async (guestId: number): Promise<RelationshipNetwork> => {
    const response = await api.get(`/Intelligence/guests/${guestId}/relationship-network`)
    return response.data.data
  },

  // Predictive Intelligence
  predictGuestBehavior: async (guestId: number, behaviorType: string, predictionDate?: string) => {
    const params = new URLSearchParams()
    params.append('behaviorType', behaviorType)
    if (predictionDate) params.append('predictionDate', predictionDate)
    const response = await api.get(`/Intelligence/guests/${guestId}/predict-behavior?${params}`)
    return response.data.data
  },

  predictGuestSatisfaction: async (guestId: number) => {
    const response = await api.get(`/Intelligence/guests/${guestId}/predict-satisfaction`)
    return response.data.data
  },

  identifyGuestRisks: async (guestId: number) => {
    const response = await api.get(`/Intelligence/guests/${guestId}/identify-risks`)
    return response.data.data
  },

  identifyGuestOpportunities: async (guestId: number) => {
    const response = await api.get(`/Intelligence/guests/${guestId}/detect-opportunities`)
    return response.data.data
  },

  // Proactive Intelligence
  getProactiveRecommendations: async (guestId: number, targetDate?: string): Promise<ProactiveRecommendation[]> => {
    const params = new URLSearchParams()
    if (targetDate) params.append('targetDate', targetDate)
    const response = await api.get(`/Intelligence/guests/${guestId}/proactive-recommendations?${params}`)
    return response.data.data
  },

  getProblemPreventionAlerts: async (guestId?: number): Promise<ProblemPreventionAlert[]> => {
    const params = new URLSearchParams()
    if (guestId) params.append('guestId', guestId.toString())
    const response = await api.get(`/Intelligence/problem-prevention-alerts?${params}`)
    return response.data.data
  },

  getPersonalizationSuggestions: async (guestId: number): Promise<PersonalizationSuggestion[]> => {
    const response = await api.get(`/Intelligence/guests/${guestId}/personalization-suggestions`)
    return response.data.data
  },

  getEarlyWarningSignals: async (guestId?: number): Promise<EarlyWarningSignal[]> => {
    const params = new URLSearchParams()
    if (guestId) params.append('guestId', guestId.toString())
    const response = await api.get(`/Intelligence/early-warning-signals?${params}`)
    return response.data.data
  },

  getAutomaticActions: async (guestId: number): Promise<AutomaticAction[]> => {
    const response = await api.get(`/Intelligence/guests/${guestId}/automatic-actions`)
    return response.data.data
  },

  // Dynamic Pricing Intelligence
  getPricingIntelligence: async (roomTypeId: number, startDate: string, endDate: string): Promise<PricingIntelligenceResult[]> => {
    const response = await api.get(`/pricing/intelligence?roomTypeId=${roomTypeId}&startDate=${startDate}&endDate=${endDate}`)
    return response.data.data
  },

  // Behavioral Insights (AI-extracted from Staff Notes)
  getRecentBehavioralInsights: async (guestId: number, source?: string): Promise<BehavioralInsight[]> => {
    const params = new URLSearchParams()
    if (source) params.append('source', source)
    const response = await api.get(`/Intelligence/guests/${guestId}/behavioral-insights?${params}`)
    return response.data.data
  },

  executeAutomaticAction: async (action: AutomaticAction): Promise<boolean> => {
    const response = await api.post('/Intelligence/execute-action', action)
    return response.data.data
  },

  getActionHistory: async (guestId: number): Promise<GuestIntelligenceAction[]> => {
    const response = await api.get(`/Intelligence/guests/${guestId}/history`)
    return response.data.data
  },
}

export interface AppliedRuleDetail {
  ruleName: string
  ruleType: string
  adjustmentType: string
  adjustmentValue: number
  resultingRate: number
}

export interface PricingIntelligenceResult {
  date: string
  forecastedOccupancy: number
  baseRate: number
  dynamicRate: number
  isStopSell: boolean
  appliedRules: string[]
  ruleDetails: AppliedRuleDetail[]
}

export interface BehavioralInsight {
  id: number
  guestId: number
  behaviorType: string
  category?: string
  behaviorValue?: string
  behaviorDate: string
  sentimentScore?: number
  satisfactionScore?: number
  relatedEntityType?: string
  relatedEntityId?: number
}
