import { useState, useEffect } from 'react'
import {
    Box,
    Typography,
    Card,
    CardContent,
    List,
    ListItem,
    ListItemText,
    ListItemIcon,
    Avatar,
    Fade,
    Chip,
} from '@mui/material'
import {
    DirectionsCar as TransferIcon,
    Login as CheckInIcon,
    Logout as CheckOutIcon,
    Tour as TourIcon,
    Payment as PaymentIcon,
    Circle as DotIcon,
} from '@mui/icons-material'
import { formatTime } from '../../utils/formatters'

interface OpsEvent {
    id: number
    type: 'TRANSFER' | 'CHECKIN' | 'CHECKOUT' | 'TOUR' | 'PAYMENT'
    message: string
    time: Date
    severity: 'info' | 'success' | 'warning'
}

const LiveOpsFeed = () => {
    const [events, setEvents] = useState<OpsEvent[]>([
        { id: 1, type: 'TRANSFER', message: 'Transfer started: IST-Term1 to Ritz-Carlton', time: new Date(Date.now() - 1000 * 60 * 5), severity: 'info' },
        { id: 2, type: 'CHECKIN', message: 'Guest Emma Johnson arrived at Marriott', time: new Date(Date.now() - 1000 * 60 * 12), severity: 'success' },
        { id: 3, type: 'PAYMENT', message: 'Payment received: $450 for Yacht Tour #442', time: new Date(Date.now() - 1000 * 60 * 25), severity: 'success' },
        { id: 4, type: 'TOUR', message: 'Guide assigned to Istanbul City Tour (14:00)', time: new Date(Date.now() - 1000 * 60 * 45), severity: 'info' },
    ])

    useEffect(() => {
        const interval = setInterval(() => {
            const types: OpsEvent['type'][] = ['TRANSFER', 'CHECKIN', 'CHECKOUT', 'TOUR', 'PAYMENT']
            const messages: Record<string, string[]> = {
                TRANSFER: ['Vehicle reached pickup point', 'Transfer completed successfully', 'Driver delayed in traffic', 'New transfer request received'],
                CHECKIN: ['Guest checking in at Hilton', 'Room assigned to VIP guest', 'Checked in via PMS sync'],
                CHECKOUT: ['Guest checking out from Kempinski', 'Final invoice generated', 'Late checkout requested'],
                TOUR: ['City Tour #88 started', 'Yacht Tour boarding completed', 'Guide confirmed itinerary'],
                PAYMENT: ['Payment approved: Credit Card', 'Pending bank transfer confirmed', 'Refund processed for cancelled tour'],
            }

            const selectedType = types[Math.floor(Math.random() * types.length)]
            const selectedMessage = messages[selectedType][Math.floor(Math.random() * messages[selectedType].length)]

            const newEvent: OpsEvent = {
                id: Date.now(),
                type: selectedType,
                message: selectedMessage,
                time: new Date(),
                severity: Math.random() > 0.8 ? 'warning' : 'info',
            }

            setEvents(prev => [newEvent, ...prev.slice(0, 9)])
        }, 15000)

        return () => clearInterval(interval)
    }, [])

    const getIcon = (type: OpsEvent['type'], severity: string) => {
        const iconProps = { fontSize: 'small' as const, sx: { color: severity === 'warning' ? 'warning.main' : 'primary.main' } }
        switch (type) {
            case 'TRANSFER': return <TransferIcon {...iconProps} />
            case 'CHECKIN': return <CheckInIcon {...iconProps} />
            case 'CHECKOUT': return <CheckOutIcon {...iconProps} />
            case 'TOUR': return <TourIcon {...iconProps} />
            case 'PAYMENT': return <PaymentIcon {...iconProps} />
            default: return <DotIcon {...iconProps} />
        }
    }

    return (
        <Card sx={{ height: '100%', minHeight: 400 }}>
            <CardContent sx={{ p: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="h6" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <DotIcon color="error" sx={{ fontSize: 12, animation: 'pulse 1.5s infinite' }} />
                        Canlı Operasyon Akışı
                    </Typography>
                    <Chip label="LIVE" color="error" size="small" variant="outlined" sx={{ fontWeight: 'bold' }} />
                </Box>

                <List sx={{ pt: 0 }}>
                    {events.map((event) => (
                        <Fade key={event.id} in={true} timeout={500}>
                            <ListItem
                                alignItems="flex-start"
                                sx={{
                                    px: 0,
                                    py: 1,
                                    borderBottom: '1px solid',
                                    borderColor: 'divider',
                                    '&:last-child': { borderBottom: 0 }
                                }}
                            >
                                <ListItemIcon sx={{ minWidth: 40, mt: 0.5 }}>
                                    <Avatar
                                        sx={{
                                            width: 28,
                                            height: 28,
                                            bgcolor: event.severity === 'warning' ? 'warning.light' : 'primary.light',
                                            opacity: 0.8
                                        }}
                                    >
                                        {getIcon(event.type, event.severity)}
                                    </Avatar>
                                </ListItemIcon>
                                <ListItemText
                                    primary={
                                        <Typography variant="body2" sx={{ fontWeight: 500, fontSize: '0.875rem' }}>
                                            {event.message}
                                        </Typography>
                                    }
                                    secondary={
                                        <Typography variant="caption" color="text.secondary">
                                            {formatTime(event.time)} • {event.type}
                                        </Typography>
                                    }
                                />
                            </ListItem>
                        </Fade>
                    ))}
                </List>
            </CardContent>

            <style dangerouslySetInnerHTML={{
                __html: `
        @keyframes pulse {
          0% { opacity: 1; transform: scale(1); }
          50% { opacity: 0.5; transform: scale(1.2); }
          100% { opacity: 1; transform: scale(1); }
        }
      `}} />
        </Card>
    )
}

export default LiveOpsFeed
