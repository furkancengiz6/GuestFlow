import { useNavigate, useLocation } from 'react-router-dom'
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Box,
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
  { text: 'Personel', icon: <PersonnelIcon />, path: '/personnel', roles: ['Admin'] },
  { text: 'Raporlar', icon: <ReportsIcon />, path: '/reports', roles: ['Admin'] },
  { text: 'Ayarlar', icon: <SettingsIcon />, path: '/settings', roles: ['Admin'] },
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

