import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Box } from '@mui/material'
import Layout from './components/Layout/Layout'
import LoginPage from './pages/Auth/LoginPage'
import DashboardPage from './pages/Dashboard/DashboardPage'
import GuestsPage from './pages/Guests/GuestsPage'
import GuestDetailPage from './pages/Guests/GuestDetailPage'
import TransfersPage from './pages/Transfers/TransfersPage'
import TransferDetailPage from './pages/Transfers/TransferDetailPage'
import ToursPage from './pages/Tours/ToursPage'
import CityTourDetailPage from './pages/Tours/CityTourDetailPage'
import YachtTourDetailPage from './pages/Tours/YachtTourDetailPage'
import InvoicesPage from './pages/Invoices/InvoicesPage'
import InvoiceDetailPage from './pages/Invoices/InvoiceDetailPage'
import PersonnelPage from './pages/Personnel/PersonnelPage'
import ReportsPage from './pages/Reports/ReportsPage'
import SettingsPage from './pages/Settings/SettingsPage'
import ForbiddenPage from './pages/ForbiddenPage'
import ProtectedRoute from './components/Auth/ProtectedRoute'

function App() {
  return (
    <BrowserRouter
      future={{
        v7_startTransition: true,
        v7_relativeSplatPath: true,
      }}
    >
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/*"
          element={
            <ProtectedRoute>
              <Layout>
                <Routes>
                  <Route path="/" element={<Navigate to="/dashboard" replace />} />
                  <Route path="/dashboard" element={<DashboardPage />} />
                  <Route path="/guests" element={<GuestsPage />} />
                  <Route path="/guests/:id" element={<GuestDetailPage />} />
                  <Route path="/transfers" element={<TransfersPage />} />
                  <Route path="/transfers/:id" element={<TransferDetailPage />} />
                  <Route path="/tours" element={<ToursPage />} />
                  <Route path="/tours/city/:id" element={<CityTourDetailPage />} />
                  <Route path="/tours/yacht/:id" element={<YachtTourDetailPage />} />
                  <Route path="/invoices" element={<InvoicesPage />} />
                  <Route path="/invoices/:id" element={<InvoiceDetailPage />} />
                  <Route
                    path="/personnel"
                    element={
                      <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                        <PersonnelPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/reports"
                    element={
                      <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                        <ReportsPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/settings"
                    element={
                      <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                        <SettingsPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route path="/forbidden" element={<ForbiddenPage />} />
                  <Route path="*" element={<Box p={3}>404 - Sayfa Bulunamadı</Box>} />
                </Routes>
              </Layout>
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  )
}

export default App

