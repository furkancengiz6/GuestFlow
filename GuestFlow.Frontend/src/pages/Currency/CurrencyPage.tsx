import { useState } from 'react'
import {
  Box,
  Typography,
  Button,
  TextField,
  Grid,
  Card,
  CardContent,
  Alert,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'
import {
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  Search as SearchIcon,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { currencyService, Currency } from '../../services/currencyService'
import ContentState from '../../components/Feedback/ContentState'
import { useNotification } from '../../hooks/useNotification'

const CurrencyPage = () => {
  const [searchTerm, setSearchTerm] = useState('')
  const [validationCode, setValidationCode] = useState('')
  const [symbolCode, setSymbolCode] = useState('')
  const [validationResult, setValidationResult] = useState<{ isValid: boolean; currencyCode: string } | null>(null)
  const [symbolResult, setSymbolResult] = useState<{ currencyCode: string; symbol: string } | null>(null)

  const notification = useNotification()

  // Get default currency
  const { data: defaultCurrency, isLoading: isLoadingDefault } = useQuery({
    queryKey: ['currency-default'],
    queryFn: () => currencyService.getDefaultCurrency(),
  })

  // Get supported currencies
  const { data: supportedCurrencies, isLoading: isLoadingSupported } = useQuery({
    queryKey: ['currency-supported'],
    queryFn: () => currencyService.getSupportedCurrencies(),
  })

  // Get currency settings
  const { data: currencySettings, isLoading: isLoadingSettings } = useQuery({
    queryKey: ['currency-settings'],
    queryFn: () => currencyService.getCurrencySettings(),
  })

  const handleValidateCurrency = async () => {
    if (!validationCode.trim()) {
      notification.showError('Lütfen bir para birimi kodu giriniz.')
      return
    }

    try {
      const result = await currencyService.validateCurrency(validationCode.toUpperCase())
      setValidationResult(result)
      if (result.isValid) {
        notification.showSuccess('Para birimi kodu geçerlidir.')
      } else {
        notification.showError('Para birimi kodu geçersizdir.')
      }
    } catch (error: any) {
      notification.showError(error?.response?.data?.message || 'Para birimi validasyonu yapılırken bir hata oluştu.')
    }
  }

  const handleGetSymbol = async () => {
    if (!symbolCode.trim()) {
      notification.showError('Lütfen bir para birimi kodu giriniz.')
      return
    }

    try {
      const result = await currencyService.getCurrencySymbol(symbolCode.toUpperCase())
      setSymbolResult(result)
      notification.showSuccess('Para birimi sembolü başarıyla getirildi.')
    } catch (error: any) {
      notification.showError(error?.response?.data?.message || 'Para birimi sembolü getirilirken bir hata oluştu.')
    }
  }

  const currencies = Array.isArray(supportedCurrencies) ? supportedCurrencies : (supportedCurrencies as any)?.data || []

  const filteredCurrencies = currencies.filter(
    (currency: Currency) =>
      currency.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      currency.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (isLoadingDefault || isLoadingSupported || isLoadingSettings) {
    return <ContentState state="loading" />
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Para Birimi Yönetimi
      </Typography>

      <Grid container spacing={3} sx={{ mt: 2 }}>
        {/* Default Currency Card */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Varsayılan Para Birimi
              </Typography>
              {defaultCurrency && (
                <Box sx={{ mt: 2 }}>
                  <Chip label={defaultCurrency} color="primary" size="medium" sx={{ fontSize: '1.2rem', p: 2 }} />
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Currency Settings Card */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Para Birimi Ayarları
              </Typography>
              {currencySettings ? (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Varsayılan: {currencySettings.defaultCurrency || 'Belirlenmedi'}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Desteklenen: {currencySettings.supportedCurrencies?.length || 0} para birimi
                  </Typography>
                </Box>
              ) : (
                <Typography variant="body2" color="text.secondary">Ayarlar yüklenemedi.</Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Currency Validation Card */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Para Birimi Validasyonu
              </Typography>
              <Box sx={{ mt: 2, display: 'flex', gap: 2 }}>
                <TextField
                  label="Para Birimi Kodu"
                  value={validationCode}
                  onChange={(e) => setValidationCode(e.target.value)}
                  placeholder="Örn: USD, EUR, TRY"
                  fullWidth
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      handleValidateCurrency()
                    }
                  }}
                />
                <Button variant="contained" onClick={handleValidateCurrency} sx={{ minWidth: 120 }}>
                  Kontrol Et
                </Button>
              </Box>
              {validationResult && (
                <Box sx={{ mt: 2 }}>
                  <Alert
                    severity={validationResult.isValid ? 'success' : 'error'}
                    icon={validationResult.isValid ? <CheckCircleIcon /> : <CancelIcon />}
                  >
                    {validationResult.isValid
                      ? `${validationResult.currencyCode} geçerli bir para birimi kodudur.`
                      : `${validationResult.currencyCode} geçerli bir para birimi kodu değildir.`}
                  </Alert>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Currency Symbol Card */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Para Birimi Sembolü
              </Typography>
              <Box sx={{ mt: 2, display: 'flex', gap: 2 }}>
                <TextField
                  label="Para Birimi Kodu"
                  value={symbolCode}
                  onChange={(e) => setSymbolCode(e.target.value)}
                  placeholder="Örn: USD, EUR, TRY"
                  fullWidth
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      handleGetSymbol()
                    }
                  }}
                />
                <Button variant="contained" onClick={handleGetSymbol} sx={{ minWidth: 120 }}>
                  Sembol Getir
                </Button>
              </Box>
              {symbolResult && (
                <Box sx={{ mt: 2 }}>
                  <Alert severity="success">
                    <Typography variant="body1">
                      <strong>{symbolResult.currencyCode}</strong> sembolü: <strong>{symbolResult.symbol}</strong>
                    </Typography>
                  </Alert>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Supported Currencies Table */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6">Desteklenen Para Birimleri</Typography>
                <TextField
                  size="small"
                  placeholder="Ara..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  InputProps={{
                    startAdornment: <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />,
                  }}
                  sx={{ width: 300 }}
                />
              </Box>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>
                        <strong>Kod</strong>
                      </TableCell>
                      <TableCell>
                        <strong>İsim</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Sembol</strong>
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filteredCurrencies && filteredCurrencies.length > 0 ? (
                      filteredCurrencies.map((currency: Currency) => (
                        <TableRow key={currency.code} hover>
                          <TableCell>
                            <Chip label={currency.code} color="primary" size="small" />
                          </TableCell>
                          <TableCell>{currency.name}</TableCell>
                          <TableCell>
                            <Typography variant="body1" sx={{ fontWeight: 'bold' }}>
                              {currency.symbol}
                            </Typography>
                          </TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={3} align="center">
                          <Typography variant="body2" color="text.secondary">
                            Para birimi bulunamadı.
                          </Typography>
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default CurrencyPage

