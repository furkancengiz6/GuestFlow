/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import { useState } from 'react'
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Alert,
  AlertTitle,
  Chip,
  Button,
  CircularProgress,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Paper,
  Divider,
  Snackbar,
} from '@mui/material'
import {
  Warning,
  Lightbulb,
  AutoAwesome,
  Psychology,
  Recommend,
  NotificationsActive,
  AttachMoney,
  Description,
  QueryStats,
  Group,
  Share,
  Hub,
  History as HistoryIcon
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import {
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  ResponsiveContainer,
  AreaChart,
  Area
} from 'recharts'
import {
  intelligenceService,
  type ProactiveRecommendation,
  type ProblemPreventionAlert,
  type PersonalizationSuggestion,
  type EarlyWarningSignal,
  type AutomaticAction,
  type PricingIntelligenceResult,
  type BehavioralInsight,
  type RelationshipNetwork,
  type StaffMatchResult,
  type ServiceMatchResult,
  type GuestIntelligenceAction
} from '../../services/intelligenceService'
import { RelationshipNetworkGraph } from '../../components/intelligence/RelationshipNetworkGraph'

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  )
}

export default function IntelligenceDashboardPage() {
  const [selectedGuestId, setSelectedGuestId] = useState<number | null>(null)
  const [tabValue, setTabValue] = useState(0)
  const [executingActions, setExecutingActions] = useState<Set<number>>(new Set())
  const [notification, setNotification] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success'
  })

  const handleCloseNotification = () => {
    setNotification(prev => ({ ...prev, open: false }))
  }

  const handleExecuteAction = async (action: AutomaticAction, index: number) => {
    try {
      setExecutingActions(prev => new Set(prev).add(index))
      const success = await intelligenceService.executeAutomaticAction(action)
      if (success) {
        setNotification({
          open: true,
          message: `${action.title} executed successfully.`,
          severity: 'success'
        })
      } else {
        setNotification({
          open: true,
          message: `Failed to execute ${action.title}.`,
          severity: 'error'
        })
      }
    } catch (error) {
      setNotification({
        open: true,
        message: 'An error occurred during execution.',
        severity: 'error'
      })
      console.error('Error executing action:', error)
    } finally {
      setExecutingActions(prev => {
        const next = new Set(prev)
        next.delete(index)
        return next
      })
    }
  }

  // Fetch proactive recommendations (Global or Guest-specific)
  const { data: recommendations = [], isLoading: loadingRecommendations } = useQuery({
    queryKey: ['proactiveRecommendations', selectedGuestId],
    queryFn: (): Promise<ProactiveRecommendation[]> => selectedGuestId
      ? intelligenceService.getProactiveRecommendations(selectedGuestId)
      : Promise.resolve([
        { title: 'Optimize Fleet for Afternoon Peak', description: 'Predicted 20% increase in airport transfers between 14:00-17:00. Recommend shifting 2 drivers to IST sector.', priority: 0.9, recommendationType: 'FLEET_OPTIMIZATION' },
        { title: 'Language Match Opportunity', description: '5 German-speaking guests arriving tonight. Only 1 German-speaking guide available. Recommend booking external guide.', priority: 0.85, recommendationType: 'STAFFING' }
      ]),
  })

  // Fetch problem prevention alerts
  const { data: alerts = [], isLoading: loadingAlerts } = useQuery({
    queryKey: ['problemPreventionAlerts', selectedGuestId],
    queryFn: () => intelligenceService.getProblemPreventionAlerts(selectedGuestId || undefined),
  })

  // Fetch personalization suggestions
  const { data: suggestions = [], isLoading: loadingSuggestions } = useQuery({
    queryKey: ['personalizationSuggestions', selectedGuestId],
    queryFn: (): Promise<PersonalizationSuggestion[]> => selectedGuestId
      ? intelligenceService.getPersonalizationSuggestions(selectedGuestId)
      : Promise.resolve([
        { title: 'VIP Arrival Protocol', description: '3 VIP guests arriving tomorrow. Recommend personalized welcome letter and specialized vehicle setup.', confidence: 0.95, suggestionType: 'SERVICE_QUALITY' },
        { title: 'Repeat Guest Recognition', description: 'High probability of repeat guest booking. Recommend loyalty discount offers.', confidence: 0.82, suggestionType: 'MARKETING' }
      ]),
  })

  // Fetch early warning signals
  const { data: warnings = [], isLoading: loadingWarnings } = useQuery({
    queryKey: ['earlyWarningSignals', selectedGuestId],
    queryFn: () => intelligenceService.getEarlyWarningSignals(selectedGuestId || undefined),
  })

  // Fetch automatic actions
  const { data: actions = [], isLoading: loadingActions } = useQuery({
    queryKey: ['automaticActions', selectedGuestId],
    queryFn: (): Promise<AutomaticAction[]> => selectedGuestId
      ? intelligenceService.getAutomaticActions(selectedGuestId)
      : Promise.resolve([
        { title: 'Auto-Assign Driver for Peak', description: 'Automatically assign available drivers to pending airport transfers based on proximity and rating.', canExecuteAutomatically: true, confidence: 0.98, actionType: 'AUTO_ASSIGN' },
        { title: 'Dynamic Pricing Adjustment', description: 'High demand detected for Yacht Tours. Automatically adjust prices by +5% for next 24 hours.', canExecuteAutomatically: false, confidence: 0.75, actionType: 'PRICING' }
      ]),
  })

  // Fetch pricing intelligence
  const { data: pricingData = [], isLoading: loadingPricing } = useQuery({
    queryKey: ['pricingIntelligence'],
    queryFn: () => {
      const start = new Date().toISOString().split('T')[0]
      const end = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
      return intelligenceService.getPricingIntelligence(1, start, end) // RoomType 1 for demo
    },
  })

  // Fetch action history
  const { data: actionHistory = [], isLoading: loadingHistory } = useQuery({
    queryKey: ['actionHistory', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.getActionHistory(selectedGuestId) : Promise.resolve([]),
    enabled: !!selectedGuestId
  })

  // Fetch staff note insights
  const { data: noteInsights = [], isLoading: loadingNoteInsights } = useQuery({
    queryKey: ['noteInsights', selectedGuestId],
    queryFn: () => selectedGuestId
      ? intelligenceService.getRecentBehavioralInsights(selectedGuestId, 'DailyNote')
      : Promise.resolve([]),
    enabled: !!selectedGuestId
  })

  // Fetch staff matches
  const { data: staffMatches = [], isLoading: loadingStaffMatches } = useQuery<StaffMatchResult[]>({
    queryKey: ['staffMatches', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.findBestStaffMatches(selectedGuestId) : Promise.resolve([]),
    enabled: !!selectedGuestId
  })

  // Fetch service matches
  const { data: serviceMatches = [], isLoading: loadingServiceMatches } = useQuery<ServiceMatchResult[]>({
    queryKey: ['serviceMatches', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.findBestServiceMatches(selectedGuestId) : Promise.resolve([]),
    enabled: !!selectedGuestId
  })

  // Fetch relationship network
  const { data: network, isLoading: loadingNetwork } = useQuery<RelationshipNetwork | null>({
    queryKey: ['relationshipNetwork', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.getGuestRelationshipNetwork(selectedGuestId) : Promise.resolve(null),
    enabled: !!selectedGuestId
  })

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'Critical':
        return 'error'
      case 'High':
        return 'warning'
      case 'Medium':
        return 'info'
      default:
        return 'success'
    }
  }

  const getPriorityColor = (priority: number) => {
    if (priority >= 0.8) return 'error'
    if (priority >= 0.6) return 'warning'
    if (priority >= 0.4) return 'info'
    return 'success'
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
        <Psychology sx={{ fontSize: 32 }} />
        Intelligence Dashboard
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Turizm Operasyon Intelligence Layer - Proaktif zeka ve öneriler
      </Typography>

      {/* Guest Selector */}
      <Box sx={{ mb: 4, p: 2, bgcolor: 'action.hover', borderRadius: 1, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Typography variant="subtitle1">Select Guest ID for Personalized AI Insights:</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {[1, 5, 12, 42].map(id => (
            <Button
              key={id}
              variant={selectedGuestId === id ? "contained" : "outlined"}
              size="small"
              onClick={() => setSelectedGuestId(id)}
            >
              Guest #{id}
            </Button>
          ))}
          <Button
            variant={selectedGuestId === null ? "contained" : "outlined"}
            size="small"
            onClick={() => setSelectedGuestId(null)}
          >
            Global View
          </Button>
        </Box>
      </Box>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Proactive Recommendations
                  </Typography>
                  <Typography variant="h4">
                    {loadingRecommendations ? <CircularProgress size={24} /> : recommendations.length}
                  </Typography>
                </Box>
                <Recommend sx={{ fontSize: 40, color: 'primary.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Problem Alerts
                  </Typography>
                  <Typography variant="h4">
                    {loadingAlerts ? <CircularProgress size={24} /> : alerts.length}
                  </Typography>
                </Box>
                <Warning sx={{ fontSize: 40, color: 'warning.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Early Warnings
                  </Typography>
                  <Typography variant="h4">
                    {loadingWarnings ? <CircularProgress size={24} /> : warnings.length}
                  </Typography>
                </Box>
                <NotificationsActive sx={{ fontSize: 40, color: 'error.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Personalization Suggestions
                  </Typography>
                  <Typography variant="h4">
                    {loadingSuggestions ? <CircularProgress size={24} /> : suggestions.length}
                  </Typography>
                </Box>
                <Lightbulb sx={{ fontSize: 40, color: 'info.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)} variant="scrollable" scrollButtons="auto">
          <Tab label="Recommendations" icon={<Recommend />} iconPosition="start" />
          <Tab label="Problem Prevention" icon={<Warning />} iconPosition="start" />
          <Tab label="Early Warnings" icon={<NotificationsActive />} iconPosition="start" />
          <Tab label="Personalization" icon={<Lightbulb />} iconPosition="start" />
          <Tab label="Auto Actions" icon={<AutoAwesome />} iconPosition="start" />
          <Tab label="Action History" icon={<HistoryIcon />} iconPosition="start" />
          <Tab label="Pricing" icon={<AttachMoney />} iconPosition="start" />
          <Tab label="Note Analytics" icon={<Description />} iconPosition="start" />
          <Tab label="Relationship Network" icon={<Hub />} iconPosition="start" />
        </Tabs>

        {/* Proactive Recommendations Tab */}
        <TabPanel value={tabValue} index={0}>
          {loadingRecommendations ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : recommendations.length === 0 ? (
            <Alert severity="info">No proactive recommendations available. Select a guest to see recommendations.</Alert>
          ) : (
            <List>
              {recommendations.map((rec: ProactiveRecommendation, index: number) => (
                <ListItem key={index} sx={{ mb: 2, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                        <Typography variant="h6">{rec.title}</Typography>
                        <Chip
                          label={`${(rec.priority * 100).toFixed(0)}%`}
                          color={getPriorityColor(rec.priority)}
                          size="small"
                        />
                        <Chip label={rec.recommendationType} size="small" variant="outlined" />
                      </Box>
                    }
                    secondary={
                      <Box>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                          {rec.description}
                        </Typography>
                        {rec.recommendedAction && (
                          <Typography variant="body2" sx={{ fontStyle: 'italic', color: 'primary.main' }}>
                            💡 {rec.recommendedAction}
                          </Typography>
                        )}
                        {rec.recommendedDate && (
                          <Typography variant="caption" color="text.secondary">
                            Recommended Date: {new Date(rec.recommendedDate).toLocaleDateString()}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </TabPanel>

        {/* Problem Prevention Tab */}
        <TabPanel value={tabValue} index={1}>
          {loadingAlerts ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : alerts.length === 0 ? (
            <Alert severity="success">No problem alerts detected. All systems operating normally.</Alert>
          ) : (
            <List>
              {alerts.map((alert: ProblemPreventionAlert, index: number) => (
                <ListItem key={index} sx={{ mb: 2 }}>
                  <Alert
                    severity={getSeverityColor(alert.severity) as any}
                    sx={{ width: '100%' }}
                    action={
                      <Button size="small" onClick={() => alert.recommendedIntervention && console.log(alert.recommendedIntervention)}>
                        View Details
                      </Button>
                    }
                  >
                    <AlertTitle>{alert.title}</AlertTitle>
                    {alert.description}
                    {alert.recommendedIntervention && (
                      <Box sx={{ mt: 1 }}>
                        <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                          Recommended Intervention:
                        </Typography>
                        <Typography variant="body2">{alert.recommendedIntervention}</Typography>
                      </Box>
                    )}
                  </Alert>
                </ListItem>
              ))}
            </List>
          )}
        </TabPanel>

        {/* Early Warnings Tab */}
        <TabPanel value={tabValue} index={2}>
          {loadingWarnings ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : warnings.length === 0 ? (
            <Alert severity="success">No early warning signals detected.</Alert>
          ) : (
            <List>
              {warnings.map((warning: EarlyWarningSignal, index: number) => (
                <ListItem key={index} sx={{ mb: 2, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="h6">{warning.message}</Typography>
                        <Chip
                          label={warning.severity}
                          color={getSeverityColor(warning.severity)}
                          size="small"
                        />
                        <Chip label={warning.signalType} size="small" variant="outlined" />
                      </Box>
                    }
                    secondary={
                      <Typography variant="caption" color="text.secondary">
                        Detected: {new Date(warning.detectedAt).toLocaleString()}
                      </Typography>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </TabPanel>

        {/* Personalization Tab */}
        <TabPanel value={tabValue} index={3}>
          {loadingSuggestions ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : suggestions.length === 0 ? (
            <Alert severity="info">No personalization suggestions available. Select a guest to see suggestions.</Alert>
          ) : (
            <List>
              {suggestions.map((suggestion: PersonalizationSuggestion, index: number) => (
                <ListItem key={index} sx={{ mb: 2, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                        <Typography variant="h6">{suggestion.title}</Typography>
                        <Chip
                          label={`${(suggestion.confidence * 100).toFixed(0)}% confidence`}
                          color="info"
                          size="small"
                        />
                        <Chip label={suggestion.suggestionType} size="small" variant="outlined" />
                      </Box>
                    }
                    secondary={
                      <Box>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                          {suggestion.description}
                        </Typography>
                        {suggestion.suggestedAction && (
                          <Typography variant="body2" sx={{ fontStyle: 'italic', color: 'primary.main' }}>
                            💡 {suggestion.suggestedAction}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </TabPanel>

        {/* Automatic Actions Tab */}
        <TabPanel value={tabValue} index={4}>
          {loadingActions ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : actions.length === 0 ? (
            <Alert severity="info">No automatic actions available. Select a guest to see actions.</Alert>
          ) : (
            <List>
              {actions.map((action: AutomaticAction, index: number) => (
                <ListItem key={index} sx={{ mb: 2, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                        <Typography variant="h6">{action.title}</Typography>
                        <Chip
                          label={action.canExecuteAutomatically ? 'Auto' : 'Manual'}
                          color={action.canExecuteAutomatically ? 'success' : 'warning'}
                          size="small"
                        />
                        <Chip
                          label={`${(action.confidence * 100).toFixed(0)}%`}
                          color="info"
                          size="small"
                        />
                      </Box>
                    }
                    secondary={
                      <Box>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                          {action.description}
                        </Typography>
                        {action.executionDetails && (
                          <Typography variant="body2" sx={{ fontStyle: 'italic' }}>
                            {action.executionDetails}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    {action.canExecuteAutomatically ? (
                      <Button
                        variant="contained"
                        color="primary"
                        size="small"
                        onClick={() => handleExecuteAction(action, index)}
                        disabled={executingActions.has(index)}
                        startIcon={executingActions.has(index) ? <CircularProgress size={16} color="inherit" /> : null}
                      >
                        {executingActions.has(index) ? 'Executing...' : 'Execute'}
                      </Button>
                    ) : (
                      <Button variant="outlined" size="small">
                        Review
                      </Button>
                    )}
                  </ListItemSecondaryAction>
                </ListItem>
              ))}
            </List>
          )}
        </TabPanel>

        {/* Action History Tab */}
        <TabPanel value={tabValue} index={5}>
          {loadingHistory ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : !selectedGuestId ? (
            <Alert severity="info">Please select a guest to view their intervention history.</Alert>
          ) : actionHistory.length === 0 ? (
            <Alert severity="info">No intervention history found for this guest.</Alert>
          ) : (
            <List>
              {actionHistory.map((history: GuestIntelligenceAction) => (
                <ListItem key={history.id} sx={{ mb: 2, border: 1, borderColor: 'divider', borderRadius: 1, borderLeft: 5, borderLeftColor: history.status === 'Success' ? 'success.main' : 'error.main' }}>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                        <Typography variant="h6">{history.title}</Typography>
                        <Chip
                          label={history.status}
                          color={history.status === 'Success' ? 'success' : 'error'}
                          size="small"
                        />
                        <Chip
                          label={history.isAutomatic ? 'Auto' : 'Manual'}
                          variant="outlined"
                          size="small"
                        />
                        <Typography variant="caption" color="text.secondary">
                          {new Date(history.executionDate).toLocaleString()}
                        </Typography>
                      </Box>
                    }
                    secondary={
                      <Box>
                        <Typography variant="body2" sx={{ mb: 1 }}>{history.description}</Typography>
                        <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                          Details: {history.executionDetails}
                        </Typography>
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </TabPanel>

        {/* Pricing Intelligence Tab */}
        <TabPanel value={tabValue} index={6}>
          {loadingPricing ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
                <QueryStats color="primary" />
                <Typography variant="h6">AI Price Forecasting & Occupancy</Typography>
                <Chip label="Beta AI" size="small" color="secondary" sx={{ ml: 1 }} />
              </Box>

              <Grid container spacing={4}>
                <Grid item xs={12} lg={8}>
                  <Paper variant="outlined" sx={{ p: 2, height: 400 }}>
                    <Typography variant="subtitle2" gutterBottom>Occupancy Forecast vs Price Strategy</Typography>
                    <ResponsiveContainer width="100%" height="100%">
                      <AreaChart data={pricingData}>
                        <defs>
                          <linearGradient id="colorOcc" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="#8884d8" stopOpacity={0.1} />
                            <stop offset="95%" stopColor="#8884d8" stopOpacity={0} />
                          </linearGradient>
                        </defs>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="date" tickFormatter={(str) => new Date(str).toLocaleDateString(undefined, { weekday: 'short' })} />
                        <YAxis yAxisId="left" orientation="left" stroke="#8884d8" />
                        <YAxis yAxisId="right" orientation="right" stroke="#82ca9d" />
                        <RechartsTooltip
                          content={({ active, payload }) => {
                            if (active && payload && payload.length) {
                              const data = payload[0].payload as PricingIntelligenceResult;
                              return (
                                <Box sx={{ bgcolor: 'background.paper', p: 1.5, border: '1px solid', borderColor: 'divider', borderRadius: 1, boxShadow: 2 }}>
                                  <Typography variant="subtitle2" sx={{ mb: 1 }}>{new Date(data.date).toLocaleDateString()}</Typography>
                                  <Typography variant="body2" color="primary">Occupancy: {(data.forecastedOccupancy * 100).toFixed(0)}%</Typography>
                                  <Typography variant="body2" color="success.main">Rate: {data.dynamicRate} TRY</Typography>
                                  {data.ruleDetails && data.ruleDetails.length > 0 ? (
                                    <Box sx={{ mt: 1 }}>
                                      <Typography variant="caption" sx={{ fontWeight: 'bold', display: 'block' }}>Applied Rules:</Typography>
                                      {data.ruleDetails.map((r, i) => (
                                        <Typography key={i} variant="caption" sx={{ display: 'block', color: 'text.secondary' }}>
                                          • {r.ruleName}: {r.adjustmentValue > 0 ? '+' : ''}{r.adjustmentValue}{r.adjustmentType === 'Percentage' ? '%' : ' TRY'} → {r.resultingRate} TRY
                                        </Typography>
                                      ))}
                                    </Box>
                                  ) : data.appliedRules.length > 0 && (
                                    <Box sx={{ mt: 1 }}>
                                      <Typography variant="caption" sx={{ fontWeight: 'bold', display: 'block' }}>Applied Rules:</Typography>
                                      {data.appliedRules.map((r, i) => (
                                        <Typography key={i} variant="caption" sx={{ display: 'block', color: 'text.secondary' }}>• {r}</Typography>
                                      ))}
                                    </Box>
                                  )}
                                </Box>
                              );
                            }
                            return null;
                          }}
                        />
                        <Legend />
                        <Area
                          yAxisId="left"
                          type="monotone"
                          dataKey="forecastedOccupancy"
                          name="AI Occupancy Forecast"
                          stroke="#8884d8"
                          fillOpacity={1}
                          fill="url(#colorOcc)"
                        />
                        <Line
                          yAxisId="right"
                          type="stepAfter"
                          dataKey="dynamicRate"
                          name="Dynamic Rate (TRY)"
                          stroke="#82ca9d"
                          strokeWidth={2}
                          dot={{ r: 4 }}
                        />
                      </AreaChart>
                    </ResponsiveContainer>
                  </Paper>
                </Grid>
                <Grid item xs={12} lg={4}>
                  <Typography variant="subtitle1" gutterBottom>Forecast Details</Typography>
                  <List dense>
                    {pricingData.slice(0, 5).map((point: PricingIntelligenceResult, idx: number) => (
                      <ListItem key={idx} sx={{ mb: 1, backgroundColor: 'action.hover', borderRadius: 1 }}>
                        <ListItemText
                          primary={`${new Date(point.date).toLocaleDateString()} - Occupancy: ${(point.forecastedOccupancy * 100).toFixed(0)}%`}
                          secondary={
                            <Box>
                              <Typography variant="caption" display="block">Rate: {point.dynamicRate} TRY</Typography>
                              {point.isStopSell && <Chip label="STOP SELL" color="error" size="small" sx={{ mt: 0.5 }} />}
                              <Box sx={{ mt: 1, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                                {point.appliedRules.map((r, i) => (
                                  <Chip
                                    key={i}
                                    label={r}
                                    size="small"
                                    color="secondary"
                                    variant="filled"
                                    sx={{
                                      fontSize: '0.65rem',
                                      bgcolor: 'secondary.light',
                                      color: 'secondary.dark',
                                      fontWeight: 'bold'
                                    }}
                                  />
                                ))}
                              </Box>
                            </Box>
                          }
                        />
                      </ListItem>
                    ))}
                  </List>
                </Grid>
              </Grid>
            </Box>
          )}
        </TabPanel>

        {/* Note Analytics Tab */}
        <TabPanel value={tabValue} index={7}>
          {!selectedGuestId ? (
            <Alert severity="info">Please select a guest to view AI-extracted insights from staff notes.</Alert>
          ) : loadingNoteInsights ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : noteInsights.length === 0 ? (
            <Alert severity="info" icon={<AutoAwesome />}>AI has not yet processed any specific behavioral patterns from staff notes for this guest.</Alert>
          ) : (
            <Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
                <AutoAwesome color="primary" />
                <Typography variant="h6">Staff Note Behavioral Insights</Typography>
              </Box>
              <Grid container spacing={2}>
                {noteInsights.map((insight: BehavioralInsight, idx: number) => (
                  <Grid item xs={12} md={6} key={idx}>
                    <Card variant="outlined">
                      <CardContent>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                          <Typography variant="subtitle1" fontWeight="bold">{insight.behaviorType}</Typography>
                          <Chip label={insight.category} size="small" color="primary" variant="outlined" />
                        </Box>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                          {insight.behaviorValue}
                        </Typography>
                        <Divider sx={{ my: 1 }} />
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Typography variant="caption" color="text.disabled">
                            Extracted: {new Date(insight.behaviorDate).toLocaleDateString()}
                          </Typography>
                          {insight.sentimentScore !== null && (
                            <Chip
                              label={`Sentiment: ${insight.sentimentScore! > 0 ? 'Positive' : 'Negative'}`}
                              size="small"
                              color={insight.sentimentScore! > 0 ? 'success' : 'error'}
                              variant="outlined"
                            />
                          )}
                        </Box>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          )}
        </TabPanel>

        {/* Relationship Network Tab */}
        <TabPanel value={tabValue} index={8}>
          {!selectedGuestId ? (
            <Alert severity="info">Please select a guest to view their interaction and relationship network.</Alert>
          ) : loadingNetwork ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : !network ? (
            <Alert severity="warning">Could not load relationship network. Ensure Neo4j sync is active.</Alert>
          ) : (
            <Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
                <Hub color="primary" />
                <Typography variant="h6">Guest Relationship Network (Graph Patterns)</Typography>
              </Box>

              <Box sx={{ mb: 4 }}>
                <RelationshipNetworkGraph network={network} />
              </Box>

              <Grid container spacing={3}>
                {/* Guest Context */}
                <Grid item xs={12}>
                  <Card sx={{ bgcolor: 'primary.main', color: 'white' }}>
                    <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <Psychology sx={{ fontSize: 40 }} />
                      <Box>
                        <Typography variant="h5">{network.guestNode.name}</Typography>
                        <Typography variant="body2">Network Anchor - ID: {network.guestNode.id}</Typography>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>

                {/* Staff Relationships */}
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle1" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Group fontSize="small" /> Trusted Staff Interactions
                  </Typography>
                  <List>
                    {network.staffNodes.map(staff => {
                      const edge = network.edges.find(e => e.targetId === staff.id);
                      return (
                        <Paper key={staff.id} variant="outlined" sx={{ mb: 1, p: 2 }}>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Typography variant="subtitle2">{staff.name}</Typography>
                            <Chip label={`Strength: ${(edge?.weight || 0).toFixed(2)}`} size="small" color="success" />
                          </Box>
                          <Typography variant="caption" color="text.secondary">
                            Frequency: {staff.properties?.Frequency || 0} | Sat: {staff.properties?.Satisfaction || 0}/10
                          </Typography>
                        </Paper>
                      );
                    })}
                    {network.staffNodes.length === 0 && <Typography variant="body2" color="text.disabled">No staff interactions mapped yet.</Typography>}
                  </List>
                </Grid>

                {/* Service/Preference Relationships */}
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle1" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Share fontSize="small" /> Service & Preference Ties
                  </Typography>
                  <List>
                    {network.serviceNodes.map(service => {
                      const edge = network.edges.find(e => e.targetId === service.id);
                      return (
                        <Paper key={service.id} variant="outlined" sx={{ mb: 1, p: 2 }}>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Typography variant="subtitle2">{service.name}</Typography>
                              <Chip label={service.type} size="small" variant="outlined" />
                            </Box>
                            <Chip label={edge?.relationshipType} size="small" color="info" />
                          </Box>
                          <Typography variant="caption" color="text.secondary">
                            Score: {service.properties?.Satisfaction || service.properties?.Sentiment || 0}
                          </Typography>
                        </Paper>
                      );
                    })}
                    {network.serviceNodes.length === 0 && <Typography variant="body2" color="text.disabled">No service preferences discovered yet.</Typography>}
                  </List>
                </Grid>
              </Grid>

              <Grid container spacing={3} sx={{ mt: 2 }}>
                {/* Staff Recommendations (From backend logic) */}
                <Grid item xs={12} md={6}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                    <AutoAwesome color="secondary" fontSize="small" />
                    <Typography variant="subtitle1">AI Staff Recommendations</Typography>
                  </Box>
                  <List>
                    {(staffMatches || []).map((match: any, idx: number) => (
                      <Paper key={idx} variant="outlined" sx={{ mb: 1, p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box>
                          <Typography variant="subtitle2">{match.staffName}</Typography>
                          <Typography variant="caption" color="text.secondary">Matches: {match.interactionCount} | Avg Sat: {match.averageSatisfaction}/10</Typography>
                        </Box>
                        <Chip label={`${(match.compatibilityScore * 100).toFixed(0)}% Match`} color="primary" size="small" />
                      </Paper>
                    ))}
                    {loadingStaffMatches && <CircularProgress size={20} />}
                    {(!staffMatches || staffMatches.length === 0) && !loadingStaffMatches && (
                      <Alert severity="info" sx={{ py: 0 }}>No AI staff recommendations available yet.</Alert>
                    )}
                  </List>
                </Grid>

                {/* Service Recommendations (From backend logic) */}
                <Grid item xs={12} md={6}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                    <AutoAwesome color="secondary" fontSize="small" />
                    <Typography variant="subtitle1">Service & Tour Recommendations</Typography>
                  </Box>
                  <List>
                    {(serviceMatches || []).map((match: any, idx: number) => (
                      <Paper key={idx} variant="outlined" sx={{ mb: 1, p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box>
                          <Typography variant="subtitle2">{match.serviceName}</Typography>
                          <Typography variant="caption" color="text.secondary">{match.recommendationReason}</Typography>
                        </Box>
                        <Chip label={`${(match.matchScore * 100).toFixed(0)}% Score`} color="info" size="small" />
                      </Paper>
                    ))}
                    {loadingServiceMatches && <CircularProgress size={20} />}
                    {(!serviceMatches || serviceMatches.length === 0) && !loadingServiceMatches && (
                      <Alert severity="info" sx={{ py: 0 }}>No personalized service recommendations available yet.</Alert>
                    )}
                  </List>
                </Grid>
              </Grid>
            </Box>
          )}
        </TabPanel>
      </Paper>

      {/* Guest Selection Note */}
      {!selectedGuestId && (
        <Alert severity="info" sx={{ mt: 2 }}>
          <AlertTitle>Guest Selection Required</AlertTitle>
          Some features require a guest to be selected. Please select a guest from the Guests page to see personalized recommendations and suggestions.
        </Alert>
      )}

      {/* Execution Feedback Notification */}
      <Snackbar
        open={notification.open}
        autoHideDuration={6000}
        onClose={handleCloseNotification}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={handleCloseNotification} severity={notification.severity} variant="filled" sx={{ width: '100%' }}>
          {notification.message}
        </Alert>
      </Snackbar>
    </Box>
  )
}
