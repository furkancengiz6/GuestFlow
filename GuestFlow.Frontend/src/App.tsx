import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Suspense, lazy } from 'react'
import { Box, CircularProgress } from '@mui/material'
import Layout from './components/Layout/Layout'
import EnhancedErrorBoundary from './components/Common/EnhancedErrorBoundary'
import ProtectedRoute from './components/Auth/ProtectedRoute'
import { useTokenRefresh } from './hooks/useTokenRefresh'
import { useSessionTimeout } from './hooks/useSessionTimeout'

// Lazy load pages for code splitting
const LoginPage = lazy(() => import('./pages/Auth/LoginPage'))
const DashboardPage = lazy(() => import('./pages/Dashboard/DashboardPage'))
const GuestsPage = lazy(() => import('./pages/Guests/GuestsPage'))
const GuestDetailPage = lazy(() => import('./pages/Guests/GuestDetailPage'))
const TransfersPage = lazy(() => import('./pages/Transfers/TransfersPage'))
const TransferDetailPage = lazy(() => import('./pages/Transfers/TransferDetailPage'))
const ToursPage = lazy(() => import('./pages/Tours/ToursPage'))
const CityTourDetailPage = lazy(() => import('./pages/Tours/CityTourDetailPage'))
const YachtTourDetailPage = lazy(() => import('./pages/Tours/YachtTourDetailPage'))
const InvoicesPage = lazy(() => import('./pages/Invoices/InvoicesPage'))
const InvoiceDetailPage = lazy(() => import('./pages/Invoices/InvoiceDetailPage'))
const PersonnelPage = lazy(() => import('./pages/Personnel/PersonnelPage'))
const ReportsPage = lazy(() => import('./pages/Reports/ReportsPage'))
const SettingsPage = lazy(() => import('./pages/Settings/SettingsPage'))
const AirportsPage = lazy(() => import('./pages/Airports/AirportsPage'))
const CitiesPage = lazy(() => import('./pages/Cities/CitiesPage'))
const VehiclesPage = lazy(() => import('./pages/Vehicles/VehiclesPage'))
const DailyNotesPage = lazy(() => import('./pages/DailyNotes/DailyNotesPage'))
const DailyRevenuesPage = lazy(() => import('./pages/DailyRevenues/DailyRevenuesPage'))
const ReservationsPage = lazy(() => import('./pages/Reservations/ReservationsPage'))
const ReservationDetailPage = lazy(() => import('./pages/Reservations/ReservationDetailPage'))
const SmsPage = lazy(() => import('./pages/SMS/SmsPage'))
const EmailsPage = lazy(() => import('./pages/Emails/EmailsPage'))
const NotificationsPage = lazy(() => import('./pages/Notifications/NotificationsPage'))
const FilesPage = lazy(() => import('./pages/Files/FilesPage'))
const CalendarPage = lazy(() => import('./pages/Calendar/CalendarPage'))
const ForbiddenPage = lazy(() => import('./pages/ForbiddenPage'))
const HotelsPage = lazy(() => import('./pages/Hotels/HotelsPage'))
const RestaurantsPage = lazy(() => import('./pages/Restaurants/RestaurantsPage'))
const ItinerariesPage = lazy(() => import('./pages/Itineraries/ItinerariesPage'))
const ItineraryTimelinePage = lazy(() => import('./pages/Itineraries/ItineraryTimelinePage'))
const CurrencyPage = lazy(() => import('./pages/Currency/CurrencyPage'))
const ServicePackagesPage = lazy(() => import('./pages/ServicePackages/ServicePackagesPage'))
const PaymentsPage = lazy(() => import('./pages/Payments/PaymentsPage'))
const RoomAssignmentsPage = lazy(() => import('./pages/RoomAssignments/RoomAssignmentsPage'))

// Loading fallback component
const PageLoader = () => (
  <Box
    sx={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      minHeight: '50vh',
    }}
  >
    <CircularProgress />
  </Box>
)

function App() {
  // Automatic token refresh
  useTokenRefresh()

  // Session timeout management
  useSessionTimeout({
    timeout: 30 * 60 * 1000, // 30 minutes
    warningTime: 5 * 60 * 1000, // 5 minutes warning
  })

  return (
    <EnhancedErrorBoundary>
      <BrowserRouter
        future={{
          v7_startTransition: true,
          v7_relativeSplatPath: true,
        }}
      >
        <Routes>
          <Route
            path="/login"
            element={
              <Suspense fallback={<PageLoader />}>
                <LoginPage />
              </Suspense>
            }
          />
          <Route
            path="/*"
            element={
              <ProtectedRoute>
                <Layout>
                  <Routes>
                    <Route path="/" element={<Navigate to="/dashboard" replace />} />
                    <Route
                      path="/dashboard"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <DashboardPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/guests"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <GuestsPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/guests/:id"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <GuestDetailPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/transfers"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <TransfersPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/transfers/:id"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <TransferDetailPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/tours"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <ToursPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/tours/city/:id"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <CityTourDetailPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/tours/yacht/:id"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <YachtTourDetailPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/invoices"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <InvoicesPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/invoices/:id"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <InvoiceDetailPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/personnel"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <PersonnelPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/reports"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <ReportsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/settings"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <SettingsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/airports"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <AirportsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/cities"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <CitiesPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/vehicles"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <VehiclesPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/daily-notes"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <DailyNotesPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/daily-revenues"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <DailyRevenuesPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/reservations"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <ReservationsPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/reservations/:id"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <ReservationDetailPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/sms"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <SmsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/emails"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <EmailsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/notifications"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <NotificationsPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/files"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <FilesPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/calendar"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <CalendarPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/hotels"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <HotelsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/restaurants"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <RestaurantsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/itineraries"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <ItinerariesPage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/itineraries/:id/timeline"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <ItineraryTimelinePage />
                        </Suspense>
                      }
                    />
                    <Route
                      path="/currency"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <CurrencyPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/service-packages"
                      element={
                        <ProtectedRoute roles={['Admin', 'Staff']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <ServicePackagesPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/payments"
                      element={
                        <ProtectedRoute roles={['Admin', 'Staff']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <PaymentsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/room-assignments"
                      element={
                        <ProtectedRoute roles={['Admin', 'Staff']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <RoomAssignmentsPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/forbidden"
                      element={
                        <Suspense fallback={<PageLoader />}>
                          <ForbiddenPage />
                        </Suspense>
                      }
                    />
                    <Route path="*" element={<Box p={3}>404 - Sayfa Bulunamadı</Box>} />
                  </Routes>
                </Layout>
              </ProtectedRoute>
            }
          />
        </Routes>
    </BrowserRouter>
    </EnhancedErrorBoundary>
  )
}

export default App

