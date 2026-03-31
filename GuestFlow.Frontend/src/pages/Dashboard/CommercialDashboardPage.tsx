/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import { useState } from 'react'
import {
    Box,
    Typography,
    Grid,
    Card,
    CardContent,
    Tab,
    Tabs,
    CircularProgress,
    List,
    ListItem,
    ListItemText,
    Chip,
    LinearProgress,
    TableContainer,
    Paper,
    Divider,
} from '@mui/material'
import {
    TrendingUp,
    TrendingDown,
    Psychology,
    AutoGraph,
    Groups,
    Nature,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { commercialDashboardService } from '../../services/commercialDashboardService'
import { formatCurrency } from '../../utils/formatters'
import {
    ResponsiveContainer,
    Tooltip,
    Legend,
    PieChart,
    Pie,
    Cell,
} from 'recharts'

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

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8']

const CommercialDashboardPage = () => {
    const [tabValue, setTabValue] = useState(0)

    const { data: summary, isLoading: isLoadingSummary } = useQuery({
        queryKey: ['commercialSummary'],
        queryFn: commercialDashboardService.getExecutiveSummary,
    })

    const { data: upsells } = useQuery({
        queryKey: ['upsellOpportunities'],
        queryFn: commercialDashboardService.getUpsellOpportunities,
    })

    const { data: friction } = useQuery({
        queryKey: ['frictionReport'],
        queryFn: commercialDashboardService.getFrictionReport,
    })

    const { data: loyalty } = useQuery({
        queryKey: ['loyaltyInsights'],
        queryFn: commercialDashboardService.getLoyaltyInsights,
    })

    const { data: bundles } = useQuery({
        queryKey: ['bundledOpportunities'],
        queryFn: commercialDashboardService.getBundledOpportunities,
    })

    const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
        setTabValue(newValue)
    }

    if (isLoadingSummary) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <CircularProgress />
            </Box>
        )
    }

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
                <AutoGraph fontSize="large" color="primary" />
                Commercial Strategy & BI
            </Typography>

            {/* KPI Section */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid item xs={12} sm={6} md={3}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>Total Revenue</Typography>
                            <Typography variant="h4">{formatCurrency(summary?.totalRevenue || 0)}</Typography>
                            <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                                <TrendingUp color="success" sx={{ mr: 0.5 }} />
                                <Typography variant="body2" color="success.main">+{summary?.revenueGrowth}%</Typography>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>Occupancy Rate</Typography>
                            <Typography variant="h4">{summary?.occupancyRate}%</Typography>
                            <LinearProgress variant="determinate" value={summary?.occupancyRate || 0} sx={{ mt: 2, height: 8, borderRadius: 5 }} />
                        </CardContent>
                    </Card>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>Average Booking Value</Typography>
                            <Typography variant="h4">{formatCurrency(summary?.averageBookingValue || 0)}</Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>Based on {summary?.totalBookings} bookings</Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                    <Card>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>Conversion Rate</Typography>
                            <Typography variant="h4">{summary?.conversionRate}%</Typography>
                            <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                                <TrendingDown color="error" sx={{ mr: 0.5 }} />
                                <Typography variant="body2" color="error.main">-1.2% from last week</Typography>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {/* Tabs Section */}
            <Paper sx={{ mb: 4 }}>
                <Tabs value={tabValue} onChange={handleTabChange} variant="scrollable" scrollButtons="auto">
                    <Tab label="Upsell Opportunities" icon={<Psychology sx={{ mr: 1 }} />} iconPosition="start" />
                    <Tab label="Service Friction" icon={<TrendingDown sx={{ mr: 1 }} />} iconPosition="start" />
                    <Tab label="Loyalty Intelligence" icon={<Groups sx={{ mr: 1 }} />} iconPosition="start" />
                    <Tab label="AI Bundles" icon={<AutoGraph sx={{ mr: 1 }} />} iconPosition="start" />
                </Tabs>

                {/* Upsell Tab */}
                <TabPanel value={tabValue} index={0}>
                    <Grid container spacing={3}>
                        <Grid item xs={12} lg={8}>
                            <Typography variant="h6" gutterBottom>Top Revenue Opportunities</Typography>
                            <List>
                                {upsells?.map((item) => (
                                    <ListItem key={item.id} divider sx={{ py: 2 }}>
                                        <ListItemText
                                            primary={<Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                {item.guestName}
                                                <Chip label={item.serviceType} size="small" variant="outlined" />
                                                <Chip label={`${(item.confidenceScore * 100).toFixed(0)}% Confidence`} size="small" color="primary" />
                                            </Box>}
                                            secondary={item.reason}
                                        />
                                        <Box sx={{ textAlign: 'right' }}>
                                            <Typography variant="h6" color="success.main">+{formatCurrency(item.estimatedRevenue)}</Typography>
                                            <Typography variant="caption" color="text.secondary">Est. Value</Typography>
                                        </Box>
                                    </ListItem>
                                ))}
                            </List>
                        </Grid>
                        <Grid item xs={12} lg={4}>
                            <Typography variant="h6" gutterBottom>Opportunity Mix</Typography>
                            <ResponsiveContainer width="100%" height={300}>
                                <PieChart>
                                    <Pie
                                        data={upsells ? Array.from(new Set(upsells.map(u => u.serviceType))).map(type => ({
                                            name: type,
                                            value: upsells.filter(u => u.serviceType === type).length
                                        })) : []}
                                        cx="50%"
                                        cy="50%"
                                        outerRadius={80}
                                        fill="#8884d8"
                                        dataKey="value"
                                        label
                                    >
                                        {upsells?.map((_, index) => <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />)}
                                    </Pie>
                                    <Tooltip />
                                    <Legend />
                                </PieChart>
                            </ResponsiveContainer>
                        </Grid>
                    </Grid>
                </TabPanel>

                {/* Friction Tab */}
                <TabPanel value={tabValue} index={1}>
                    <Typography variant="h6" gutterBottom>Departmental Performance & Friction</Typography>
                    <Grid container spacing={3}>
                        {friction?.map((item, idx) => (
                            <Grid item xs={12} md={6} lg={4} key={idx}>
                                <Card variant="outlined">
                                    <CardContent>
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                                            <Typography variant="subtitle1" fontWeight="bold">{item.department}</Typography>
                                            <Chip
                                                label={`Friction: ${item.frictionScore}/10`}
                                                color={item.frictionScore > 7 ? 'error' : item.frictionScore > 4 ? 'warning' : 'success'}
                                                size="small"
                                            />
                                        </Box>
                                        <Typography variant="body2" color="text.secondary">Avg. Response: {item.averageResponseTime}m</Typography>
                                        <Typography variant="body2" color="text.secondary">Pending: {item.pendingRequests}</Typography>
                                        <Divider sx={{ my: 1 }} />
                                        <Typography variant="caption" fontWeight="bold">Identified Issues:</Typography>
                                        <List dense>
                                            {item.issuesIdentified.map((issue, i) => (
                                                <ListItem key={i} disablePadding>
                                                    <ListItemText primary={`• ${issue}`} primaryTypographyProps={{ variant: 'caption' }} />
                                                </ListItem>
                                            ))}
                                        </List>
                                    </CardContent>
                                </Card>
                            </Grid>
                        ))}
                    </Grid>
                </TabPanel>

                {/* Loyalty Tab */}
                <TabPanel value={tabValue} index={2}>
                    <Typography variant="h6" gutterBottom>Guest Lifetime Value & Retention</Typography>
                    <TableContainer component={Paper} variant="outlined">
                        <List>
                            {loyalty?.map((item, idx) => (
                                <ListItem key={idx} divider>
                                    <ListItemText
                                        primary={item.guestName}
                                        secondary={`Tier: ${item.loyaltyTier} | LTV: ${formatCurrency(item.lifetimeValue)}`}
                                    />
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 3 }}>
                                        <Box sx={{ width: 100 }}>
                                            <Typography variant="caption">Churn Risk</Typography>
                                            <LinearProgress
                                                variant="determinate"
                                                value={item.churnRisk * 100}
                                                color={item.churnRisk > 0.6 ? 'error' : 'success'}
                                            />
                                        </Box>
                                        <Chip label={item.nextBestAction} color="info" variant="outlined" />
                                    </Box>
                                </ListItem>
                            ))}
                        </List>
                    </TableContainer>
                </TabPanel>

                {/* AI Bundles Tab */}
                <TabPanel value={tabValue} index={3}>
                    <Grid container spacing={3}>
                        <Grid item xs={12} md={6}>
                            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Psychology color="primary" /> Smart-Bundled Offers
                            </Typography>
                            <List>
                                {bundles?.map((item) => (
                                    <Card sx={{ mb: 2 }} key={item.id}>
                                        <CardContent>
                                            <Typography variant="subtitle1" fontWeight="bold">{item.recommendedService}</Typography>
                                            <Typography variant="body2" color="text.secondary" gutterBottom>{item.reason}</Typography>
                                            <Box sx={{ mt: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                <Chip label={`Value: ${formatCurrency(item.estimatedRevenue)}`} color="success" size="small" />
                                                <Typography variant="caption">Confidence: {(item.confidenceScore * 100).toFixed(0)}%</Typography>
                                            </Box>
                                        </CardContent>
                                    </Card>
                                ))}
                            </List>
                        </Grid>
                        <Grid item xs={12} md={6}>
                            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Nature color="success" /> Sustainable Recommendations
                            </Typography>
                            <List>
                                {/* Sustainable Bundles */}
                                {/* We'll fetch these as well */}
                            </List>
                        </Grid>
                    </Grid>
                </TabPanel>
            </Paper>
        </Box>
    )
}

export default CommercialDashboardPage
