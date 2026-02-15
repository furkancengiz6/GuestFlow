/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import { useState } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TextField,
  Button,
  Grid,
  Chip,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '../../components/ui'
import {
  IconButton,
  Tooltip,
} from '@mui/material'
import { useLoginAttempts, useLoginAuditStatistics, useFailedLoginSummary } from '../../hooks/useLoginAudit'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import SearchIcon from '@mui/icons-material/Search'
import RefreshIcon from '@mui/icons-material/Refresh'
import SecurityIcon from '@mui/icons-material/Security'
import WarningIcon from '@mui/icons-material/Warning'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'

const LoginAuditPage = () => {
  const [filters, setFilters] = useState({
    startDate: '',
    endDate: '',
    email: '',
    ipAddress: '',
    isSuccessful: undefined as boolean | undefined,
  })

  const [dateRange, setDateRange] = useState({
    start: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), // Last 7 days
    end: new Date(),
  })

  const { data: attempts, isLoading: attemptsLoading, refetch: refetchAttempts } = useLoginAttempts({
    startDate: dateRange.start.toISOString().split('T')[0],
    endDate: dateRange.end.toISOString().split('T')[0],
    email: filters.email || undefined,
    ipAddress: filters.ipAddress || undefined,
    isSuccessful: filters.isSuccessful,
  })

  const { data: statistics, isLoading: _statsLoading } = useLoginAuditStatistics({
    startDate: dateRange.start.toISOString().split('T')[0],
    endDate: dateRange.end.toISOString().split('T')[0],
  })

  const { data: failedSummary, isLoading: _summaryLoading } = useFailedLoginSummary({
    startDate: dateRange.start.toISOString().split('T')[0],
    endDate: dateRange.end.toISOString().split('T')[0],
    topCount: 10,
  })

  const handleFilterChange = (field: string, value: any) => {
    setFilters((prev) => ({ ...prev, [field]: value }))
  }

  const handleDateRangeChange = (field: 'start' | 'end', value: Date | null) => {
    if (value) {
      setDateRange((prev) => ({ ...prev, [field]: value }))
    }
  }

  const handleSearch = () => {
    refetchAttempts()
  }

  const handleReset = () => {
    setFilters({
      startDate: '',
      endDate: '',
      email: '',
      ipAddress: '',
      isSuccessful: undefined,
    })
    setDateRange({
      start: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000),
      end: new Date(),
    })
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
        <SecurityIcon sx={{ fontSize: 32, color: 'primary.main' }} />
        <Typography variant="h4" component="h1">
          Login Audit - Güvenlik İzleme
        </Typography>
      </Box>

      {/* Statistics Cards */}
      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Toplam Deneme
                </Typography>
                <Typography variant="h5">{statistics.totalAttempts}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Başarılı
                </Typography>
                <Typography variant="h5" color="success.main">
                  {statistics.successfulAttempts}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Başarısız
                </Typography>
                <Typography variant="h5" color="error.main">
                  {statistics.failedAttempts}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Başarı Oranı
                </Typography>
                <Typography variant="h5">
                  {statistics.successRate.toFixed(1)}%
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={6} md={3}>
              <TextField
                fullWidth
                label="Başlangıç Tarihi"
                type="date"
                value={format(dateRange.start, 'yyyy-MM-dd')}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleDateRangeChange('start', new Date(e.target.value))}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <TextField
                fullWidth
                label="Bitiş Tarihi"
                type="date"
                value={format(dateRange.end, 'yyyy-MM-dd')}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleDateRangeChange('end', new Date(e.target.value))}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={6} md={2}>
              <TextField
                fullWidth
                label="Email"
                value={filters.email}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleFilterChange('email', e.target.value)}
                placeholder="Email ara..."
              />
            </Grid>
            <Grid item xs={12} sm={6} md={2}>
              <TextField
                fullWidth
                label="IP Adresi"
                value={filters.ipAddress}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleFilterChange('ipAddress', e.target.value)}
                placeholder="IP ara..."
              />
            </Grid>
            <Grid item xs={12} sm={6} md={2}>
              <FormControl fullWidth>
                <InputLabel>Durum</InputLabel>
                <Select
                  value={filters.isSuccessful === undefined ? '' : filters.isSuccessful ? 'true' : 'false'}
                  onChange={(e: any) =>
                    handleFilterChange('isSuccessful', e.target.value === '' ? undefined : e.target.value === 'true')
                  }
                  label="Durum"
                >
                  <MenuItem value="">Tümü</MenuItem>
                  <MenuItem value="true">Başarılı</MenuItem>
                  <MenuItem value="false">Başarısız</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6} md={2}>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="contained" onClick={handleSearch} startIcon={<SearchIcon />}>
                  Ara
                </Button>
                <Button variant="outlined" onClick={handleReset}>
                  Sıfırla
                </Button>
                <Tooltip title="Yenile">
                  <IconButton onClick={() => refetchAttempts()}>
                    <RefreshIcon />
                  </IconButton>
                </Tooltip>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Failed Login Summary */}
      {failedSummary && failedSummary.length > 0 && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <WarningIcon color="warning" />
              En Çok Başarısız Deneme Yapan Kullanıcılar
            </Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Email</TableCell>
                    <TableCell>Kullanıcı</TableCell>
                    <TableCell align="right">Başarısız Deneme</TableCell>
                    <TableCell>Son Deneme</TableCell>
                    <TableCell>Son IP</TableCell>
                    <TableCell>En Yaygın Hata</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {failedSummary.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell>{item.email}</TableCell>
                      <TableCell>{item.personnelName || '-'}</TableCell>
                      <TableCell align="right">
                        <Chip label={item.failedAttemptCount} color="error" size="small" />
                      </TableCell>
                      <TableCell>
                        {format(new Date(item.lastFailedAttempt), 'dd MMM yyyy HH:mm', { locale: tr })}
                      </TableCell>
                      <TableCell>{item.lastIpAddress || '-'}</TableCell>
                      <TableCell>
                        {item.mostCommonFailureReason ? (
                          <Chip label={item.mostCommonFailureReason} size="small" variant="outlined" />
                        ) : (
                          '-'
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}

      {/* Login Attempts Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Login Denemeleri
          </Typography>
          {attemptsLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : attempts && attempts.length > 0 ? (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Tarih/Saat</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Kullanıcı</TableCell>
                    <TableCell>IP Adresi</TableCell>
                    <TableCell align="center">Durum</TableCell>
                    <TableCell>Hata Nedeni</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {attempts.map((attempt) => (
                    <TableRow key={attempt.id}>
                      <TableCell>
                        {format(new Date(attempt.attemptDate), 'dd MMM yyyy HH:mm:ss', { locale: tr })}
                      </TableCell>
                      <TableCell>{attempt.email}</TableCell>
                      <TableCell>{attempt.personnelName || '-'}</TableCell>
                      <TableCell>{attempt.ipAddress || '-'}</TableCell>
                      <TableCell align="center">
                        {attempt.isSuccessful ? (
                          <Chip
                            icon={<CheckCircleIcon />}
                            label="Başarılı"
                            color="success"
                            size="small"
                          />
                        ) : (
                          <Chip
                            icon={<CancelIcon />}
                            label="Başarısız"
                            color="error"
                            size="small"
                          />
                        )}
                      </TableCell>
                      <TableCell>
                        {attempt.failureReason ? (
                          <Chip label={attempt.failureReason} size="small" variant="outlined" color="error" />
                        ) : (
                          '-'
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Alert severity="info">Seçilen kriterlere uygun login denemesi bulunamadı.</Alert>
          )}
        </CardContent>
      </Card>
    </Box>
  )
}

export default LoginAuditPage
