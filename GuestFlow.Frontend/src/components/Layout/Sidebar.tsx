import { useState } from 'react'
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
} from '@mui/icons-material'

const drawerWidth = 240

const menuItems = [
  { text: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
  { text: 'Misafirler', icon: <PeopleIcon />, path: '/guests' },
  { text: 'Transferler', icon: <TransferIcon />, path: '/transfers' },
  { text: 'Turlar', icon: <TourIcon />, path: '/tours' },
  { text: 'Faturalar', icon: <InvoiceIcon />, path: '/invoices' },
  { text: 'Raporlar', icon: <ReportsIcon />, path: '/reports' },
  { text: 'Ayarlar', icon: <SettingsIcon />, path: '/settings' },
]

const Sidebar = () => {
  const navigate = useNavigate()
  const location = useLocation()

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
        {menuItems.map((item) => (
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

