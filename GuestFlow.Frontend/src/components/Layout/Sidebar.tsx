import { useNavigate, useLocation } from 'react-router-dom'
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  Box,
  Divider,
  ListSubheader,
  alpha,
} from '@mui/material'
import {
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  DirectionsCar as TransferIcon,
  Receipt as InvoiceIcon,
  Tour as TourIcon,
  Settings as SettingsIcon,
  Assessment as ReportsIcon,
  Badge as PersonnelIcon,
  Flight as AirportIcon,
  LocationCity as CityIcon,
  DirectionsCarFilled as VehicleIcon,
  AttachMoney as RevenueIcon,
  Payment as PaymentIcon,
  BookOnline as ReservationIcon,
  WhatsApp as WhatsAppIcon,
  Email as EmailIcon,
  Notifications as NotificationIcon,
  CalendarToday as CalendarIcon,
  Hotel as HotelIcon,
  Restaurant as RestaurantIcon,
  Timeline as TimelineIcon,
  CurrencyExchange as CurrencyIcon,
  Inventory as PackageIcon,
  SettingsApplications as RuleSettingsIcon,
  Map as MapIcon,
  Today as TodayIcon,
  Security as SecurityIcon,
  Psychology as IntelligenceIcon,
  HealthAndSafety as HealthAndSafetyIcon,
} from '@mui/icons-material'
import { useAuthStore } from '../../stores/authStore'

const drawerWidth = 260

type MenuItem = {
  text: string
  icon: React.ReactNode
  path: string
  roles?: string[]
}

type MenuCategory = {
  category: string
  items: MenuItem[]
}

