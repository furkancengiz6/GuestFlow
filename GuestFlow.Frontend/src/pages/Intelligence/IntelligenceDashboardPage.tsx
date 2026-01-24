/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import { useState, useEffect } from 'react'
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
  IconButton,
  Tooltip,
  Paper,
  Divider,
} from '@mui/material'
import {
  TrendingUp,
  Warning,
  Lightbulb,
  AutoAwesome,
  Psychology,
  PersonSearch,
  Recommend,
  NotificationsActive,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { intelligenceService, type ProactiveRecommendation, type ProblemPreventionAlert, type PersonalizationSuggestion, type EarlyWarningSignal, type AutomaticAction } from '../../services/intelligenceService'

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

  // Fetch proactive recommendations
  const { data: recommendations = [], isLoading: loadingRecommendations } = useQuery({
    queryKey: ['proactiveRecommendations', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.getProactiveRecommendations(selectedGuestId) : Promise.resolve([]),
    enabled: !!selectedGuestId,
  })

  // Fetch problem prevention alerts
  const { data: alerts = [], isLoading: loadingAlerts } = useQuery({
    queryKey: ['problemPreventionAlerts', selectedGuestId],
    queryFn: () => intelligenceService.getProblemPreventionAlerts(selectedGuestId || undefined),
  })

  // Fetch personalization suggestions
  const { data: suggestions = [], isLoading: loadingSuggestions } = useQuery({
    queryKey: ['personalizationSuggestions', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.getPersonalizationSuggestions(selectedGuestId) : Promise.resolve([]),
    enabled: !!selectedGuestId,
  })

  // Fetch early warning signals
  const { data: warnings = [], isLoading: loadingWarnings } = useQuery({
    queryKey: ['earlyWarningSignals', selectedGuestId],
    queryFn: () => intelligenceService.getEarlyWarningSignals(selectedGuestId || undefined),
  })

  // Fetch automatic actions
  const { data: actions = [], isLoading: loadingActions } = useQuery({
    queryKey: ['automaticActions', selectedGuestId],
    queryFn: () => selectedGuestId ? intelligenceService.getAutomaticActions(selectedGuestId) : Promise.resolve([]),
    enabled: !!selectedGuestId,
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
        <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)}>
          <Tab label="Proactive Recommendations" icon={<Recommend />} iconPosition="start" />
          <Tab label="Problem Prevention" icon={<Warning />} iconPosition="start" />
          <Tab label="Early Warnings" icon={<NotificationsActive />} iconPosition="start" />
          <Tab label="Personalization" icon={<Lightbulb />} iconPosition="start" />
          <Tab label="Automatic Actions" icon={<AutoAwesome />} iconPosition="start" />
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
                      <Button variant="contained" color="primary" size="small">
                        Execute
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
      </Paper>

      {/* Guest Selection Note */}
      {!selectedGuestId && (
        <Alert severity="info" sx={{ mt: 2 }}>
          <AlertTitle>Guest Selection Required</AlertTitle>
          Some features require a guest to be selected. Please select a guest from the Guests page to see personalized recommendations and suggestions.
        </Alert>
      )}
    </Box>
  )
}
