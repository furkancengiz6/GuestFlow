import { Breadcrumbs, Link, Typography } from '@mui/material'
import { useLocation, useNavigate } from 'react-router-dom'

const labelMap: Record<string, string> = {
  dashboard: 'Dashboard',
  guests: 'Misafirler',
  transfers: 'Transferler',
  tours: 'Turlar',
  invoices: 'Faturalar',
  reports: 'Raporlar',
  settings: 'Ayarlar',
}

export const BreadcrumbsBar = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const segments = location.pathname.split('/').filter(Boolean)

  const crumbs = segments.map((seg, idx) => {
    const href = '/' + segments.slice(0, idx + 1).join('/')
    const label = labelMap[seg] ?? seg
    const isLast = idx === segments.length - 1
    return { href, label, isLast }
  })

  if (crumbs.length === 0) return null

  return (
    <Breadcrumbs aria-label="breadcrumb" sx={{ mb: 2 }}>
      <Link underline="hover" color="inherit" onClick={() => navigate('/dashboard')} sx={{ cursor: 'pointer' }}>
        Ana Sayfa
      </Link>
      {crumbs.map((c) =>
        c.isLast ? (
          <Typography key={c.href} color="text.primary">
            {c.label}
          </Typography>
        ) : (
          <Link
            key={c.href}
            underline="hover"
            color="inherit"
            onClick={() => navigate(c.href)}
            sx={{ cursor: 'pointer' }}
          >
            {c.label}
          </Link>
        )
      )}
    </Breadcrumbs>
  )
}

export default BreadcrumbsBar

