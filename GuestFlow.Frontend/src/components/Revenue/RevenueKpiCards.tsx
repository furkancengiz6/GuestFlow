import { Box, Card, CardContent, Typography, Skeleton, alpha, useTheme } from '@mui/material'
import { TrendingUp, Hotel, Percent } from '@mui/icons-material'
import { useRevenueDashboard } from '../../hooks/useDashboard'
import { formatCurrency } from '../../utils/formatters'

/**
 * Revenue KPI kartları - ADR, RevPAR ve Doluluk Oranı göstergelerini içerir
 */
const RevenueKpiCards = () => {
    const theme = useTheme()
    const { data: revenueDashboard, isLoading, error } = useRevenueDashboard()

    const kpiCards = [
        {
            title: 'ADR',
            subtitle: 'Ortalama Günlük Ücret',
            value: revenueDashboard?.adr ?? 0,
            format: 'currency',
            icon: <TrendingUp sx={{ fontSize: 32 }} />,
            gradient: `linear-gradient(135deg, ${theme.palette.primary.main} 0%, ${theme.palette.primary.dark} 100%)`,
        },
        {
            title: 'RevPAR',
            subtitle: 'Oda Başına Gelir',
            value: revenueDashboard?.revPar ?? 0,
            format: 'currency',
            icon: <Hotel sx={{ fontSize: 32 }} />,
            gradient: `linear-gradient(135deg, ${theme.palette.secondary.main} 0%, ${theme.palette.secondary.dark} 100%)`,
        },
        {
            title: 'Doluluk',
            subtitle: 'Doluluk Oranı',
            value: revenueDashboard?.occupancyRate ?? 0,
            format: 'percent',
            icon: <Percent sx={{ fontSize: 32 }} />,
            gradient: `linear-gradient(135deg, ${theme.palette.success.main} 0%, ${theme.palette.success.dark} 100%)`,
        },
    ]

    if (error) {
        return null // Sessizce fail et, ana dashboard görünmeye devam etsin
    }

    return (
        <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
            {kpiCards.map((card) => (
                <Card
                    key={card.title}
                    sx={{
                        flex: '1 1 200px',
                        minWidth: 200,
                        background: card.gradient,
                        color: 'white',
                        borderRadius: 3,
                        boxShadow: `0 4px 20px ${alpha(theme.palette.common.black, 0.15)}`,
                        transition: 'transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out',
                        '&:hover': {
                            transform: 'translateY(-4px)',
                            boxShadow: `0 8px 30px ${alpha(theme.palette.common.black, 0.2)}`,
                        },
                    }}
                >
                    <CardContent sx={{ p: 2.5 }}>
                        {isLoading ? (
                            <>
                                <Skeleton variant="text" width="60%" sx={{ bgcolor: 'rgba(255,255,255,0.3)' }} />
                                <Skeleton variant="text" width="80%" height={40} sx={{ bgcolor: 'rgba(255,255,255,0.3)' }} />
                            </>
                        ) : (
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                <Box>
                                    <Typography variant="overline" sx={{ opacity: 0.9, fontWeight: 600 }}>
                                        {card.title}
                                    </Typography>
                                    <Typography variant="h4" sx={{ fontWeight: 700, my: 0.5 }}>
                                        {card.format === 'currency'
                                            ? formatCurrency(card.value)
                                            : `%${(card.value * 100).toFixed(1)}`}
                                    </Typography>
                                    <Typography variant="caption" sx={{ opacity: 0.8 }}>
                                        {card.subtitle}
                                    </Typography>
                                </Box>
                                <Box
                                    sx={{
                                        p: 1,
                                        borderRadius: 2,
                                        bgcolor: 'rgba(255,255,255,0.2)',
                                        display: 'flex',
                                        alignItems: 'center',
                                        justifyContent: 'center',
                                    }}
                                >
                                    {card.icon}
                                </Box>
                            </Box>
                        )}
                    </CardContent>
                </Card>
            ))}
        </Box>
    )
}

export default RevenueKpiCards
