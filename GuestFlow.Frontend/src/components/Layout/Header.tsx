import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Menu,
  MenuItem,
  Avatar,
  FormControlLabel,
  Switch,
} from '@mui/material'
import { AccountCircle, Logout, DarkMode, LightMode } from '@mui/icons-material'
import { useState } from 'react'
import { useAuthStore } from '../../stores/authStore'
import { authService } from '../../services/authService'
import { useTheme } from '../../theme/useTheme'

const Header = () => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { user, logout } = useAuthStore()
  const { mode, toggleMode } = useTheme()

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleLogout = async () => {
    await authService.logout()
    handleClose()
  }

  return (
    <AppBar position="static" elevation={1}>
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Misafir Yönetim Sistemi
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="body2">{user?.fullName}</Typography>
          <FormControlLabel
            control={
              <Switch
                checked={mode === 'dark'}
                onChange={toggleMode}
                color="default"
                inputProps={{ 'aria-label': 'theme toggle' }}
              />
            }
            label={mode === 'dark' ? <DarkMode fontSize="small" /> : <LightMode fontSize="small" />}
            sx={{ color: 'inherit', ml: 1 }}
          />
          <IconButton
            size="large"
            edge="end"
            aria-label="account of current user"
            aria-controls="menu-appbar"
            aria-haspopup="true"
            onClick={handleMenu}
            color="inherit"
          >
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'secondary.main' }}>
              {user?.fullName?.charAt(0).toUpperCase()}
            </Avatar>
          </IconButton>
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
          >
            <MenuItem onClick={handleClose}>
              <AccountCircle sx={{ mr: 1 }} />
              Profil
            </MenuItem>
            <MenuItem onClick={handleLogout}>
              <Logout sx={{ mr: 1 }} />
              Çıkış Yap
            </MenuItem>
          </Menu>
        </Box>
      </Toolbar>
    </AppBar>
  )
}

export default Header

