import {
    Box,
    Typography,
    Grid,
    Card,
    CardContent,
    Chip,
    LinearProgress,
    List,
    ListItem,
    ListItemText,
    Divider,
    Alert,
    AlertTitle,
    CircularProgress,
} from '@mui/material'
import {
    Warning as WarningIcon,
    EmojiObjects as TipsIcon,
    Person as PersonIcon,
    Handshake as MatchIcon,
    TrendingUp as TrendIcon,
    Star as PriorityIcon,
} from '@mui/icons-material'
import {
    useGuestIntelligenceRisks,
    useGuestStaffMatches,
    useGuestProactiveRecommendations,
    useGuestServiceRecommendations,
} from '../../hooks/useIntelligence'

interface GuestIntelligenceTabProps {
    guestId: number
}

const GuestIntelligenceTab = ({ guestId }: GuestIntelligenceTabProps) => {
    const { data: riskData, isLoading: loadingRisks } = useGuestIntelligenceRisks(guestId)
    const { data: staffMatches, isLoading: loadingStaff } = useGuestStaffMatches(guestId)
    const { data: proactiveRecs, isLoading: loadingRecs } = useGuestProactiveRecommendations(guestId)
    const { data: serviceRecs, isLoading: loadingServices } = useGuestServiceRecommendations(guestId)

    if (loadingRisks || loadingStaff || loadingRecs || loadingServices) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}>
                <CircularProgress />
            </Box>
        )
    }

    const getSeverityColor = (severity: string) => {
        switch (severity?.toLowerCase()) {
            case 'critical':
            case 'high':
                return 'error'
            case 'medium':
                return 'warning'
            default:
                return 'info'
        }
    }

    return (
        <Box>
            <Grid container spacing={3}>
                {/* Risk Prediction Section */}
                <Grid item xs={12} md={6}>
                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <WarningIcon color="error" />
                        Risk Tahminleri
                    </Typography>
                    <Card variant="outlined">
                        <CardContent>
                            {riskData?.risks && riskData.risks.length > 0 ? (
                                <List dense>
                                    {riskData.risks.map((risk: any, index: number) => (
                                        <Box key={index} sx={{ mb: 2 }}>
                                            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                                                <Typography variant="body2" fontWeight="medium">
                                                    {risk.riskType}
                                                </Typography>
                                                <Chip
                                                    label={risk.severity}
                                                    size="small"
                                                    color={getSeverityColor(risk.severity) as any}
                                                    variant="filled"
                                                />
                                            </Box>
                                            <LinearProgress
                                                variant="determinate"
                                                value={risk.riskScore * 100}
                                                color={getSeverityColor(risk.severity) as any}
                                                sx={{ height: 8, borderRadius: 4, mb: 0.5 }}
                                            />
                                            <Typography variant="caption" color="text.secondary">
                                                {risk.description}
                                            </Typography>
                                            {index < riskData.risks.length - 1 && <Divider sx={{ mt: 1.5 }} />}
                                        </Box>
                                    ))}
                                </List>
                            ) : (
                                <Alert severity="success">
                                    Aktif bir risk sinyali tespit edilmedi.
                                </Alert>
                            )}
                        </CardContent>
                    </Card>
                </Grid>

                {/* Proactive Recommendations */}
                <Grid item xs={12} md={6}>
                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <TipsIcon color="primary" />
                        AI Önerileri & Aksiyonlar
                    </Typography>
                    {proactiveRecs && proactiveRecs.length > 0 ? (
                        <Box>
                            {proactiveRecs.map((rec: any, index: number) => (
                                <Alert
                                    key={index}
                                    severity="info"
                                    icon={<TrendIcon />}
                                    sx={{ mb: 2, '& .MuiAlert-message': { width: '100%' } }}
                                >
                                    <AlertTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        {rec.title}
                                        <Chip
                                            label={`Öncelik: %${(rec.priority * 100).toFixed(0)}`}
                                            size="small"
                                            color="primary"
                                        />
                                    </AlertTitle>
                                    <Typography variant="body2">{rec.description}</Typography>
                                    {rec.recommendedAction && (
                                        <Box sx={{ mt: 1, p: 1, bgcolor: 'rgba(25, 118, 210, 0.08)', borderRadius: 1 }}>
                                            <Typography variant="caption" fontWeight="bold">
                                                Önerilen Aksiyon:
                                            </Typography>
                                            <Typography variant="body2">{rec.recommendedAction}</Typography>
                                        </Box>
                                    )}
                                </Alert>
                            ))}
                        </Box>
                    ) : (
                        <Card variant="outlined">
                            <CardContent>
                                <Typography variant="body2" color="text.secondary">
                                    Şu an için yeni bir proaktif öneri bulunmuyor.
                                </Typography>
                            </CardContent>
                        </Card>
                    )}
                </Grid>

                {/* Staff & Service Matching */}
                <Grid item xs={12} md={6}>
                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <PersonIcon color="action" />
                        En İyi Personel Eşleşmeleri
                    </Typography>
                    <Card variant="outlined">
                        <CardContent>
                            {staffMatches && staffMatches.length > 0 ? (
                                <List sx={{ pt: 0 }}>
                                    {staffMatches.map((match: any, index: number) => (
                                        <Box key={index}>
                                            <ListItem sx={{ px: 0 }}>
                                                <ListItemText
                                                    primary={match.staffName}
                                                    secondary={`Uyumluluk: %${(match.compatibilityScore * 100).toFixed(0)} | ${match.interactionCount} Etkileşim`}
                                                />
                                                <Chip
                                                    icon={<PriorityIcon sx={{ fontSize: '1rem !important' }} />}
                                                    label={`${match.averageSatisfaction.toFixed(1)}/10`}
                                                    color="success"
                                                    variant="outlined"
                                                    size="small"
                                                />
                                            </ListItem>
                                            {index < staffMatches.length - 1 && <Divider />}
                                        </Box>
                                    ))}
                                </List>
                            ) : (
                                <Typography variant="body2" color="text.secondary">
                                    Yeterli veri bulunmadığı için personel eşleşmesi yapılamadı.
                                </Typography>
                            )}
                        </CardContent>
                    </Card>
                </Grid>

                <Grid item xs={12} md={6}>
                    <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <MatchIcon color="action" />
                        Servis Önerileri
                    </Typography>
                    <Card variant="outlined">
                        <CardContent>
                            {serviceRecs && serviceRecs.length > 0 ? (
                                <List sx={{ pt: 0 }}>
                                    {serviceRecs.map((service: any, index: number) => (
                                        <Box key={index}>
                                            <ListItem sx={{ px: 0 }}>
                                                <ListItemText
                                                    primary={service.serviceName}
                                                    secondary={service.recommendationReason}
                                                />
                                                <Chip
                                                    label={`Skor: %${(service.matchScore * 100).toFixed(0)}`}
                                                    size="small"
                                                    color="secondary"
                                                    variant="outlined"
                                                />
                                            </ListItem>
                                            {index < serviceRecs.length - 1 && <Divider />}
                                        </Box>
                                    ))}
                                </List>
                            ) : (
                                <Typography variant="body2" color="text.secondary">
                                    Kişiselleştirilmiş servis önerisi bulunmuyor.
                                </Typography>
                            )}
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>
        </Box>
    )
}

export default GuestIntelligenceTab
