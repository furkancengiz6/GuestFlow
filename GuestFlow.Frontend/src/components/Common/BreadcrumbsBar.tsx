import { Breadcrumbs, Link, Typography, Box } from '@mui/material'
import { Home as HomeIcon } from '@mui/icons-material'
import { useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from '../../hooks/useTranslation'

const labelMap: Record<string, string> = {
  dashboard: 'Dashboard',
  guests: 'Misafirler',
  transfers: 'Transferler',
  tours: 'Turlar',
  invoices: 'Faturalar',
  reports: 'Raporlar',
  settings: 'Ayarlar',
  hotels: 'Oteller',
  restaurants: 'Restoranlar',
  itineraries: 'İtineraryler',
  'service-packages': 'Servis Paketleri',
  personnel: 'Personel',
  vehicles: 'Araçlar',
  airports: 'Havalimanları',
  cities: 'Şehirler',
  currency: 'Para Birimi',
  notifications: 'Bildirimler',
  files: 'Dosyalar',
  calendar: 'Takvim',
  'daily-revenues': 'Günlük Gelirler',
  'daily-notes': 'Günlük Notlar',
  reservations: 'Rezervasyonlar',
}

export const BreadcrumbsBar = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const { t } = useTranslation()
  const segments = location.pathname.split('/').filter(Boolean)

  const crumbs = segments.map((seg, idx) => {
    const href = '/' + segments.slice(0, idx + 1).join('/')
    const label = labelMap[seg] ?? t(`navigation.${seg}`) ?? seg
    const isLast = idx === segments.length - 1
    return { href, label, isLast }
  })

  if (crumbs.length === 0) return null

  return (
    <Box sx={{ mb: 2 }}>
      <Breadcrumbs aria-label="breadcrumb" separator="›">
        <Link
          underline="hover"
          color="inherit"
          onClick={() => navigate('/dashboard')}
          sx={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 0.5 }}
        >
          <HomeIcon fontSize="small" />
          {t('navigation.dashboard') || 'Ana Sayfa'}
        </Link>
        {crumbs.map((c) =>
          c.isLast ? (
            <Typography key={c.href} color="text.primary" sx={{ fontWeight: 500 }}>
              {c.label}
            </Typography>
          ) : (
            <Link
              key={c.href}
              underline="hover"
              color="inherit"
              onClick={() => navigate(c.href)}
              sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }}
            >
              {c.label}
            </Link>
          )
        )}
      </Breadcrumbs>
    </Box>
  )
}

export default BreadcrumbsBar

