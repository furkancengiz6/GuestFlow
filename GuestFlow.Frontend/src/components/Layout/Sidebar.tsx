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
  Note as NoteIcon,
  AttachMoney as RevenueIcon,
  Payment as PaymentIcon,
  BookOnline as ReservationIcon,
  Sms as SmsIcon,
  WhatsApp as WhatsAppIcon,
  Email as EmailIcon,
  Notifications as NotificationIcon,
  Folder as FolderIcon,
  CalendarToday as CalendarIcon,
  Hotel as HotelIcon,
  Restaurant as RestaurantIcon,
  Timeline as TimelineIcon,
  CurrencyExchange as CurrencyIcon,
  Inventory as PackageIcon,
  MeetingRoom as RoomIcon,
  SettingsApplications as RuleSettingsIcon,
  Map as MapIcon,
  Today as TodayIcon,
  Security as SecurityIcon,
  Psychology as IntelligenceIcon,
} from '@mui/icons-material'
import { useAuthStore } from '../../stores/authStore'

const drawerWidth = 240

type MenuItem = {
  text: string
  icon: React.ReactNode
  path: string
  roles?: string[] // izin verilen roller
}

const menuItems: MenuItem[] = [
  { text: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
  { text: 'Misafirler', icon: <PeopleIcon />, path: '/guests' },
  { text: 'Transferler', icon: <TransferIcon />, path: '/transfers' },
  { text: 'Turlar', icon: <TourIcon />, path: '/tours' },
  { text: 'Faturalar', icon: <InvoiceIcon />, path: '/invoices' },
  { text: 'Ödemeler', icon: <PaymentIcon />, path: '/payments', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Oda Yönetimi', icon: <RoomIcon />, path: '/room-assignments', roles: ['Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Rezervasyonlar', icon: <ReservationIcon />, path: '/reservations', roles: ['Reception', 'Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'İtineraryler', icon: <TimelineIcon />, path: '/itineraries', roles: ['Reception', 'Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Oteller', icon: <HotelIcon />, path: '/hotels', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Restoranlar', icon: <RestaurantIcon />, path: '/restaurants', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Personel', icon: <PersonnelIcon />, path: '/personnel', roles: ['Manager', 'Admin', 'Owner'] },
    { text: 'Raporlar', icon: <ReportsIcon />, path: '/reports', roles: ['Manager', 'Admin', 'Owner'] },
    { text: 'Intelligence', icon: <IntelligenceIcon />, path: '/intelligence', roles: ['Admin', 'Staff'] },
  { text: 'Havalimanları', icon: <AirportIcon />, path: '/airports', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Şehirler', icon: <CityIcon />, path: '/cities', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Araçlar', icon: <VehicleIcon />, path: '/vehicles', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Günlük Notlar', icon: <NoteIcon />, path: '/daily-notes', roles: ['Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Günlük Gelirler', icon: <RevenueIcon />, path: '/daily-revenues', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'SMS', icon: <SmsIcon />, path: '/sms', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'WhatsApp', icon: <WhatsAppIcon />, path: '/whatsapp', roles: ['Manager', 'Admin', 'Owner', 'Staff'] },
  { text: 'E-postalar', icon: <EmailIcon />, path: '/emails', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Bildirimler', icon: <NotificationIcon />, path: '/notifications', roles: ['Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Bildirim Kuralları', icon: <RuleSettingsIcon />, path: '/notification-rules', roles: ['Admin', 'Staff'] },
  { text: 'Günlük Operasyonlar', icon: <TodayIcon />, path: '/daily-operations', roles: ['Admin', 'Staff'] },
  { text: 'Operasyonel Harita', icon: <MapIcon />, path: '/operational-map', roles: ['Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Dosyalar', icon: <FolderIcon />, path: '/files', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Takvim', icon: <CalendarIcon />, path: '/calendar', roles: ['Reception', 'Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Para Birimi', icon: <CurrencyIcon />, path: '/currency', roles: ['Manager', 'Admin', 'Owner'] },
  { text: 'Servis Paketleri', icon: <PackageIcon />, path: '/service-packages', roles: ['Concierge', 'Manager', 'Admin', 'Owner'] },
  { text: 'Ayarlar', icon: <SettingsIcon />, path: '/settings', roles: ['Admin', 'Owner'] },
  { text: 'Login Audit', icon: <SecurityIcon />, path: '/security/login-audit', roles: ['Admin', 'Owner'] },
  { text: 'PII Yönetimi', icon: <SecurityIcon />, path: '/privacy', roles: ['Admin', 'Owner'] },
]

const Sidebar = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { user } = useAuthStore()

  const userRole = user?.role || user?.userType
  const visibleMenus = menuItems.filter((item) => {
    if (!item.roles || item.roles.length === 0) return true
    return !!userRole && item.roles.includes(userRole)
  })

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
        },
      }}
    >
      <Toolbar>
        <Typography variant="h6" noWrap component="div" sx={{ fontWeight: 600 }}>
          GuestFlow
        </Typography>
      </Toolbar>
      <List>
        {visibleMenus.map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton
              selected={location.pathname === item.path}
              onClick={() => navigate(item.path)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Drawer>
  )
}

export default Sidebar

