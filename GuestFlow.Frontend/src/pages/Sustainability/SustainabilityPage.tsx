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
    CircularProgress,
    List,
    ListItem,
    ListItemText,
    Chip,
    Button,
    Paper,
    Alert,
} from '@mui/material'
import {
    Nature,
    EmojiEvents,
    AutoAwesome,
    History,
    Person,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { sustainabilityService } from '../../services/sustainabilityService'

const SustainabilityPage = () => {
    const [selectedGuestId, setSelectedGuestId] = useState<number>(1)

    const { data: score, isLoading: isLoadingScore } = useQuery({
        queryKey: ['guestSustainabilityScore', selectedGuestId],
        queryFn: () => sustainabilityService.getGuestScore(selectedGuestId),
    })

    const { data: reward, isLoading: isLoadingReward } = useQuery({
        queryKey: ['sustainabilityReward', selectedGuestId],
        queryFn: () => sustainabilityService.getRewardRecommendation(selectedGuestId),
    })

    if (isLoadingScore) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <CircularProgress />
            </Box>
        )
    }

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
                <Nature fontSize="large" color="success" />
                Sustainability Management
            </Typography>

            {/* Guest Selector (Demo) */}
            <Box sx={{ mb: 4, p: 2, bgcolor: 'action.hover', borderRadius: 1 }}>
                <Typography variant="subtitle2" gutterBottom>Select Guest for Analysis:</Typography>
                <Box sx={{ display: 'flex', gap: 1 }}>
                    {[1, 5, 12, 42].map(id => (
                        <Button
                            key={id}
                            variant={selectedGuestId === id ? "contained" : "outlined"}
                            onClick={() => setSelectedGuestId(id)}
                            size="small"
                        >
                            Guest #{id}
                        </Button>
                    ))}
                </Box>
            </Box>

            <Grid container spacing={3}>
                {/* Score & Profile Card */}
                <Grid item xs={12} md={4}>
                    <Card>
                        <CardContent sx={{ textAlign: 'center', py: 4 }}>
                            <Person sx={{ fontSize: 60, color: 'primary.main', mb: 2 }} />
                            <Typography variant="h6">Guest #{selectedGuestId}</Typography>
                            <Chip label={score?.level || 'Bronze Member'} color="secondary" sx={{ mt: 1 }} />

                            <Box sx={{ mt: 4, mb: 2 }}>
                                <Typography variant="h2" fontWeight="bold" color="success.main">{score?.score || 0}</Typography>
                                <Typography variant="subtitle1" color="text.secondary">Total Sustainability Points</Typography>
                            </Box>

                            <Button fullWidth variant="contained" color="success" startIcon={<EmojiEvents />}>
                                Redeem Reward
                            </Button>
                        </CardContent>
                    </Card>
                </Grid>

                {/* AI Recommendations */}
                <Grid item xs={12} md={8}>
                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <AutoAwesome color="primary" /> AI Reward Recommendations
                    </Typography>
                    <Card sx={{ mb: 4, borderLeft: 5, borderLeftColor: 'primary.main' }}>
                        <CardContent>
                            {isLoadingReward ? <CircularProgress size={24} /> : reward ? (
                                <Box>
                                    <Typography variant="h6" color="primary">{reward.rewardType}</Typography>
                                    <Typography variant="body1" sx={{ my: 1 }}>{reward.description}</Typography>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 2 }}>
                                        <Chip label={`Confidence: ${(reward.confidence * 100).toFixed(0)}%`} variant="outlined" />
                                        <Typography variant="caption">Required Score: {reward.scoreRequired}</Typography>
                                    </Box>
                                </Box>
                            ) : <Typography color="text.secondary">No recommendations available.</Typography>}
                        </CardContent>
                    </Card>

                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <History /> Recent Sustainable Actions
                    </Typography>
                    <Paper variant="outlined">
                        <List>
                            {score?.recentActions && score.recentActions.length > 0 ? score.recentActions.map((action, idx) => (
                                <ListItem key={idx} divider={idx !== score.recentActions.length - 1}>
                                    <ListItemText primary={action} />
                                    <Chip label="+10 pts" color="success" size="small" variant="outlined" />
                                </ListItem>
                            )) : (
                                <ListItem>
                                    <ListItemText secondary="No actions recorded yet." />
                                </ListItem>
                            )}
                        </List>
                    </Paper>

                    <Box sx={{ mt: 4 }}>
                        <Alert severity="info">
                            Sustainability points are earned by guests who choose eco-friendly transport, reuse towels, or opt-out of daily housekeeping.
                        </Alert>
                    </Box>
                </Grid>
            </Grid>
        </Box>
    )
}

export default SustainabilityPage
