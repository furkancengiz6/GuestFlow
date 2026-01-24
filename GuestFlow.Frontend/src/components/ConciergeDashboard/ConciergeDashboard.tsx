import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  IconButton,
  Tooltip,
  Alert,
  AlertTitle,
} from '@mui/material'
import {
  Login as CheckInIcon,
  Logout as CheckOutIcon,
  People as PeopleIcon,
  Room as RoomIcon,
  Phone as PhoneIcon,
  Email as EmailIcon,
  Star as VIPIcon,
  Visibility as ViewIcon,
  Sync as SyncIcon,
} from '@mui/icons-material'
import { useNavigate } from 'react-router-dom'
import { formatDate, formatTime } from '../../utils/formatters'
import ContentState from '../Feedback/ContentState'
import {
  useTodayCheckIns,
  useTodayCheckOuts,
  useActiveGuests,
  useUpcomingServices,
} from '../../hooks/useConciergeDashboard'
import type { CheckInOutItem, ActiveGuest } from '../../types/conciergeDashboard'

const ConciergeDashboard = () => {
  const navigate = useNavigate()
  const { data: checkIns, isLoading: isLoadingCheckIns } = useTodayCheckIns()
  const { data: checkOuts, isLoading: isLoadingCheckOuts } = useTodayCheckOuts()
  const { data: activeGuests, isLoading: isLoadingActiveGuests } = useActiveGuests()
  const { data: upcomingServices, isLoading: isLoadingServices } = useUpcomingServices()

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" gutterBottom>
          🏨 Concierge Dashboard
        </Typography>
        <Typography variant="body2" color="text.secondary">
          PMS entegrasyonlu misafir yönetim görünümü
        </Typography>
      </Box>

      {/* Check-in/Check-out Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        {/* Today's Check-ins */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <CheckInIcon color="success" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  Bugünkü Check-in'ler
                </Typography>
                <Chip
                  label={checkIns?.totalCount || 0}
                  color="success"
                  size="small"
                  sx={{ ml: 'auto' }}
                />
              </Box>
              {isLoadingCheckIns ? (
                <ContentState state="loading" skeletonLines={3} />
              ) : checkIns?.items && checkIns.items.length > 0 ? (
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Oda</TableCell>
                        <TableCell>Kaynak</TableCell>
                        <TableCell>İşlem</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {checkIns.items.slice(0, 5).map((item) => (
                        <TableRow key={`${item.guestId}-${item.source}`}>
                          <TableCell>
                            <Typography variant="body2" fontWeight="medium">
                              {item.guestName}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {item.guestCode}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            {item.roomNumber ? (
                              <Chip
                                label={item.roomNumber}
                                size="small"
                                icon={<RoomIcon />}
                              />
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={item.source}
                              size="small"
                              color={item.source === 'PMS' ? 'primary' : 'default'}
                              icon={item.source === 'PMS' ? <SyncIcon /> : undefined}
                            />
                          </TableCell>
                          <TableCell>
                            <Tooltip title="Misafir Detayı">
                              <IconButton
                                size="small"
                                onClick={() => navigate(`/guests/${item.guestId}`)}
                              >
                                <ViewIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Bugün check-in yok"
                  description="Bugün için check-in kaydı bulunmamaktadır."
                />
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Today's Check-outs */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <CheckOutIcon color="error" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  Bugünkü Check-out'lar
                </Typography>
                <Chip
                  label={checkOuts?.totalCount || 0}
                  color="error"
                  size="small"
                  sx={{ ml: 'auto' }}
                />
              </Box>
              {isLoadingCheckOuts ? (
                <ContentState state="loading" skeletonLines={3} />
              ) : checkOuts?.items && checkOuts.items.length > 0 ? (
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Oda</TableCell>
                        <TableCell>Kaynak</TableCell>
                        <TableCell>İşlem</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {checkOuts.items.slice(0, 5).map((item) => (
                        <TableRow key={`${item.guestId}-${item.source}`}>
                          <TableCell>
                            <Typography variant="body2" fontWeight="medium">
                              {item.guestName}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {item.guestCode}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            {item.roomNumber ? (
                              <Chip
                                label={item.roomNumber}
                                size="small"
                                icon={<RoomIcon />}
                              />
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={item.source}
                              size="small"
                              color={item.source === 'PMS' ? 'primary' : 'default'}
                              icon={item.source === 'PMS' ? <SyncIcon /> : undefined}
                            />
                          </TableCell>
                          <TableCell>
                            <Tooltip title="Misafir Detayı">
                              <IconButton
                                size="small"
                                onClick={() => navigate(`/guests/${item.guestId}`)}
                              >
                                <ViewIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Bugün check-out yok"
                  description="Bugün için check-out kaydı bulunmamaktadır."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Active Guests */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <PeopleIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  Aktif Misafirler
                </Typography>
                <Chip
                  label={activeGuests?.length || 0}
                  color="primary"
                  size="small"
                  sx={{ ml: 'auto' }}
                />
              </Box>
              {isLoadingActiveGuests ? (
                <ContentState state="loading" skeletonLines={5} />
              ) : activeGuests && activeGuests.length > 0 ? (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Oda</TableCell>
                        <TableCell>Check-in</TableCell>
                        <TableCell>Check-out</TableCell>
                        <TableCell>Gece</TableCell>
                        <TableCell>Yaklaşan Servisler</TableCell>
                        <TableCell>Kaynak</TableCell>
                        <TableCell>İşlem</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {activeGuests.map((guest) => (
                        <TableRow key={`${guest.guestId}-${guest.source}`} hover>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Typography variant="body2" fontWeight="medium">
                                {guest.guestName}
                              </Typography>
                              {guest.isVIP && (
                                <VIPIcon color="warning" fontSize="small" />
                              )}
                            </Box>
                            <Typography variant="caption" color="text.secondary">
                              {guest.guestCode}
                            </Typography>
                            {guest.email && (
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <EmailIcon fontSize="small" sx={{ fontSize: 12 }} />
                                <Typography variant="caption" color="text.secondary">
                                  {guest.email}
                                </Typography>
                              </Box>
                            )}
                            {guest.phoneNumber && (
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <PhoneIcon fontSize="small" sx={{ fontSize: 12 }} />
                                <Typography variant="caption" color="text.secondary">
                                  {guest.phoneNumber}
                                </Typography>
                              </Box>
                            )}
                          </TableCell>
                          <TableCell>
                            {guest.roomNumber ? (
                              <Chip
                                label={guest.roomNumber}
                                size="small"
                                icon={<RoomIcon />}
                              />
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            {guest.checkInDate ? (
                              <Typography variant="body2">
                                {formatDate(guest.checkInDate)}
                              </Typography>
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            {guest.checkOutDate ? (
                              <Typography variant="body2">
                                {formatDate(guest.checkOutDate)}
                              </Typography>
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            {guest.numberOfNights ? (
                              <Chip
                                label={`${guest.numberOfNights} gece`}
                                size="small"
                                color="info"
                              />
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            {guest.upcomingServices && guest.upcomingServices.length > 0 ? (
                              <Box>
                                {guest.upcomingServices.slice(0, 2).map((service) => (
                                  <Chip
                                    key={service.serviceId}
                                    label={`${service.serviceType} - ${formatTime(service.serviceDate)}`}
                                    size="small"
                                    color={service.isUrgent ? 'error' : 'default'}
                                    sx={{ mb: 0.5, display: 'block' }}
                                  />
                                ))}
                                {guest.upcomingServices.length > 2 && (
                                  <Typography variant="caption" color="text.secondary">
                                    +{guest.upcomingServices.length - 2} daha
                                  </Typography>
                                )}
                              </Box>
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                Yok
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={guest.source}
                              size="small"
                              color={guest.source === 'PMS' ? 'primary' : 'default'}
                              icon={guest.source === 'PMS' ? <SyncIcon /> : undefined}
                            />
                            {guest.pmsProviderName && (
                              <Typography variant="caption" color="text.secondary" display="block">
                                {guest.pmsProviderName}
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            <Tooltip title="Unified Guest Profile">
                              <IconButton
                                size="small"
                                onClick={() => navigate(`/guests/${guest.guestId}`)}
                              >
                                <ViewIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Aktif misafir yok"
                  description="Şu anda aktif misafir bulunmamaktadır."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Upcoming Services */}
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                📅 Yaklaşan Servisler (Bugün & Yarın)
              </Typography>
              {isLoadingServices ? (
                <ContentState state="loading" skeletonLines={3} />
              ) : upcomingServices?.items && upcomingServices.items.length > 0 ? (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Tarih/Saat</TableCell>
                        <TableCell>Servis Tipi</TableCell>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Oda</TableCell>
                        <TableCell>Lokasyon</TableCell>
                        <TableCell>Durum</TableCell>
                        <TableCell>İşlem</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {upcomingServices.items.map((service) => (
                        <TableRow key={`${service.serviceType}-${service.serviceId}`} hover>
                          <TableCell>
                            <Typography variant="body2" fontWeight="medium">
                              {formatDate(service.serviceDate)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {formatTime(service.serviceDate)}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={service.serviceType}
                              size="small"
                              color={
                                service.serviceType === 'Transfer'
                                  ? 'primary'
                                  : service.serviceType === 'CityTour'
                                  ? 'secondary'
                                  : 'success'
                              }
                            />
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2">
                              {service.guestName}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            {service.roomNumber ? (
                              <Chip
                                label={service.roomNumber}
                                size="small"
                                icon={<RoomIcon />}
                              />
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            {service.cityName ? (
                              <Typography variant="body2">
                                {service.cityName}
                              </Typography>
                            ) : (
                              <Typography variant="caption" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                          <TableCell>
                            <Chip
                              label={service.status || 'Pending'}
                              size="small"
                              color={service.isUrgent ? 'error' : 'default'}
                            />
                          </TableCell>
                          <TableCell>
                            <Button
                              size="small"
                              variant="outlined"
                              onClick={() => {
                                const routeMap: Record<string, string> = {
                                  Transfer: '/transfers',
                                  CityTour: '/tours',
                                  YachtTour: '/tours',
                                }
                                navigate(`${routeMap[service.serviceType] || '/dashboard'}/${service.serviceId}`)
                              }}
                            >
                              Detay
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Yaklaşan servis yok"
                  description="Bugün ve yarın için planlanmış servis bulunmamaktadır."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default ConciergeDashboard
