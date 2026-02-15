import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
    Box,
    Grid,
    Paper,
    Typography,
    Avatar,
    Chip,
    Button,
    List,
    ListItem,
    ListItemText,
    CircularProgress,
    Card,
    CardContent,
    Tab,
    Tabs,
    LinearProgress,
    Alert,
} from '@mui/material'
import {
    ArrowBack as ArrowBackIcon,
    Email as EmailIcon,
    Work as WorkIcon,
    Star as StarIcon,
    Timeline as TimelineIcon,
    Person as PersonIcon,
    TrendingUp as TrendingUpIcon,
    Psychology as PsychologyIcon,
} from '@mui/icons-material'
import { personnelService } from '../../services/personnelService'
import { intelligenceService } from '../../services/intelligenceService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

interface TabPanelProps {
    children?: React.ReactNode
    index: number
    value: number
}

function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props

    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`simple-tabpanel-${index}`}
            aria-labelledby={`simple-tab-${index}`}
            {...other}
        >
            {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
        </div>
    )
}

const PersonnelDetailPage = () => {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const [tabValue, setTabValue] = useState(0)
    const staffId = parseInt(id || '0')

    const { data: personnel, isLoading: isLoadingPersonnel } = useQuery({
        queryKey: ['personnel', staffId],
        queryFn: () => personnelService.getPersonnelDetail(staffId),
        enabled: !!staffId,
    })

    // Intelligence Queries
    const { data: guestMatches, isLoading: isLoadingMatches } = useQuery({
        queryKey: ['staff-guest-matches', staffId],
        queryFn: () => intelligenceService.findBestGuestMatches(staffId),
        enabled: !!staffId,
    })

    const { data: behaviorPatterns, isLoading: isLoadingPatterns } = useQuery({
        queryKey: ['staff-behavior-patterns', staffId],
        queryFn: () => intelligenceService.getStaffBehaviorPatterns(staffId),
        enabled: !!staffId,
    })

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setTabValue(newValue)
    }

    if (isLoadingPersonnel) {
        return <ContentState state="loading" />
    }

    if (!personnel) {
        return <ContentState state="error" title="Personel bulunamadı" />
    }

    // Mock Performance Data (until backend sends real data)
    const performanceScore = 85
    const responseTime = '4.2 dk'
    const taskCompletionRate = '92%'

    return (
        <Box p={3}>
            <Button
                startIcon={<ArrowBackIcon />}
                onClick={() => navigate('/personnel')}
                sx={{ mb: 3 }}
            >
                Listeye Dön
            </Button>

            <Grid container spacing={3}>
                {/* Profile Header */}
                <Grid item xs={12}>
                    <Paper sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 3 }}>
                        <Avatar
                            sx={{ width: 100, height: 100, bgcolor: 'primary.main', fontSize: '3rem' }}
                        >
                            {personnel.fullName.charAt(0)}
                        </Avatar>
                        <Box flex={1}>
                            <Box display="flex" alignItems="center" gap={2} mb={1}>
                                <Typography variant="h4">{personnel.fullName}</Typography>
                                <Chip
                                    label={personnel.userType}
                                    color={personnel.userType === 'Admin' ? 'error' : 'primary'}
                                />
                            </Box>
                            <Box display="flex" gap={3} color="text.secondary">
                                <Box display="flex" alignItems="center" gap={1}>
                                    <EmailIcon fontSize="small" />
                                    <Typography>{personnel.email}</Typography>
                                </Box>
                                <Box display="flex" alignItems="center" gap={1}>
                                    <WorkIcon fontSize="small" />
                                    <Typography>{personnel.createdDate ? `Katılma: ${formatDate(personnel.createdDate)}` : ''}</Typography>
                                </Box>
                            </Box>
                        </Box>
                    </Paper>
                </Grid>

                {/* Main Content */}
                <Grid item xs={12}>
                    <Paper sx={{ width: '100%' }}>
                        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                            <Tabs value={tabValue} onChange={handleTabChange}>
                                <Tab label="Genel Bakış" icon={<PersonIcon />} iconPosition="start" />
                                <Tab label="Performans Analizi" icon={<TrendingUpIcon />} iconPosition="start" />
                                <Tab label="İlişki Zekası" icon={<PsychologyIcon />} iconPosition="start" />
                                <Tab label="Aktivite Geçmişi" icon={<TimelineIcon />} iconPosition="start" />
                            </Tabs>
                        </Box>

                        {/* Overview Tab */}
                        <TabPanel value={tabValue} index={0}>
                            <Grid container spacing={3}>
                                <Grid item xs={12} md={4}>
                                    <Card sx={{ height: '100%' }}>
                                        <CardContent>
                                            <Typography variant="h6" gutterBottom>Hızlı İstatistikler</Typography>
                                            <List>
                                                <ListItem>
                                                    <ListItemText primary="Toplam Görev" secondary="142" />
                                                </ListItem>
                                                <ListItem>
                                                    <ListItemText primary="Tamamlanan" secondary="138" />
                                                </ListItem>
                                                <ListItem>
                                                    <ListItemText primary="Bekleyen" secondary="4" />
                                                </ListItem>
                                            </List>
                                        </CardContent>
                                    </Card>
                                </Grid>
                                <Grid item xs={12} md={8}>
                                    <Card sx={{ height: '100%' }}>
                                        <CardContent>
                                            <Typography variant="h6" gutterBottom>Son Hareketler</Typography>
                                            {/* Placeholder for recent activity */}
                                            <Alert severity="info">Henüz aktivite kaydı bulunmuyor.</Alert>
                                        </CardContent>
                                    </Card>
                                </Grid>
                            </Grid>
                        </TabPanel>

                        {/* Performance Tab */}
                        <TabPanel value={tabValue} index={1}>
                            <Grid container spacing={3}>
                                <Grid item xs={12} md={4}>
                                    <Card>
                                        <CardContent sx={{ textAlign: 'center' }}>
                                            <Typography color="text.secondary" gutterBottom>
                                                Genel Verimlilik Skoru
                                            </Typography>
                                            <Box position="relative" display="inline-flex">
                                                <CircularProgress variant="determinate" value={performanceScore} size={120} thickness={4} />
                                                <Box
                                                    top={0}
                                                    left={0}
                                                    bottom={0}
                                                    right={0}
                                                    position="absolute"
                                                    display="flex"
                                                    alignItems="center"
                                                    justifyContent="center"
                                                >
                                                    <Typography variant="h4" component="div" color="text.secondary">
                                                        {performanceScore}
                                                    </Typography>
                                                </Box>
                                            </Box>
                                            <Typography variant="body2" sx={{ mt: 2 }}>
                                                Ortalama üstü performans
                                            </Typography>
                                        </CardContent>
                                    </Card>
                                </Grid>
                                <Grid item xs={12} md={8}>
                                    <Typography variant="h6" gutterBottom>Metrikler</Typography>
                                    <Box sx={{ mb: 2 }}>
                                        <Box display="flex" justifyContent="space-between" mb={1}>
                                            <Typography>Görev Tamamlama Hızı ({responseTime})</Typography>
                                            <Typography>{taskCompletionRate}</Typography>
                                        </Box>
                                        <LinearProgress variant="determinate" value={92} color="success" sx={{ height: 10, borderRadius: 5 }} />
                                    </Box>
                                    <Box sx={{ mb: 2 }}>
                                        <Box display="flex" justifyContent="space-between" mb={1}>
                                            <Typography>Misafir Memnuniyeti (8.9/10)</Typography>
                                            <Typography>89%</Typography>
                                        </Box>
                                        <LinearProgress variant="determinate" value={89} color="primary" sx={{ height: 10, borderRadius: 5 }} />
                                    </Box>
                                </Grid>
                            </Grid>
                        </TabPanel>

                        {/* Relationship Intelligence Tab */}
                        <TabPanel value={tabValue} index={2}>
                            <Grid container spacing={3}>
                                <Grid item xs={12} md={6}>
                                    <Typography variant="h6" gutterBottom>En İyi Misafir Eşleşmeleri</Typography>
                                    <Typography variant="body2" color="text.secondary" paragraph>
                                        Personelin en verimli olduğu ve en iyi iletişim kurduğu misafir profilleri.
                                    </Typography>

                                    {isLoadingMatches ? (
                                        <CircularProgress />
                                    ) : (
                                        <List>
                                            {guestMatches?.map((match) => (
                                                <Paper key={match.guestId} sx={{ mb: 2, p: 2 }} variant="outlined">
                                                    <Box display="flex" justifyContent="space-between" alignItems="center">
                                                        <Box display="flex" alignItems="center" gap={2}>
                                                            <Avatar>{match.guestName.charAt(0)}</Avatar>
                                                            <Box>
                                                                <Typography variant="subtitle1">{match.guestName}</Typography>
                                                                <Typography variant="caption" display="block">
                                                                    {match.interactionCount} Etkileşim
                                                                </Typography>
                                                            </Box>
                                                        </Box>
                                                        <Box textAlign="right">
                                                            <Box display="flex" alignItems="center" gap={0.5} justifyContent="flex-end">
                                                                <StarIcon color="warning" fontSize="small" />
                                                                <Typography fontWeight="bold">{match.averageSatisfaction.toFixed(1)}</Typography>
                                                            </Box>
                                                            <Typography variant="caption" color="text.secondary">
                                                                Uyum: %{(match.compatibilityScore * 100).toFixed(0)}
                                                            </Typography>
                                                        </Box>
                                                    </Box>
                                                    {match.matchReason && (
                                                        <Alert severity="success" sx={{ mt: 1, py: 0 }} icon={false}>
                                                            <Typography variant="caption">{match.matchReason}</Typography>
                                                        </Alert>
                                                    )}
                                                </Paper>
                                            ))}
                                            {(!guestMatches || guestMatches.length === 0) && (
                                                <Alert severity="info">Yeterli veri bulunamadı.</Alert>
                                            )}
                                        </List>
                                    )}
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <Typography variant="h6" gutterBottom>Davranış Analizi</Typography>
                                    {isLoadingPatterns ? (
                                        <CircularProgress />
                                    ) : (
                                        <Box>
                                            {behaviorPatterns ? (
                                                <Paper sx={{ p: 2, bgcolor: 'background.default' }} variant="outlined">
                                                    <Typography variant="subtitle2" gutterBottom>Tespit Edilen Kalıplar</Typography>
                                                    <Box display="flex" gap={1} flexWrap="wrap">
                                                        {/* Visualize patterns here if available, otherwise show chips */}
                                                        <Chip label="Veri Analizi Bekleniyor" size="small" />
                                                    </Box>
                                                </Paper>
                                            ) : (
                                                <Alert severity="info" sx={{ mb: 2 }}>
                                                    AI tarafından henüz yeterli davranış verisi toplanmadı.
                                                </Alert>
                                            )}
                                            <Paper sx={{ p: 2, bgcolor: 'background.default', mt: 2 }} variant="outlined">
                                                <Typography variant="subtitle2" gutterBottom>Otomatik Tespitler</Typography>
                                                <Box display="flex" gap={1} flexWrap="wrap">
                                                    <Chip label="Hızlı Çözüm Üretme" color="success" size="small" />
                                                    <Chip label="VIP Misafir İletişimi" color="success" size="small" />
                                                </Box>
                                            </Paper>
                                        </Box>
                                    )}
                                </Grid>
                            </Grid>
                        </TabPanel>

                        <TabPanel value={tabValue} index={3}>
                            <Typography variant="h6">Aktivite Geçmişi</Typography>
                            <Typography color="text.secondary">Personelin sistem üzerindeki tüm işlem geçmişi.</Typography>
                            {/* Activity Timeline would go here */}
                        </TabPanel>
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    )
}

export default PersonnelDetailPage
