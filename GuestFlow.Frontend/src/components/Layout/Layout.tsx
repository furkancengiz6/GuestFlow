import { ReactNode } from 'react'
import { Box, Container } from '@mui/material'
import Sidebar from './Sidebar'
import Header from './Header'

interface LayoutProps {
  children: ReactNode
}

const Layout = ({ children }: LayoutProps) => {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Sidebar />
      <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        <Header />
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            p: 3,
            backgroundColor: 'background.default',
          }}
        >
          <Container maxWidth="xl">{children}</Container>
        </Box>
      </Box>
    </Box>
  )
}

export default Layout

