import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  TextField,
  FormControlLabel,
  Checkbox,
  Button,
  Alert,
  Chip,
} from '@mui/material'
import {
  Room as RoomIcon,
  Restaurant as RestaurantIcon,
  Sports as SportsIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  WhatsApp as WhatsAppIcon,
  Sms as SmsIcon,
} from '@mui/icons-material'
import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { guestService } from '../../services/guestService'
import type { GuestPreferences as GuestPreferencesType, UpsertGuestPreferences } from '../../types/guestPreferences'
import ContentState from '../Feedback/ContentState'

interface GuestPreferencesProps {
  guestId: number
  readOnly?: boolean
}

const GuestPreferences = ({ guestId, readOnly = false }: GuestPreferencesProps) => {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState<UpsertGuestPreferences>({
    guestId,
    prefersEmail: true,
    prefersSMS: true,
    prefersWhatsApp: false,
    prefersPhone: true,
    source: 'Manual',
  })

  const { data: preferences, isLoading, error } = useQuery<GuestPreferencesType>({
    queryKey: ['guest-preferences', guestId],
    queryFn: () => guestService.getGuestPreferences(guestId),
    enabled: !!guestId,
    retry: false, // 404 durumunda retry yapma
  })

  const upsertMutation = useMutation({
    mutationFn: (data: UpsertGuestPreferences) => guestService.upsertGuestPreferences(guestId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['guest-preferences', guestId] })
    },
  })

  useEffect(() => {
    if (preferences) {
      setFormData({
        guestId: preferences.guestId,
        preferredRoomType: preferences.preferredRoomType || '',
        roomSpecialRequests: preferences.roomSpecialRequests || '',
        bedPreference: preferences.bedPreference || '',
        smokingPreference: preferences.smokingPreference || '',
        dietaryPreferences: preferences.dietaryPreferences || '',
        foodAllergies: preferences.foodAllergies || '',
        specialFoodRequests: preferences.specialFoodRequests || '',
        activityPreferences: preferences.activityPreferences || '',
        interests: preferences.interests || '',
        prefersEmail: preferences.prefersEmail,
        prefersSMS: preferences.prefersSMS,
        prefersWhatsApp: preferences.prefersWhatsApp,
        prefersPhone: preferences.prefersPhone,
        preferredLanguage: preferences.preferredLanguage || '',
        notes: preferences.notes || '',
        source: preferences.source,
      })
    }
  }, [preferences])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await upsertMutation.mutateAsync(formData)
  }

  const handleChange = (field: keyof UpsertGuestPreferences, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }))
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={5} />
  }

  if (error && !preferences) {
    // Preferences yoksa, boş form göster
    return (
      <Box>
        <Alert severity="info" sx={{ mb: 2 }}>
          Bu misafir için henüz tercih kaydı bulunmamaktadır. Yeni tercih kaydı oluşturabilirsiniz.
        </Alert>
        {!readOnly && (
          <form onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              {/* Oda Tercihleri */}
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                      <RoomIcon color="primary" />
                      <Typography variant="h6">Oda Tercihleri</Typography>
                    </Box>
                    <Grid container spacing={2}>
                      <Grid item xs={12} md={6}>
                        <TextField
                          fullWidth
                          label="Tercih Edilen Oda Tipi"
                          value={formData.preferredRoomType || ''}
                          onChange={(e) => handleChange('preferredRoomType', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: Deluxe, Suite, Sea View"
                        />
                      </Grid>
                      <Grid item xs={12} md={6}>
                        <TextField
                          fullWidth
                          label="Yatak Tercihi"
                          value={formData.bedPreference || ''}
                          onChange={(e) => handleChange('bedPreference', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: Twin, Double, King"
                        />
                      </Grid>
                      <Grid item xs={12} md={6}>
                        <TextField
                          fullWidth
                          label="Sigara Tercihi"
                          value={formData.smokingPreference || ''}
                          onChange={(e) => handleChange('smokingPreference', e.target.value)}
                          disabled={readOnly}
                          placeholder="Smoking / Non-smoking"
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          multiline
                          rows={3}
                          label="Özel Oda İstekleri"
                          value={formData.roomSpecialRequests || ''}
                          onChange={(e) => handleChange('roomSpecialRequests', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: High floor, sea view, quiet room"
                        />
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>

              {/* Yemek Tercihleri */}
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                      <RestaurantIcon color="primary" />
                      <Typography variant="h6">Yemek Tercihleri</Typography>
                    </Box>
                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Diyet Tercihleri"
                          value={formData.dietaryPreferences || ''}
                          onChange={(e) => handleChange('dietaryPreferences', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: Vegan, Vegetarian, Halal, Kosher"
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Gıda Alerjileri"
                          value={formData.foodAllergies || ''}
                          onChange={(e) => handleChange('foodAllergies', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: Peanut, Dairy, Gluten"
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          multiline
                          rows={3}
                          label="Özel Yemek İstekleri"
                          value={formData.specialFoodRequests || ''}
                          onChange={(e) => handleChange('specialFoodRequests', e.target.value)}
                          disabled={readOnly}
                        />
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>

              {/* Aktivite Tercihleri */}
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                      <SportsIcon color="primary" />
                      <Typography variant="h6">Aktivite Tercihleri</Typography>
                    </Box>
                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Aktivite Tercihleri"
                          value={formData.activityPreferences || ''}
                          onChange={(e) => handleChange('activityPreferences', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: Spor, Kültür, Eğlence"
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          multiline
                          rows={3}
                          label="İlgi Alanları"
                          value={formData.interests || ''}
                          onChange={(e) => handleChange('interests', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: Müze, Plaj, Gece hayatı, Spa"
                        />
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>

              {/* İletişim Tercihleri */}
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                      <EmailIcon color="primary" />
                      <Typography variant="h6">İletişim Tercihleri</Typography>
                    </Box>
                    <Grid container spacing={2}>
                      <Grid item xs={12} md={6}>
                        <FormControlLabel
                          control={
                            <Checkbox
                              checked={formData.prefersEmail}
                              onChange={(e) => handleChange('prefersEmail', e.target.checked)}
                              disabled={readOnly}
                            />
                          }
                          label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <EmailIcon fontSize="small" />
                              E-posta
                            </Box>
                          }
                        />
                      </Grid>
                      <Grid item xs={12} md={6}>
                        <FormControlLabel
                          control={
                            <Checkbox
                              checked={formData.prefersSMS}
                              onChange={(e) => handleChange('prefersSMS', e.target.checked)}
                              disabled={readOnly}
                            />
                          }
                          label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <SmsIcon fontSize="small" />
                              SMS
                            </Box>
                          }
                        />
                      </Grid>
                      <Grid item xs={12} md={6}>
                        <FormControlLabel
                          control={
                            <Checkbox
                              checked={formData.prefersWhatsApp}
                              onChange={(e) => handleChange('prefersWhatsApp', e.target.checked)}
                              disabled={readOnly}
                            />
                          }
                          label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <WhatsAppIcon fontSize="small" />
                              WhatsApp
                            </Box>
                          }
                        />
                      </Grid>
                      <Grid item xs={12} md={6}>
                        <FormControlLabel
                          control={
                            <Checkbox
                              checked={formData.prefersPhone}
                              onChange={(e) => handleChange('prefersPhone', e.target.checked)}
                              disabled={readOnly}
                            />
                          }
                          label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <PhoneIcon fontSize="small" />
                              Telefon
                            </Box>
                          }
                        />
                      </Grid>
                      <Grid item xs={12} md={6}>
                        <TextField
                          fullWidth
                          label="Tercih Edilen Dil"
                          value={formData.preferredLanguage || ''}
                          onChange={(e) => handleChange('preferredLanguage', e.target.value)}
                          disabled={readOnly}
                          placeholder="Örn: TR, EN, DE"
                        />
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>

              {/* Genel Notlar */}
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Genel Notlar
                    </Typography>
                    <TextField
                      fullWidth
                      multiline
                      rows={4}
                      label="Notlar"
                      value={formData.notes || ''}
                      onChange={(e) => handleChange('notes', e.target.value)}
                      disabled={readOnly}
                    />
                  </CardContent>
                </Card>
              </Grid>

              {!readOnly && (
                <Grid item xs={12}>
                  <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                    <Button
                      type="submit"
                      variant="contained"
                      color="primary"
                      disabled={upsertMutation.isPending}
                    >
                      {preferences ? 'Güncelle' : 'Kaydet'}
                    </Button>
                  </Box>
                </Grid>
              )}
            </Grid>
          </form>
        )}
      </Box>
    )
  }

  return (
    <Box>
      {preferences && (
        <Alert severity="success" sx={{ mb: 2 }}>
          <Typography variant="body2">
            Kaynak: <Chip label={preferences.source} size="small" sx={{ ml: 1 }} />
          </Typography>
        </Alert>
      )}

      <form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          {/* Oda Tercihleri */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <RoomIcon color="primary" />
                  <Typography variant="h6">Oda Tercihleri</Typography>
                </Box>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Tercih Edilen Oda Tipi"
                      value={formData.preferredRoomType || ''}
                      onChange={(e) => handleChange('preferredRoomType', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: Deluxe, Suite, Sea View"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Yatak Tercihi"
                      value={formData.bedPreference || ''}
                      onChange={(e) => handleChange('bedPreference', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: Twin, Double, King"
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Sigara Tercihi"
                      value={formData.smokingPreference || ''}
                      onChange={(e) => handleChange('smokingPreference', e.target.value)}
                      disabled={readOnly}
                      placeholder="Smoking / Non-smoking"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      label="Özel Oda İstekleri"
                      value={formData.roomSpecialRequests || ''}
                      onChange={(e) => handleChange('roomSpecialRequests', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: High floor, sea view, quiet room"
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Yemek Tercihleri */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <RestaurantIcon color="primary" />
                  <Typography variant="h6">Yemek Tercihleri</Typography>
                </Box>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Diyet Tercihleri"
                      value={formData.dietaryPreferences || ''}
                      onChange={(e) => handleChange('dietaryPreferences', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: Vegan, Vegetarian, Halal, Kosher"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Gıda Alerjileri"
                      value={formData.foodAllergies || ''}
                      onChange={(e) => handleChange('foodAllergies', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: Peanut, Dairy, Gluten"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      label="Özel Yemek İstekleri"
                      value={formData.specialFoodRequests || ''}
                      onChange={(e) => handleChange('specialFoodRequests', e.target.value)}
                      disabled={readOnly}
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Aktivite Tercihleri */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <SportsIcon color="primary" />
                  <Typography variant="h6">Aktivite Tercihleri</Typography>
                </Box>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Aktivite Tercihleri"
                      value={formData.activityPreferences || ''}
                      onChange={(e) => handleChange('activityPreferences', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: Spor, Kültür, Eğlence"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      label="İlgi Alanları"
                      value={formData.interests || ''}
                      onChange={(e) => handleChange('interests', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: Müze, Plaj, Gece hayatı, Spa"
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* İletişim Tercihleri */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <EmailIcon color="primary" />
                  <Typography variant="h6">İletişim Tercihleri</Typography>
                </Box>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={formData.prefersEmail}
                          onChange={(e) => handleChange('prefersEmail', e.target.checked)}
                          disabled={readOnly}
                        />
                      }
                      label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <EmailIcon fontSize="small" />
                          E-posta
                        </Box>
                      }
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={formData.prefersSMS}
                          onChange={(e) => handleChange('prefersSMS', e.target.checked)}
                          disabled={readOnly}
                        />
                      }
                      label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <SmsIcon fontSize="small" />
                          SMS
                        </Box>
                      }
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={formData.prefersWhatsApp}
                          onChange={(e) => handleChange('prefersWhatsApp', e.target.checked)}
                          disabled={readOnly}
                        />
                      }
                      label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <WhatsAppIcon fontSize="small" />
                          WhatsApp
                        </Box>
                      }
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={formData.prefersPhone}
                          onChange={(e) => handleChange('prefersPhone', e.target.checked)}
                          disabled={readOnly}
                        />
                      }
                      label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <PhoneIcon fontSize="small" />
                          Telefon
                        </Box>
                      }
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Tercih Edilen Dil"
                      value={formData.preferredLanguage || ''}
                      onChange={(e) => handleChange('preferredLanguage', e.target.value)}
                      disabled={readOnly}
                      placeholder="Örn: TR, EN, DE"
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Genel Notlar */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Genel Notlar
                </Typography>
                <TextField
                  fullWidth
                  multiline
                  rows={4}
                  label="Notlar"
                  value={formData.notes || ''}
                  onChange={(e) => handleChange('notes', e.target.value)}
                  disabled={readOnly}
                />
              </CardContent>
            </Card>
          </Grid>

          {!readOnly && (
            <Grid item xs={12}>
              <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                <Button
                  type="submit"
                  variant="contained"
                  color="primary"
                  disabled={upsertMutation.isPending}
                >
                  {preferences ? 'Güncelle' : 'Kaydet'}
                </Button>
              </Box>
            </Grid>
          )}
        </Grid>
      </form>
    </Box>
  )
}

export default GuestPreferences
