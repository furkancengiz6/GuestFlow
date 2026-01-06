import { Menu, MenuItem, IconButton, Tooltip, ListItemIcon, ListItemText } from '@mui/material'
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
          >
            <ListItemIcon>
              <span style={{ fontSize: '1.2rem' }}>{lang.flag}</span>
            </ListItemIcon>
            <ListItemText>{lang.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  )
}

export default LanguageSwitcher

