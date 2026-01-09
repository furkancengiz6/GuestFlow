import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Suspense } from 'react'
import { Box, CircularProgress } from './components/ui'
import Layout from './components/Layout/Layout'
import EnhancedErrorBoundary from './components/Common/EnhancedErrorBoundary'
import ProtectedRoute from './components/Auth/ProtectedRoute'
import { useTokenRefresh } from './hooks/useTokenRefresh'
import { useSessionTimeout } from './hooks/useSessionTimeout'
import { lazyLoad } from './utils/performance'

// Lazy load pages for code splitting - grouped by priority with error handling
// High priority pages (always loaded)
const LoginPage = lazyLoad(() => import('./pages/Auth/LoginPage'), 'LoginPage')
const DashboardPage = lazyLoad(() => import('./pages/Dashboard/DashboardPage'), 'DashboardPage')
const ForbiddenPage = lazyLoad(() => import('./pages/ForbiddenPage'), 'ForbiddenPage')

// Medium priority pages (common operations)
const GuestsPage = lazyLoad(() => import('./pages/Guests/GuestsPage'), 'GuestsPage')
const GuestDetailPage = lazyLoad(() => import('./pages/Guests/GuestDetailPage'), 'GuestDetailPage')
const TransfersPage = lazyLoad(() => import('./pages/Transfers/TransfersPage'), 'TransfersPage')
const TransferDetailPage = lazyLoad(() => import('./pages/Transfers/TransferDetailPage'), 'TransferDetailPage')
const InvoicesPage = lazyLoad(() => import('./pages/Invoices/InvoicesPage'), 'InvoicesPage')
const InvoiceDetailPage = lazyLoad(() => import('./pages/Invoices/InvoiceDetailPage'), 'InvoiceDetailPage')
const ReservationsPage = lazyLoad(() => import('./pages/Reservations/ReservationsPage'), 'ReservationsPage')
const ReservationDetailPage = lazyLoad(() => import('./pages/Reservations/ReservationDetailPage'), 'ReservationDetailPage')

// Low priority pages (admin features) - loaded on demand
const ToursPage = lazyLoad(() => import('./pages/Tours/ToursPage'), 'ToursPage')
const CityTourDetailPage = lazyLoad(() => import('./pages/Tours/CityTourDetailPage'), 'CityTourDetailPage')
const YachtTourDetailPage = lazyLoad(() => import('./pages/Tours/YachtTourDetailPage'), 'YachtTourDetailPage')
const PersonnelPage = lazyLoad(() => import('./pages/Personnel/PersonnelPage'), 'PersonnelPage')
const ReportsPage = lazyLoad(() => import('./pages/Reports/ReportsPage'), 'ReportsPage')
const SettingsPage = lazyLoad(() => import('./pages/Settings/SettingsPage'), 'SettingsPage')
const AirportsPage = lazyLoad(() => import('./pages/Airports/AirportsPage'), 'AirportsPage')
const CitiesPage = lazyLoad(() => import('./pages/Cities/CitiesPage'), 'CitiesPage')
const VehiclesPage = lazyLoad(() => import('./pages/Vehicles/VehiclesPage'), 'VehiclesPage')
const DailyNotesPage = lazyLoad(() => import('./pages/DailyNotes/DailyNotesPage'), 'DailyNotesPage')
const DailyRevenuesPage = lazyLoad(() => import('./pages/DailyRevenues/DailyRevenuesPage'), 'DailyRevenuesPage')
const SmsPage = lazyLoad(() => import('./pages/SMS/SmsPage'), 'SmsPage')
const EmailsPage = lazyLoad(() => import('./pages/Emails/EmailsPage'), 'EmailsPage')
const NotificationsPage = lazyLoad(() => import('./pages/Notifications/NotificationsPage'), 'NotificationsPage')
const FilesPage = lazyLoad(() => import('./pages/Files/FilesPage'), 'FilesPage')
const CalendarPage = lazyLoad(() => import('./pages/Calendar/CalendarPage'), 'CalendarPage')
const HotelsPage = lazyLoad(() => import('./pages/Hotels/HotelsPage'), 'HotelsPage')
const RestaurantsPage = lazyLoad(() => import('./pages/Restaurants/RestaurantsPage'), 'RestaurantsPage')
const ItinerariesPage = lazyLoad(() => import('./pages/Itineraries/ItinerariesPage'), 'ItinerariesPage')
const ItineraryTimelinePage = lazyLoad(() => import('./pages/Itineraries/ItineraryTimelinePage'), 'ItineraryTimelinePage')
const CurrencyPage = lazyLoad(() => import('./pages/Currency/CurrencyPage'), 'CurrencyPage')
const ServicePackagesPage = lazyLoad(() => import('./pages/ServicePackages/ServicePackagesPage'), 'ServicePackagesPage')
const PaymentsPage = lazyLoad(() => import('./pages/Payments/PaymentsPage'), 'PaymentsPage')
const RoomAssignmentsPage = lazyLoad(() => import('./pages/RoomAssignments/RoomAssignmentsPage'), 'RoomAssignmentsPage')
const SuppliersPage = lazyLoad(() => import('./pages/Suppliers/SuppliersPage'), 'SuppliersPage')
const SupplierCostsPage = lazyLoad(() => import('./pages/Suppliers/SupplierCostsPage'), 'SupplierCostsPage')

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
                      path="/suppliers"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <SuppliersPage />
                          </Suspense>
                        </ProtectedRoute>
                      }
                    />
                    <Route
                      path="/suppliers/costs"
                      element={
                        <ProtectedRoute roles={['Admin']} fallbackPath="/dashboard">
                          <Suspense fallback={<PageLoader />}>
                            <SupplierCostsPage />
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

