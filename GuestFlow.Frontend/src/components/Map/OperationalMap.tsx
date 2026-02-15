// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import React, { useState, useEffect, useMemo } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Button,
  Grid,
  Paper,
  Alert,
  CircularProgress,
} from '@mui/material'
import {
  FilterList as FilterIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material'
import { GoogleMap, Marker, InfoWindow, Polyline, useJsApiLoader } from '@react-google-maps/api'
import { useMapView } from '../../hooks/useMap'
import type { MapFilter, MapServiceLocation } from '../../types/map'
import { format } from 'date-fns'

const mapContainerStyle = {
  width: '100%',
  height: '600px',
}

const defaultCenter = {
  lat: 41.0082, // İstanbul
  lng: 28.9784,
}

const OperationalMap: React.FC = () => {
  const [filter, setFilter] = useState<MapFilter>({
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
  })
  const [selectedService, setSelectedService] = useState<MapServiceLocation | null>(null)
  const [mapCenter, setMapCenter] = useState(defaultCenter)
  const [mapZoom, setMapZoom] = useState(10)

  const { data: mapView, isLoading, error, refetch } = useMapView(filter)

  // Load Google Maps API
  const { isLoaded, loadError } = useJsApiLoader({
    id: 'google-map-script',
    googleMapsApiKey: process.env.VITE_GOOGLE_MAPS_API_KEY || '',
  })

  // Calculate map bounds from services
  const bounds = useMemo(() => {
    if (!mapView?.services || mapView.services.length === 0) return null

    const locations = mapView.services
      .flatMap((s) => [s.pickupLocation, s.dropoffLocation])
      .filter((l): l is NonNullable<typeof l> => l != null)

    if (locations.length === 0) return null

    const lats = locations.map((l) => l.latitude)
    const lngs = locations.map((l) => l.longitude)

    return {
      north: Math.max(...lats),
      south: Math.min(...lats),
      east: Math.max(...lngs),
      west: Math.min(...lngs),
    }
  }, [mapView])

  // Update map center and zoom when bounds change
  useEffect(() => {
    if (bounds) {
      const centerLat = (bounds.north + bounds.south) / 2
      const centerLng = (bounds.east + bounds.west) / 2
      setMapCenter({ lat: centerLat, lng: centerLng })

      // Calculate zoom based on bounds
      const latDiff = bounds.north - bounds.south
      const lngDiff = bounds.east - bounds.west
      const maxDiff = Math.max(latDiff, lngDiff)
      const zoom = maxDiff > 0.5 ? 8 : maxDiff > 0.2 ? 10 : maxDiff > 0.1 ? 12 : 14
      setMapZoom(zoom)
    }
  }, [bounds])

  const getMarkerColor = (colorCode?: string) => {
    switch (colorCode) {
      case 'green':
        return '#4caf50'
      case 'yellow':
        return '#ffc107'
      case 'red':
        return '#f44336'
      case 'blue':
        return '#2196f3'
      default:
        return '#9e9e9e'
    }
  }

  const getMarkerIcon = (colorCode?: string) => {
    getMarkerColor(colorCode)
    // Return undefined to use default marker if google.maps is not available
    // The icon will be set after Google Maps loads
    return undefined
  }

  const handleServiceClick = (service: MapServiceLocation) => {
    setSelectedService(service)
    if (service.pickupLocation) {
      setMapCenter({
        lat: service.pickupLocation.latitude,
        lng: service.pickupLocation.longitude,
      })
      setMapZoom(14)
    }
  }

  const handleRefresh = () => {
    refetch()
  }

  if (!isLoaded) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (loadError) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        Google Maps yüklenirken bir hata oluştu. Lütfen API key'inizi kontrol edin.
      </Alert>
    )
  }

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        Harita verileri yüklenirken bir hata oluştu
      </Alert>
    )
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4">Operasyonel Harita</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
          >
            Yenile
          </Button>
        </Box>
      </Box>

      {/* Statistics Cards */}
      {mapView?.statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={2}>
            <Card>
              <CardContent>
                <Typography variant="h6">{mapView.statistics.totalServices}</Typography>
                <Typography variant="body2" color="text.secondary">
                  Toplam Servis
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={2}>
            <Card>
              <CardContent>
                <Typography variant="h6" color="success.main">
                  {mapView.statistics.completedServices}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Tamamlanan
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={2}>
            <Card>
              <CardContent>
                <Typography variant="h6" color="info.main">
                  {mapView.statistics.inProgressServices}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Devam Eden
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={2}>
            <Card>
              <CardContent>
                <Typography variant="h6" color="warning.main">
                  {mapView.statistics.urgentServices}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Acil
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={2}>
            <Card>
              <CardContent>
                <Typography variant="h6" color="error.main">
                  {mapView.statistics.delayedServices}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Gecikmeli
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={6} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Başlangıç Tarihi</InputLabel>
              <Select
                value={filter.startDate || ''}
                label="Başlangıç Tarihi"
                onChange={(e) =>
                  setFilter({ ...filter, startDate: e.target.value })
                }
              >
                <MenuItem value={new Date().toISOString().split('T')[0]}>
                  Bugün
                </MenuItem>
                <MenuItem
                  value={new Date(Date.now() + 86400000).toISOString().split('T')[0]}
                >
                  Yarın
                </MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Servis Tipi</InputLabel>
              <Select
                multiple
                value={filter.serviceTypes || []}
                label="Servis Tipi"
                onChange={(e) =>
                  setFilter({
                    ...filter,
                    serviceTypes: e.target.value as string[],
                  })
                }
              >
                <MenuItem value="Transfer">Transfer</MenuItem>
                <MenuItem value="CityTour">Şehir Turu</MenuItem>
                <MenuItem value="YachtTour">Yat Turu</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Durum</InputLabel>
              <Select
                multiple
                value={filter.statuses || []}
                label="Durum"
                onChange={(e) =>
                  setFilter({
                    ...filter,
                    statuses: e.target.value as string[],
                  })
                }
              >
                <MenuItem value="Confirmed">Onaylandı</MenuItem>
                <MenuItem value="InProgress">Devam Ediyor</MenuItem>
                <MenuItem value="Completed">Tamamlandı</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Button
              variant="contained"
              fullWidth
              startIcon={<FilterIcon />}
              onClick={() => refetch()}
            >
              Filtrele
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Map */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          <GoogleMap
            mapContainerStyle={mapContainerStyle}
            center={mapCenter}
            zoom={mapZoom}
            options={{
              mapTypeControl: true,
              streetViewControl: false,
              fullscreenControl: true,
            }}
          >
            {/* Markers */}
            {mapView?.services.map((service) => {
              const markers: React.ReactNode[] = []

              // Pickup marker
              if (service.pickupLocation) {
                markers.push(
                  <Marker
                    key={`pickup-${service.serviceId}`}
                    position={{
                      lat: service.pickupLocation.latitude,
                      lng: service.pickupLocation.longitude,
                    }}
                    icon={getMarkerIcon(service.colorCode)}
                    onClick={() => handleServiceClick(service)}
                    title={`${service.serviceName} - Pickup`}
                  />
                )
              }

              // Dropoff marker
              if (service.dropoffLocation) {
                markers.push(
                  <Marker
                    key={`dropoff-${service.serviceId}`}
                    position={{
                      lat: service.dropoffLocation.latitude,
                      lng: service.dropoffLocation.longitude,
                    }}
                    icon={getMarkerIcon(service.colorCode)}
                    onClick={() => handleServiceClick(service)}
                    title={`${service.serviceName} - Dropoff`}
                  />
                )
              }

              // Polyline between pickup and dropoff
              if (
                service.pickupLocation &&
                service.dropoffLocation &&
                service.serviceType === 'Transfer'
              ) {
                markers.push(
                  <Polyline
                    key={`line-${service.serviceId}`}
                    path={[
                      {
                        lat: service.pickupLocation.latitude,
                        lng: service.pickupLocation.longitude,
                      },
                      {
                        lat: service.dropoffLocation.latitude,
                        lng: service.dropoffLocation.longitude,
                      },
                    ]}
                    options={{
                      strokeColor: getMarkerColor(service.colorCode),
                      strokeOpacity: 0.6,
                      strokeWeight: 3,
                    }}
                  />
                )
              }

              return markers
            })}

            {/* Info Window */}
            {selectedService && selectedService.pickupLocation && (
              <InfoWindow
                position={{
                  lat: selectedService.pickupLocation.latitude,
                  lng: selectedService.pickupLocation.longitude,
                }}
                onCloseClick={() => setSelectedService(null)}
              >
                <Box sx={{ p: 1, minWidth: 200 }}>
                  <Typography variant="subtitle2" fontWeight="bold">
                    {selectedService.serviceName}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {selectedService.serviceType}
                  </Typography>
                  <Typography variant="body2">
                    Misafir: {selectedService.guestName}
                  </Typography>
                  {selectedService.roomNumber && (
                    <Typography variant="body2">
                      Oda: {selectedService.roomNumber}
                    </Typography>
                  )}
                  <Typography variant="body2">
                    Tarih: {format(new Date(selectedService.serviceDate), 'dd.MM.yyyy HH:mm')}
                  </Typography>
                  <Box sx={{ mt: 1, display: 'flex', gap: 1 }}>
                    <Chip
                      label={selectedService.status}
                      size="small"
                      color={
                        selectedService.status === 'Completed'
                          ? 'success'
                          : selectedService.status === 'InProgress'
                            ? 'info'
                            : 'default'
                      }
                    />
                    {selectedService.isUrgent && (
                      <Chip label="Acil" size="small" color="warning" />
                    )}
                    {selectedService.isDelayed && (
                      <Chip label="Gecikmeli" size="small" color="error" />
                    )}
                  </Box>
                </Box>
              </InfoWindow>
            )}
          </GoogleMap>
        </CardContent>
      </Card>
    </Box>
  )
}

export default OperationalMap
