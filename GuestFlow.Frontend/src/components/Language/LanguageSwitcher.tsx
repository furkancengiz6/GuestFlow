import { Menu, MenuItem, IconButton, Tooltip, ListItemIcon, ListItemText, Box } from '@mui/material'
import { Language as LanguageIcon } from '@mui/icons-material'
import { useState } from 'react'
import { useTranslation } from '../../hooks/useTranslation'

/**
 * Language switcher component
 */
export const LanguageSwitcher = () => {
  const { t, currentLanguage, changeLanguage } = useTranslation()
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleLanguageChange = (lng: string) => {
    changeLanguage(lng)
    handleClose()
  }

  const languages = [
    { code: 'tr', label: t('language.turkish'), flag: '🇹🇷' },
    { code: 'en', label: t('language.english'), flag: '🇬🇧' },
  ]

  return (
    <>
      <Tooltip title={t('language.changeLanguage')}>
        <IconButton onClick={handleClick} color="inherit">
          <LanguageIcon />
        </IconButton>
      </Tooltip>
      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleClose}>
        {languages.map((lang) => (
          <MenuItem
            key={lang.code}
            onClick={() => handleLanguageChange(lang.code)}
            selected={currentLanguage === lang.code}
            sx={{
              borderRadius: 1,
              mx: 0.5,
              mb: 0.5,
              '&.Mui-selected': {
                bgcolor: 'primary.light',
                color: 'primary.contrastText',
                '&:hover': { bgcolor: 'primary.main' }
              }
            }}
          >
            <ListItemIcon>
              <Box
                sx={{
                  width: 24,
                  height: 24,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  borderRadius: '50%',
                  fontSize: '1rem',
                  boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                  border: '1px solid rgba(0,0,0,0.05)',
                  bgcolor: 'background.paper',
                  transform: 'scale(1.2)'
                }}
              >
                {lang.flag}
              </Box>
            </ListItemIcon>
            <ListItemText primary={lang.label} />
          </MenuItem>
        ))}
      </Menu>
    </>
  )
}

export default LanguageSwitcher

