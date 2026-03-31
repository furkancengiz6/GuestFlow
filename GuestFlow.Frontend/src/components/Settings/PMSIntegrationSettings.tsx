import { useState, useEffect } from 'react'
import {
    Box,
    Typography,
    Card,
    CardContent,
    Grid,
    TextField,
    Button,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Switch,
    FormControlLabel,
    Alert,
    Chip,
    Divider,
    CircularProgress,
    IconButton,
    Collapse
} from '@mui/material'
import {
    Save as SaveIcon,
    Sync as SyncIcon,
    CheckCircle as CheckCircleIcon,
    Error as ErrorIcon,
    ExpandMore as ExpandMoreIcon,
    ExpandLess as ExpandLessIcon,
    Add as AddIcon
} from '@mui/icons-material'
import axios from 'axios'

import { env } from '../../config/env'

// Types (Move to src/types/pms.ts later)
interface PMSIntegration {
    id: number
    providerName: string
    providerCode: string
    apiEndpoint: string
    apiKey: string
    apiSecret: string | null
    isActive: boolean
    syncMode: string
    pollingIntervalMinutes: number
    lastSyncDate: string | null
    lastSyncStatus: string | null
    lastConnectionTestDate: string | null
    lastConnectionTestResult: boolean
}

const apiClient = axios.create({
    baseURL: env.apiBaseUrl
})

const PMSIntegrationSettings = () => {
    const [integrations, setIntegrations] = useState<PMSIntegration[]>([])
    const [isLoading, setIsLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const [testingConnection, setTestingConnection] = useState<number | null>(null)
    const [testResult, setTestResult] = useState<{ id: number, success: boolean, message: string } | null>(null)
    const [expandedId, setExpandedId] = useState<number | null>(null)

    useEffect(() => {
        fetchIntegrations()
    }, [])

    const fetchIntegrations = async () => {
        try {
            setIsLoading(true)
            const response = await apiClient.get('/pms/integrations')
            if (response.data && response.data.success) {
                setIntegrations(response.data.data)
            } else {
                setIntegrations([])
            }
            setIsLoading(false)
        } catch (err) {
            console.error(err);
            setError('Entegrasyonlar yüklenirken bir hata oluştu.')
            setIsLoading(false)
        }
    }

    const handleTestConnection = async (id: number) => {
        setTestingConnection(id)
        setTestResult(null)
        try {
            const response = await apiClient.post(`/pms/integrations/${id}/test-connection`)
            setTestResult({
                id,
                success: response.data.success,
                message: response.data.message || (response.data.success ? 'Bağlantı başarılı!' : 'Bağlantı başarısız.')
            })
            fetchIntegrations()
        } catch (err: any) {
            console.error(err);
            setTestResult({
                id,
                success: false,
                message: err.response?.data?.message || err.message || 'Bağlantı testi sırasında hata oluştu.'
            })
        } finally {
            setTestingConnection(null)
        }
    }

    const handleSave = async (integration: PMSIntegration) => {
        try {
            await apiClient.put(`/pms/integrations/${integration.id}`, integration)
            fetchIntegrations()
        } catch (err) {
            console.error(err);
        }
    }

    const toggleExpand = (id: number) => {
        setExpandedId(expandedId === id ? null : id)
    }

    if (isLoading) {
        return <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress /></Box>
    }

    if (error) {
        return <Alert severity="error">{error}</Alert>
    }

    return (
        <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6">PMS Entegrasyonları</Typography>
                <Button startIcon={<AddIcon />} variant="contained" size="small">
                    Yeni Ekle
                </Button>
            </Box>

            {integrations.map((integration) => (
                <Card key={integration.id} sx={{ mb: 2, border: integration.isActive ? '1px solid #4caf50' : '1px solid #e0e0e0' }}>
                    <CardContent>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                <Typography variant="h6">{integration.providerName}</Typography>
                                <Chip
                                    label={integration.isActive ? 'Aktif' : 'Pasif'}
                                    color={integration.isActive ? 'success' : 'default'}
                                    size="small"
                                />
                                {integration.lastConnectionTestResult ? (
                                    <Chip icon={<CheckCircleIcon />} label="Bağlı" color="success" variant="outlined" size="small" />
                                ) : (
                                    <Chip icon={<ErrorIcon />} label="Bağlantı Yok" color="error" variant="outlined" size="small" />
                                )}
                            </Box>
                            <IconButton onClick={() => toggleExpand(integration.id)}>
                                {expandedId === integration.id ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                            </IconButton>
                        </Box>

                        <Collapse in={expandedId === integration.id}>
                            <Divider sx={{ my: 2 }} />
                            <Grid container spacing={3}>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="API Endpoint"
                                        value={integration.apiEndpoint}
                                        size="small"
                                        margin="normal"
                                    />
                                    <TextField
                                        fullWidth
                                        label="API Key / Client ID"
                                        value={integration.apiKey}
                                        type="password"
                                        size="small"
                                        margin="normal"
                                    />
                                    <TextField
                                        fullWidth
                                        label="API Secret"
                                        value={integration.apiSecret || ''}
                                        type="password"
                                        size="small"
                                        margin="normal"
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <FormControl fullWidth size="small" margin="normal">
                                        <InputLabel>Senkronizasyon Modu</InputLabel>
                                        <Select value={integration.syncMode} label="Senkronizasyon Modu">
                                            <MenuItem value="Polling">Polling (Periyodik)</MenuItem>
                                            <MenuItem value="Webhook">Webhook (Anlık)</MenuItem>
                                        </Select>
                                    </FormControl>
                                    <TextField
                                        fullWidth
                                        label="Polling Aralığı (Dakika)"
                                        type="number"
                                        value={integration.pollingIntervalMinutes}
                                        size="small"
                                        margin="normal"
                                    />
                                    <FormControlLabel
                                        control={<Switch checked={integration.isActive} color="primary" />}
                                        label="Entegrasyon Aktif"
                                        sx={{ mt: 2 }}
                                    />
                                </Grid>
                            </Grid>

                            <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                                <Button
                                    variant="outlined"
                                    color={testResult?.success ? 'success' : 'primary'}
                                    startIcon={testingConnection === integration.id ? <CircularProgress size={20} /> : <SyncIcon />}
                                    onClick={() => handleTestConnection(integration.id)}
                                    disabled={testingConnection === integration.id}
                                >
                                    {testingConnection === integration.id ? 'Test Ediliyor...' : 'Bağlantıyı Test Et'}
                                </Button>
                                <Button variant="contained" startIcon={<SaveIcon />} onClick={() => handleSave(integration)}>
                                    Kaydet
                                </Button>
                            </Box>

                            {testResult?.id === integration.id && (
                                <Alert severity={testResult.success ? 'success' : 'error'} sx={{ mt: 2 }}>
                                    {testResult.message}
                                </Alert>
                            )}
                        </Collapse>
                    </CardContent>
                </Card>
            ))}

            {integrations.length === 0 && (
                <Alert severity="info">Henüz eklenmiş bir PMS entegrasyonu bulunmamaktadır.</Alert>
            )}
        </Box>
    )
}

export default PMSIntegrationSettings
