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
  AutoGraph as CommercialIcon,
  Nature as SustainabilityIcon,
  Rule as PricingIcon,
  Flag as FeatureFlagIcon,
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
      { text: 'Commercial Dashboard', icon: <CommercialIcon color="secondary" />, path: '/commercial-dashboard', roles: ['Manager', 'Admin', 'Owner'] },
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
      { text: 'Sürdürülebilirlik', icon: <SustainabilityIcon color="success" />, path: '/sustainability', roles: ['Manager', 'Admin', 'Staff'] },
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
      { text: 'Fiyatlandırma Kuralları', icon: <PricingIcon />, path: '/admin/pricing-rules', roles: ['Admin', 'Owner'] },
      { text: 'Feature Flagler', icon: <FeatureFlagIcon />, path: '/admin/feature-flags', roles: ['Admin'] },
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
                  fontSize: '0.75rem',
                  fontWeight: 800,
                  textTransform: 'uppercase',
                  color: 'primary.main',
                  letterSpacing: '0.1em',
                  bgcolor: 'transparent',
                  mt: 3,
                  mb: 1,
                  px: 2,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1,
                  '&::after': {
                    content: '""',
                    flex: 1,
                    height: '1px',
                    background: alpha('#6366f1', 0.1),
                    ml: 1
                  }
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
                          borderRadius: '12px',
                          py: 1.2,
                          px: 2,
                          mx: 1,
                          transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
                          '&.Mui-selected': {
                            backgroundColor: alpha('#6366f1', 0.1),
                            color: 'primary.main',
                            '& .active-indicator': {
                              opacity: 1,
                              transform: 'scaleY(1)'
                            },
                            '& .MuiListItemIcon-root': {
                              color: 'primary.main',
                              background: alpha('#6366f1', 0.15),
                            },
                            '&:hover': {
                              backgroundColor: alpha('#6366f1', 0.15),
                            },
                          },
                          '&:hover': {
                            backgroundColor: alpha('#6366f1', 0.05),
                            '& .MuiListItemIcon-root': {
                              transform: 'translateX(2px)',
                              color: 'primary.main'
                            }
                          },
                        }}
                      >
                        <ListItemIcon
                          sx={{
                            minWidth: 42,
                            height: 32,
                            width: 32,
                            borderRadius: '8px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            mr: 1.5,
                            transition: 'all 0.2s',
                            color: active ? 'primary.main' : 'text.secondary',
                            background: active ? alpha('#6366f1', 0.1) : 'transparent',
                          }}
                        >
                          {item.icon}
                        </ListItemIcon>
                        <ListItemText
                          primary={item.text}
                          primaryTypographyProps={{
                            variant: 'body2',
                            fontWeight: active ? 700 : 500,
                            fontSize: '0.875rem'
                          }}
                        />
                        <Box
                          className="active-indicator"
                          sx={{
                            position: 'absolute',
                            right: 0,
                            width: 3,
                            height: '60%',
                            bgcolor: 'primary.main',
                            borderRadius: '4px 0 0 4px',
                            opacity: 0,
                            transform: 'scaleY(0.5)',
                            transition: 'all 0.3s'
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

