import { useEffect, useState } from 'react'
import { useForm, Controller, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  IconButton,
  Paper,
  Typography,
} from '@mui/material'
import { Add as AddIcon, Delete as DeleteIcon } from '@mui/icons-material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useQuery } from '@tanstack/react-query'
import { Itinerary, CreateItineraryRequest, UpdateItineraryRequest } from '../../services/itineraryService'
import { guestService } from '../../services/guestService'
import { personnelService } from '../../services/personnelService'

// Zod schema
const itineraryItemSchema = z.object({
  itemType: z.string().min(1, 'Öğe tipi seçiniz'),
  serviceId: z.number().min(1, 'Servis ID gerekli'),
  scheduledDateTime: z.string().min(1, 'Tarih ve saat gerekli'),
  order: z.number().min(1),
  notes: z.string().optional().or(z.literal('')),
})

const itinerarySchema = z.object({
  guestId: z.number().min(1, 'Misafir seçiniz'),
  personnelId: z.number().min(1, 'Personel seçiniz'),
  startDate: z.string().min(1, 'Başlangıç tarihi gerekli'),
  endDate: z.string().min(1, 'Bitiş tarihi gerekli'),
  notes: z.string().optional().or(z.literal('')),
  currency: z.string().optional().default('TRY'),
  items: z.array(itineraryItemSchema).min(1, 'En az bir öğe ekleyiniz'),
})

type ItineraryFormData = z.infer<typeof itinerarySchema>

interface ItineraryFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateItineraryRequest | UpdateItineraryRequest) => Promise<void>
  itinerary?: Itinerary | null
  isLoading?: boolean
}