const menuCategories: MenuCategory[] = [
  {
    category: 'Overview',
    items: [
      { text: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
      { text: 'Intelligence', icon: <IntelligenceIcon color="primary" />, path: '/intelligence', roles: ['Admin', 'Staff'] },
      { text: 'Operasyonel Harita', icon: <MapIcon />, path: '/operational-map' },
      { text: 'Takvim', icon: <CalendarIcon />, path: '/calendar' },
    ]
  },
  {
    category: 'Operations',
    items: [
      { text: 'Misafirler', icon: <PeopleIcon />, path: '/guests' },
      { text: 'Transferler', icon: <TransferIcon />, path: '/transfers' },
      { text: 'Turlar', icon: <TourIcon />, path: '/tours' },
      { text: 'Rezervasyonlar', icon: <ReservationIcon />, path: '/reservations' },
      { text: 'İtineraryler', icon: <TimelineIcon />, path: '/itineraries' },
      { text: 'Günlük Operasyonlar', icon: <TodayIcon />, path: '/daily-operations', roles: ['Admin', 'Staff'] },
    ]
  },
  {
    category: 'Finance',
    items: [
      { text: 'Faturalar', icon: <InvoiceIcon />, path: '/invoices' },
      { text: 'Ödemeler', icon: <PaymentIcon />, path: '/payments', roles: ['Manager', 'Admin', 'Owner', 'Staff'] },
      { text: 'Günlük Gelirler', icon: <RevenueIcon />, path: '/daily-revenues', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Para Birimi', icon: <CurrencyIcon />, path: '/currency', roles: ['Manager', 'Admin', 'Owner'] },
    ]
  },
  {
    category: 'Communication',
    items: [
      { text: 'WhatsApp', icon: <WhatsAppIcon color="success" />, path: '/whatsapp' },
      { text: 'E-postalar', icon: <EmailIcon />, path: '/emails', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Bildirimler', icon: <NotificationIcon />, path: '/notifications' },
    ]
  },
  {
    category: 'Master Data',
    items: [
      { text: 'Oteller', icon: <HotelIcon />, path: '/hotels', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Restoranlar', icon: <RestaurantIcon />, path: '/restaurants', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Havalimanları', icon: <AirportIcon />, path: '/airports', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Şehirler', icon: <CityIcon />, path: '/cities', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Araçlar', icon: <VehicleIcon />, path: '/vehicles', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Servis Paketleri', icon: <PackageIcon />, path: '/service-packages', roles: ['Manager', 'Admin', 'Owner'] },
    ]
  },
  {
    category: 'Administration',
    items: [
      { text: 'Personel', icon: <PersonnelIcon />, path: '/personnel', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Raporlar', icon: <ReportsIcon />, path: '/reports', roles: ['Manager', 'Admin', 'Owner'] },
      { text: 'Bildirim Kuralları', icon: <RuleSettingsIcon />, path: '/notification-rules', roles: ['Admin', 'Staff'] },
      { text: 'Login Audit', icon: <SecurityIcon />, path: '/security/login-audit', roles: ['Admin', 'Owner'] },
      { text: 'PII Yönetimi', icon: <SecurityIcon />, path: '/privacy', roles: ['Admin', 'Owner'] },
      { text: 'Sistem Sağlığı', icon: <HealthAndSafetyIcon />, path: '/admin/system-health', roles: ['Admin'] },
      { text: 'Ayarlar', icon: <SettingsIcon />, path: '/settings', roles: ['Admin', 'Owner'] },
    ]
  }
]

const Sidebar = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { user } = useAuthStore()

  const userRole = user?.role || user?.userType

  const filterItems = (items: MenuItem[]) =>
    items.filter(item => !item.roles || (userRole && item.roles.includes(userRole)))

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
          backgroundColor: '#ffffff',
          boxShadow: '4px 0 10px rgba(0,0,0,0.02)',
        },
      }}
    >
      <Toolbar sx={{ px: [2, 3], display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Box
          sx={{
            width: 32,
            height: 32,
            borderRadius: 1,
            background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'white',
            fontWeight: 'bold',
            fontSize: 18
          }}
        >
          G
        </Box>
        <Typography variant="h6" noWrap sx={{ fontWeight: 800, color: 'primary.main', letterSpacing: -0.5 }}>
          GuestFlow
        </Typography>
      </Toolbar>

      <Box sx={{ overflowX: 'hidden', overflowY: 'auto', px: 1.5, pb: 4 }}>
        {menuCategories.map((cat) => {
          const visibleItems = filterItems(cat.items)
          if (visibleItems.length === 0) return null

          return (
            <Box key={cat.category} sx={{ mt: 2 }}>
              <ListSubheader
                sx={{
                  lineHeight: '24px',
                  fontSize: '0.7rem',
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  color: 'text.secondary',
                  bgcolor: 'transparent',
                  mb: 0.5,
                  px: 2
                }}
              >
                {cat.category}
              </ListSubheader>
              <List disablePadding>
                {visibleItems.map((item) => {
                  const active = location.pathname === item.path
                  return (
                    <ListItem key={item.text} disablePadding sx={{ mb: 0.5 }}>
                      <ListItemButton
                        selected={active}
                        onClick={() => navigate(item.path)}
                        sx={{
                          borderRadius: '10px',
                          py: 1,
                          px: 2,
                          '&.Mui-selected': {
                            backgroundColor: alpha('#6366f1', 0.08),
                            color: 'primary.main',
                            '& .MuiListItemIcon-root': {
                              color: 'primary.main',
                            },
                            '&:hover': {
                              backgroundColor: alpha('#6366f1', 0.12),
                            },
                          },
                          '&:hover': {
                            backgroundColor: alpha('#6366f1', 0.04),
                          },
                        }}
                      >
                        <ListItemIcon sx={{ minWidth: 38, color: active ? 'primary.main' : 'text.secondary' }}>
                          {item.icon}
                        </ListItemIcon>
                        <ListItemText
                          primary={item.text}
                          primaryTypographyProps={{
                            variant: 'body2',
                            fontWeight: active ? 600 : 500,
                            fontSize: '0.85rem'
                          }}
                        />
                      </ListItemButton>
                    </ListItem>
                  )
                })}
              </List>
              <Divider sx={{ my: 1, mx: 2, opacity: 0.5 }} />
            </Box>
          )
        })}
      </Box>
    </Drawer>
  )
}

export default Sidebar

