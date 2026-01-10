import React, { useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  FormControl,
  FormControlLabel,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
  Alert,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
} from '@mui/material';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { invoiceService } from '../../services/invoiceService';
import { guestService } from '../../services/guestService';

// Invoice creation schema
const invoiceSchema = z.object({
  guestId: z.number().min(1, 'Misafir seçimi zorunludur'),
  currency: z.string().min(1, 'Para birimi zorunludur'),
  notes: z.string().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  selectedServiceIds: z.array(z.number()).optional(),
});

type InvoiceFormData = z.infer<typeof invoiceSchema>;

interface EligibleService {
  serviceType: string;
  serviceId: number;
  serviceDescription: string;
  serviceDate: string;
  amount: number;
  currency: string;
  isAlreadyInvoiced: boolean;
}

export const InvoiceCreator: React.FC = () => {
  const [selectedServices, setSelectedServices] = useState<number[]>([]);
  const [step, setStep] = useState<'select-guest' | 'select-services' | 'create'>('select-guest');

  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<InvoiceFormData>({
    resolver: zodResolver(invoiceSchema),
    defaultValues: {
      currency: 'TRY',
      selectedServiceIds: [],
    },
  });

  const guestId = watch('guestId');
  const startDate = watch('startDate');
  const endDate = watch('endDate');

  // Fetch guests
  const { data: guests } = useQuery({
    queryKey: ['guests'],
    queryFn: () => guestService.getGuests(),
  });

  // Fetch eligible services when guest is selected
  const { data: eligibleServices, refetch: refetchEligibleServices } = useQuery({
    queryKey: ['eligible-services', guestId, startDate, endDate],
    queryFn: () => {
      if (!guestId) return [];
      return invoiceService.getEligibleServices({
        guestId,
        startDate: startDate ? new Date(startDate) : undefined,
        endDate: endDate ? new Date(endDate) : undefined,
      });
    },
    enabled: !!guestId,
  });

  // Create invoice mutation
  const createInvoiceMutation = useMutation({
    mutationFn: (data: InvoiceFormData) => invoiceService.createInvoice(data),
    onSuccess: () => {
      alert('Invoice created successfully!');
      // Reset form
      setSelectedServices([]);
      setStep('select-guest');
    },
    onError: (error) => {
      alert(`Failed to create invoice: ${error.message}`);
    },
  });

  const handleServiceToggle = (serviceId: number) => {
    setSelectedServices(prev =>
      prev.includes(serviceId)
        ? prev.filter(id => id !== serviceId)
        : [...prev, serviceId]
    );
  };

  const handleNext = () => {
    if (step === 'select-guest' && guestId) {
      setStep('select-services');
      refetchEligibleServices();
    } else if (step === 'select-services') {
      setValue('selectedServiceIds', selectedServices);
      setStep('create');
    }
  };

  const onSubmit = (data: InvoiceFormData) => {
    data.selectedServiceIds = selectedServices;
    createInvoiceMutation.mutate(data);
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h5" gutterBottom>
          Create Invoice
        </Typography>

        {step === 'select-guest' && (
          <>
            <Typography variant="h6" gutterBottom>Select Guest</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Guest</InputLabel>
                  <Select
                    {...register('guestId', { valueAsNumber: true })}
                    error={!!errors.guestId}
                  >
                    {guests?.data?.map((guest: any) => (
                      <MenuItem key={guest.id} value={guest.id}>
                        {guest.fullName} ({guest.guestCode})
                      </MenuItem>
                    ))}
                  </Select>
                  {errors.guestId && <Alert severity="error">{errors.guestId.message}</Alert>}
                </FormControl>
              </Grid>
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  label="Start Date"
                  type="date"
                  {...register('startDate')}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  label="End Date"
                  type="date"
                  {...register('endDate')}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
            </Grid>
          </>
        )}

        {step === 'select-services' && (
          <>
            <Typography variant="h6" gutterBottom>Select Services</Typography>
            {eligibleServices && eligibleServices.length > 0 ? (
              <List>
                {eligibleServices.map((service: EligibleService) => (
                  <ListItem key={`${service.serviceType}-${service.serviceId}`}>
                    <ListItemText
                      primary={`${service.serviceType}: ${service.serviceDescription}`}
                      secondary={`Date: ${new Date(service.serviceDate).toLocaleDateString()} | Amount: ${service.amount} ${service.currency}`}
                    />
                    <ListItemSecondaryAction>
                      <Checkbox
                        checked={selectedServices.includes(service.serviceId)}
                        onChange={() => handleServiceToggle(service.serviceId)}
                        disabled={service.isAlreadyInvoiced}
                      />
                    </ListItemSecondaryAction>
                  </ListItem>
                ))}
              </List>
            ) : (
              <Alert severity="info">No eligible services found for the selected criteria.</Alert>
            )}
          </>
        )}

        {step === 'create' && (
          <>
            <Typography variant="h6" gutterBottom>Invoice Details</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Currency"
                  {...register('currency')}
                  error={!!errors.currency}
                  helperText={errors.currency?.message}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Notes"
                  multiline
                  rows={3}
                  {...register('notes')}
                />
              </Grid>
            </Grid>

            <Divider sx={{ my: 2 }} />
            <Typography variant="h6">Selected Services</Typography>
            {selectedServices.map(serviceId => {
              const service = eligibleServices?.find((s: EligibleService) => s.serviceId === serviceId);
              return service ? (
                <Chip
                  key={serviceId}
                  label={`${service.serviceType}: ${service.serviceDescription} (${service.amount} ${service.currency})`}
                  sx={{ m: 0.5 }}
                />
              ) : null;
            })}
          </>
        )}

        <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
          {step !== 'select-guest' && (
            <Button
              variant="outlined"
              onClick={() => setStep(step === 'create' ? 'select-services' : 'select-guest')}
            >
              Back
            </Button>
          )}

          {step !== 'create' && (
            <Button
              variant="contained"
              onClick={handleNext}
              disabled={
                (step === 'select-guest' && !guestId) ||
                (step === 'select-services' && selectedServices.length === 0)
              }
            >
              Next
            </Button>
          )}

          {step === 'create' && (
            <Button
              variant="contained"
              onClick={handleSubmit(onSubmit)}
              disabled={createInvoiceMutation.isPending}
            >
              {createInvoiceMutation.isPending ? 'Creating...' : 'Create Invoice'}
            </Button>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};