const ItineraryForm = ({ open, onClose, onSubmit, itinerary, isLoading = false }: ItineraryFormProps) => {
  const [startDate, setStartDate] = useState<Date | null>(null)
  const [endDate, setEndDate] = useState<Date | null>(null)

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<ItineraryFormData>({
    resolver: zodResolver(itinerarySchema) as any,
    defaultValues: {
      guestId: 0,
      personnelId: 0,
      startDate: '',
      endDate: '',
      notes: '',
      currency: 'TRY' as string | undefined,
      items: [{ itemType: '', serviceId: 0, scheduledDateTime: '', order: 1, notes: '' }],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'items',
  })

  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => guestService.getGuests(1, 1000),
  })

  const { data: personnel } = useQuery({
    queryKey: ['personnel-dropdown'],
    queryFn: () => personnelService.getPersonnel(1, 1000),
  })

  // Form'u itinerary verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (itinerary) {
      setValue('guestId', itinerary.guestId)
      setValue('personnelId', itinerary.personnelId)
      setValue('startDate', itinerary.startDate)
      setValue('endDate', itinerary.endDate)
      setValue('notes', itinerary.notes || '')
      setValue('currency', itinerary.currency)
      setStartDate(new Date(itinerary.startDate))
      setEndDate(new Date(itinerary.endDate))
      if (itinerary.items && itinerary.items.length > 0) {
        setValue('items', itinerary.items.map((item, index) => ({
          itemType: item.itemType,
          serviceId: item.serviceId,
          scheduledDateTime: item.scheduledDateTime,
          order: item.order || index + 1,
          notes: item.notes || '',
        })))
      }
    } else {
      reset()
      setStartDate(null)
      setEndDate(null)
    }
  }, [itinerary, setValue, reset])

  const onSubmitForm = async (data: ItineraryFormData) => {
    const submitData: CreateItineraryRequest | UpdateItineraryRequest = {
      ...data,
      currency: data.currency || 'TRY',
    } as CreateItineraryRequest | UpdateItineraryRequest
    await onSubmit(submitData)
    if (!itinerary) {
      reset()
      setStartDate(null)
      setEndDate(null)
    }
  }

  const addItem = () => {
    append({ itemType: '', serviceId: 0, scheduledDateTime: '', order: fields.length + 1, notes: '' })
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <form onSubmit={handleSubmit(onSubmitForm)}>
        <DialogTitle>{itinerary ? 'İtinerary Düzenle' : 'Yeni İtinerary'}</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth error={!!errors.guestId}>
                  <InputLabel>Misafir *</InputLabel>
                  <Controller
                    name="guestId"
                    control={control}
                    render={({ field }) => (
                      <Select {...field} label="Misafir *">
                        <MenuItem value={0}>Misafir Seçiniz</MenuItem>
                        {guests?.data?.map((guest) => (
                          <MenuItem key={guest.id} value={guest.id}>
                            {guest.fullName}
                          </MenuItem>
                        ))}
                      </Select>
                    )}
                  />
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth error={!!errors.personnelId}>
                  <InputLabel>Personel *</InputLabel>
                  <Controller
                    name="personnelId"
                    control={control}
                    render={({ field }) => (
                      <Select {...field} label="Personel *">
                        <MenuItem value={0}>Personel Seçiniz</MenuItem>
                        {personnel?.data?.map((p) => (
                          <MenuItem key={p.id} value={p.id}>
                            {p.fullName}
                          </MenuItem>
                        ))}
                      </Select>
                    )}
                  />
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
                  <DatePicker
                    label="Başlangıç Tarihi *"
                    value={startDate}
                    onChange={(date) => {
                      setStartDate(date)
                      setValue('startDate', date ? date.toISOString() : '')
                    }}
                    slotProps={{
                      textField: {
                        fullWidth: true,
                        error: !!errors.startDate,
                        helperText: errors.startDate?.message,
                      },
                    }}
                  />
                </LocalizationProvider>
              </Grid>
              <Grid item xs={12} md={6}>
                <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
                  <DatePicker
                    label="Bitiş Tarihi *"
                    value={endDate}
                    onChange={(date) => {
                      setEndDate(date)
                      setValue('endDate', date ? date.toISOString() : '')
                    }}
                    slotProps={{
                      textField: {
                        fullWidth: true,
                        error: !!errors.endDate,
                        helperText: errors.endDate?.message,
                      },
                    }}
                  />
                </LocalizationProvider>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Notlar"
                  {...register('notes')}
                  multiline
                  rows={3}
                />
              </Grid>
              <Grid item xs={12}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
                  <Typography variant="h6">İtinerary Öğeleri</Typography>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<AddIcon />}
                    onClick={addItem}
                  >
                    Öğe Ekle
                  </Button>
                </Box>
                {fields.map((field, index) => (
                  <Paper key={field.id} sx={{ p: 2, mb: 2 }}>
                    <Grid container spacing={2} alignItems="center">
                      <Grid item xs={12} md={3}>
                        <FormControl fullWidth size="small">
                          <InputLabel>Öğe Tipi</InputLabel>
                          <Controller
                            name={`items.${index}.itemType`}
                            control={control}
                            render={({ field }) => (
                              <Select {...field} label="Öğe Tipi">
                                <MenuItem value="Transfer">Transfer</MenuItem>
                                <MenuItem value="CityTour">Şehir Turu</MenuItem>
                                <MenuItem value="YachtTour">Yat Turu</MenuItem>
                                <MenuItem value="RestaurantReservation">Restoran Rezervasyonu</MenuItem>
                              </Select>
                            )}
                          />
                        </FormControl>
                      </Grid>
                      <Grid item xs={12} md={2}>
                        <TextField
                          fullWidth
                          size="small"
                          label="Servis ID"
                          type="number"
                          {...register(`items.${index}.serviceId`, { valueAsNumber: true })}
                        />
                      </Grid>
                      <Grid item xs={12} md={3}>
                        <TextField
                          fullWidth
                          size="small"
                          label="Tarih ve Saat"
                          type="datetime-local"
                          {...register(`items.${index}.scheduledDateTime`)}
                          InputLabelProps={{ shrink: true }}
                        />
                      </Grid>
                      <Grid item xs={12} md={2}>
                        <TextField
                          fullWidth
                          size="small"
                          label="Sıra"
                          type="number"
                          {...register(`items.${index}.order`, { valueAsNumber: true })}
                        />
                      </Grid>
                      <Grid item xs={12} md={2}>
                        <IconButton
                          color="error"
                          onClick={() => remove(index)}
                          disabled={fields.length === 1}
                        >
                          <DeleteIcon />
                        </IconButton>
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          size="small"
                          label="Notlar"
                          {...register(`items.${index}.notes`)}
                          multiline
                          rows={2}
                        />
                      </Grid>
                    </Grid>
                  </Paper>
                ))}
                {errors.items && (
                  <Typography variant="body2" color="error" sx={{ mt: 1 }}>
                    {errors.items.message}
                  </Typography>
                )}
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>İptal</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : itinerary ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default ItineraryForm

