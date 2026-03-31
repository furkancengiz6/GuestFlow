import { ReactNode } from 'react'
import { Box, Container } from '@mui/material'
import Sidebar from './Sidebar'
import Header from './Header'
import BreadcrumbsBar from '../Common/BreadcrumbsBar'
import AIChatWidget from '../ai/AIChatWidget'

interface LayoutProps {
  children: ReactNode
}

const Layout = ({ children }: LayoutProps) => {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', backgroundColor: 'background.default' }}>
      <Sidebar />
      <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        <Header />
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            p: { xs: 2, md: 3 },
            backgroundColor: '#F1F5F9', // Slightly darker slate for better card contrast
          }}
        >
          <Container maxWidth="xl" className="animate-fade-in">
            <BreadcrumbsBar />
            {children}
          </Container>
        </Box>
      </Box>
      <AIChatWidget />
    </Box>
  )
}

export default Layout
