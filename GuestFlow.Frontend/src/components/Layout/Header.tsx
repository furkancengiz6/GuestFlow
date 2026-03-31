import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Menu,
  MenuItem,
  Avatar,
  Tooltip,
  Divider,
  ListItemIcon,
  ListItemText,
  alpha,
} from '@mui/material'
import {
  AccountCircle,
  Logout,
  DarkMode,
  LightMode,
  Search,
  Keyboard,
  Add as AddIcon,
  PersonAdd as PersonAddIcon,
  LocalTaxi as TransferIcon,
  EventNote as ReservationIcon,
} from '@mui/icons-material'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../../stores/authStore'
import { authService } from '../../services/authService'
import { useTheme } from '../../theme/useTheme'
import NotificationCenter from '../Notifications/NotificationCenter'
import LanguageSwitcher from '../Language/LanguageSwitcher'
import GlobalSearch from '../Common/GlobalSearch'
import KeyboardShortcutsDialog from '../Common/KeyboardShortcutsDialog'
import { useGlobalSearch } from '../../hooks/useGlobalSearch'
import { useKeyboardShortcuts, commonShortcuts } from '../../hooks/useKeyboardShortcuts'

const Header = () => {
  const navigate = useNavigate()
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [quickActionEl, setQuickActionEl] = useState<null | HTMLElement>(null)
  const [shortcutsOpen, setShortcutsOpen] = useState(false)
  const { user } = useAuthStore()
  const { mode, toggleMode } = useTheme()
  const { open, openSearch, closeSearch } = useGlobalSearch()

  // Keyboard shortcuts
  useKeyboardShortcuts([
    ...commonShortcuts,
    {
      key: 'k',
      ctrl: true,
      action: openSearch,
      description: 'Global search',
    },
    {
      key: '?',
      shift: true,
      action: () => setShortcutsOpen(true),
      description: 'Show keyboard shortcuts',
    },
    {
      key: 'n',
      shift: true,
      action: () => setQuickActionEl(prev => prev ? null : document.getElementById('quick-action-btn')),
      description: 'Quick action menu',
    }
  ])

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleQuickActionOpen = (event: React.MouseEvent<HTMLElement>) => {
    setQuickActionEl(event.currentTarget)
  }

  const handleQuickActionClose = () => {
    setQuickActionEl(null)
  }

  const handleLogout = async () => {
    await authService.logout()
    handleClose()
  }

  const handleQuickNav = (path: string) => {
    navigate(path)
    handleQuickActionClose()
  }

  return (
    <AppBar
      position="sticky"
      elevation={0}
      className="glass-panel"
      sx={{
        borderBottom: '1px solid',
        borderColor: 'divider',
        borderTop: '3px solid',
        borderTopColor: 'primary.main',
        top: 0
      }}
    >
      <Toolbar>
        <Box sx={{ flexGrow: 1, display: 'flex', justifyContent: 'center', ml: { xs: 0, lg: -10 } }}>
          <Box
            onClick={openSearch}
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1.5,
              bgcolor: mode === 'dark' ? alpha('#ffffff', 0.05) : alpha('#0F172A', 0.03),
              px: 3,
              py: 1,
              borderRadius: '24px',
              width: { xs: '100%', sm: 400, md: 500 },
              cursor: 'pointer',
              border: '1px solid',
              borderColor: 'divider',
              transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
              '&:hover': {
                bgcolor: mode === 'dark' ? alpha('#ffffff', 0.08) : alpha('#0F172A', 0.05),
                borderColor: 'primary.main',
                boxShadow: '0 4px 12px rgba(0,0,0,0.05)',
                '& .search-icon': { color: 'primary.main', transform: 'scale(1.1)' },
                '& .search-text': { color: 'text.primary' }
              }
            }}
          >
            <Search className="search-icon" sx={{ fontSize: 20, color: 'text.secondary', transition: 'all 0.2s' }} />
            <Typography className="search-text" variant="body2" sx={{ color: 'text.secondary', fontWeight: 500, flexGrow: 1, transition: 'all 0.2s' }}>
              Misafir, transfer veya rezervasyon ara...
            </Typography>
            <Box
              sx={{
                display: { xs: 'none', md: 'flex' },
                alignItems: 'center',
                gap: 0.5,
                bgcolor: alpha('#64748B', 0.1),
                px: 1,
                py: 0.3,
                borderRadius: '6px',
                border: '1px solid',
                borderColor: 'divider'
              }}
            >
              <Typography variant="caption" sx={{ fontWeight: 700, fontSize: '0.65rem', color: 'text.secondary' }}>⌘ K</Typography>
            </Box>
          </Box>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {/* Quick Actions */}
          <Tooltip title="Hızlı İşlem (Shift+N)">
            <IconButton
              id="quick-action-btn"
              onClick={handleQuickActionOpen}
              sx={{
                background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
                color: 'white',
                '&:hover': {
                  background: 'linear-gradient(135deg, #4f46e5 0%, #4338ca 100%)',
                  transform: 'scale(1.05)',
                },
                transition: 'all 0.2s',
                mr: 1
              }}
              size="small"
            >
              <AddIcon />
            </IconButton>
          </Tooltip>

          <Box sx={{ display: 'flex', alignItems: 'center', bgcolor: alpha('#64748B', 0.05), borderRadius: '12px', p: 0.5, gap: 0.5 }}>
            <Tooltip title="Klavye Kısayolları (Shift+?)">
              <IconButton
                sx={{
                  color: 'text.secondary',
                  display: { xs: 'none', md: 'flex' },
                  transition: 'all 0.2s',
                  '&:hover': { color: 'primary.main', bgcolor: alpha('#5754E8', 0.08) }
                }}
                onClick={() => setShortcutsOpen(true)}
                size="small"
              >
                <Keyboard sx={{ fontSize: 20 }} />
              </IconButton>
            </Tooltip>
            <LanguageSwitcher />
            <NotificationCenter />
          </Box>

          <Divider orientation="vertical" flexItem sx={{ mx: 1, height: 24, alignSelf: 'center' }} />

          <Typography variant="body2" sx={{ fontWeight: 700, color: 'text.primary', display: { xs: 'none', sm: 'block' } }}>
            {user?.fullName}
          </Typography>

          <IconButton
            size="large"
            edge="end"
            aria-label="account of current user"
            aria-controls="menu-appbar"
            aria-haspopup="true"
            onClick={handleMenu}
            color="inherit"
          >
            <Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main', fontWeight: 600, fontSize: '0.9rem' }}>
              {user?.fullName?.charAt(0).toUpperCase()}
            </Avatar>
          </IconButton>

          <Menu
            id="quick-action-menu"
            anchorEl={quickActionEl}
            open={Boolean(quickActionEl)}
            onClose={handleQuickActionClose}
            transformOrigin={{ horizontal: 'right', vertical: 'top' }}
            anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
            PaperProps={{
              sx: {
                width: 220,
                mt: 1.5,
                borderRadius: 2,
                boxShadow: '0 10px 40px rgba(0,0,0,0.1)',
                '& .MuiMenuItem-root': {
                  px: 2,
                  py: 1,
                  borderRadius: 1,
                  mx: 0.5,
                  mb: 0.5,
                }
              }
            }}
          >
            <MenuItem onClick={() => handleQuickNav('/guests?add=true')}>
              <ListItemIcon><PersonAddIcon fontSize="small" color="primary" /></ListItemIcon>
              <ListItemText primary="Yeni Misafir" secondary="Hızlı kayıt oluştur" />
            </MenuItem>
            <MenuItem onClick={() => handleQuickNav('/transfers?add=true')}>
              <ListItemIcon><TransferIcon fontSize="small" color="secondary" /></ListItemIcon>
              <ListItemText primary="Yeni Transfer" secondary="Havalimanı / Otel" />
            </MenuItem>
            <MenuItem onClick={() => handleQuickNav('/reservations?add=true')}>
              <ListItemIcon><ReservationIcon fontSize="small" color="success" /></ListItemIcon>
              <ListItemText primary="Yeni Rezervasyon" secondary="Genel hizmet talebi" />
            </MenuItem>
          </Menu>

          <Menu
            id="menu-appbar"
            anchorEl={anchorEl}
            anchorOrigin={{
              vertical: 'bottom',
              horizontal: 'right',
            }}
            keepMounted
            transformOrigin={{
              vertical: 'top',
              horizontal: 'right',
            }}
            open={Boolean(anchorEl)}
            onClose={handleClose}
            PaperProps={{
              sx: {
                width: 180,
                mt: 1.5,
                borderRadius: 2,
                boxShadow: '0 10px 40px rgba(0,0,0,0.1)',
              }
            }}
          >
            <MenuItem onClick={handleClose}>
              <AccountCircle sx={{ mr: 1, color: 'primary.main' }} />
              Profil
            </MenuItem>
            <Divider />
            <MenuItem onClick={toggleMode}>
              {mode === 'dark' ? <LightMode sx={{ mr: 1, color: 'warning.main' }} /> : <DarkMode sx={{ mr: 1 }} />}
              {mode === 'dark' ? 'Aydınlık Mod' : 'Karanlık Mod'}
            </MenuItem>
            <MenuItem onClick={handleLogout}>
              <Logout sx={{ mr: 1, color: 'error.main' }} />
              Çıkış Yap
            </MenuItem>
          </Menu>
        </Box>
      </Toolbar>
      <GlobalSearch open={open} onClose={closeSearch} />
      <KeyboardShortcutsDialog open={shortcutsOpen} onClose={() => setShortcutsOpen(false)} />
    </AppBar>

  )
}

export default Header

