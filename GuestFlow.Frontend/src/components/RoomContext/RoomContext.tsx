import React, { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  Grid,
  TextField,
  Button,
  Box,
  Card,
  CardContent,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Alert,
  CircularProgress
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { useQuery } from '@tanstack/react-query';
import { roomService } from '../../services/roomService';
import { RoomContext as RoomContextType } from '../../types/room';

const RoomContext: React.FC = () => {
  const [roomNumber, setRoomNumber] = useState('');
  const [startDate, setStartDate] = useState<Date | null>(new Date());
  const [endDate, setEndDate] = useState<Date | null>(new Date());
  const [hotelId, setHotelId] = useState<number | undefined>();

  const { data: roomContext, isLoading, error, refetch } = useQuery({
    queryKey: ['roomContext', roomNumber, startDate, endDate, hotelId],
    queryFn: () => roomService.getRoomContext({
      roomNumber,
      startDate: startDate?.toISOString() || '',
      endDate: endDate?.toISOString() || '',
      hotelId
    }),
    enabled: false // Only run when button is clicked
  });

  const handleSearch = () => {
    if (!roomNumber || !startDate || !endDate) {
      alert('Please fill in all required fields');
      return;
    }
    refetch();
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" gutterBottom>
          Room Context Lookup
        </Typography>

        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Search Parameters
          </Typography>

          <Grid container spacing={3} alignItems="center">
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Room Number"
                value={roomNumber}
                onChange={(e) => setRoomNumber(e.target.value)}
                required
              />
            </Grid>

            <Grid item xs={12} md={2}>
            <DatePicker
                label="Start Date"
                value={startDate}
                onChange={setStartDate}
                slotProps={{ textField: { fullWidth: true, required: true } as any }}
              />
            </Grid>

            <Grid item xs={12} md={2}>
              <DatePicker
                label="End Date"
                value={endDate}
                onChange={setEndDate}
                slotProps={{ textField: { fullWidth: true, required: true } as any }}
              />
            </Grid>

            <Grid item xs={12} md={2}>
              <TextField
                fullWidth
                label="Hotel ID (Optional)"
                type="number"
                value={hotelId || ''}
                onChange={(e) => setHotelId(e.target.value ? parseInt(e.target.value) : undefined)}
              />
            </Grid>

            <Grid item xs={12} md={3}>
              <Button
                variant="contained"
                onClick={handleSearch}
                disabled={!roomNumber || !startDate || !endDate}
                fullWidth
              >
                Search Room Context
              </Button>
            </Grid>
          </Grid>
        </Paper>

        {isLoading && (
          <Box display="flex" justifyContent="center" my={4}>
            <CircularProgress />
          </Box>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            Error loading room context: {(error as Error).message}
          </Alert>
        )}

        {roomContext && (
          <>
            {/* Room Summary */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" gutterBottom>
                Room Summary
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} md={3}>
                  <Typography variant="body2" color="text.secondary">Room Number</Typography>
                  <Typography variant="h6">{roomContext.roomNumber}</Typography>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Typography variant="body2" color="text.secondary">Hotel</Typography>
                  <Typography variant="h6">{roomContext.hotelName}</Typography>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Typography variant="body2" color="text.secondary">Date Range</Typography>
                  <Typography variant="body1">
                    {new Date(roomContext.searchStartDate).toLocaleDateString()} - {new Date(roomContext.searchEndDate).toLocaleDateString()}
                  </Typography>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Typography variant="body2" color="text.secondary">Guests Assigned</Typography>
                  <Typography variant="h6">{roomContext.guests.length}</Typography>
                </Grid>
              </Grid>
            </Paper>

            {/* Guests */}
            {roomContext.guests.length > 0 && (
              <Paper sx={{ p: 3, mb: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Guest Assignments
                </Typography>
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Guest Name</TableCell>
                        <TableCell>Guest Code</TableCell>
                        <TableCell>Assignment Period</TableCell>
                        <TableCell>Notes</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {roomContext.guests.map((guest, index) => (
                        <TableRow key={index}>
                          <TableCell>{guest.guestName}</TableCell>
                          <TableCell>{guest.guestCode}</TableCell>
                          <TableCell>
                            {new Date(guest.assignmentStart).toLocaleDateString()}
                            {guest.assignmentEnd && ` - ${new Date(guest.assignmentEnd).toLocaleDateString()}`}
                          </TableCell>
                          <TableCell>{guest.notes || '-'}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Paper>
            )}

            {/* Services */}
            {(roomContext.transfers.length > 0 || roomContext.cityTours.length > 0 || roomContext.yachtTours.length > 0) && (
              <Paper sx={{ p: 3, mb: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Services Provided
                </Typography>

                {roomContext.transfers.length > 0 && (
                  <Box mb={2}>
                    <Typography variant="subtitle1" gutterBottom>Transfers</Typography>
                    <TableContainer>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Date</TableCell>
                            <TableCell>Route</TableCell>
                            <TableCell>Guest</TableCell>
                            <TableCell align="right">Amount</TableCell>
                            <TableCell>Status</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {roomContext.transfers.map((transfer, index) => (
                            <TableRow key={index}>
                              <TableCell>{new Date(transfer.serviceDate).toLocaleDateString()}</TableCell>
                              <TableCell>{transfer.description}</TableCell>
                              <TableCell>{transfer.guestName}</TableCell>
                              <TableCell align="right">{transfer.amount} {transfer.currency}</TableCell>
                              <TableCell>
                                <Chip label={transfer.status} size="small" />
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </Box>
                )}

                {roomContext.cityTours.length > 0 && (
                  <Box mb={2}>
                    <Typography variant="subtitle1" gutterBottom>City Tours</Typography>
                    <TableContainer>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Date</TableCell>
                            <TableCell>Description</TableCell>
                            <TableCell>Guest</TableCell>
                            <TableCell align="right">Amount</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {roomContext.cityTours.map((tour, index) => (
                            <TableRow key={index}>
                              <TableCell>{new Date(tour.serviceDate).toLocaleDateString()}</TableCell>
                              <TableCell>{tour.description}</TableCell>
                              <TableCell>{tour.guestName}</TableCell>
                              <TableCell align="right">{tour.amount} {tour.currency}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </Box>
                )}

                {roomContext.yachtTours.length > 0 && (
                  <Box>
                    <Typography variant="subtitle1" gutterBottom>Yacht Tours</Typography>
                    <TableContainer>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Date</TableCell>
                            <TableCell>Description</TableCell>
                            <TableCell>Guest</TableCell>
                            <TableCell align="right">Amount</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {roomContext.yachtTours.map((tour, index) => (
                            <TableRow key={index}>
                              <TableCell>{new Date(tour.serviceDate).toLocaleDateString()}</TableCell>
                              <TableCell>{tour.description}</TableCell>
                              <TableCell>{tour.guestName}</TableCell>
                              <TableCell align="right">{tour.amount} {tour.currency}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </Box>
                )}
              </Paper>
            )}

            {/* Financial Summary */}
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Financial Summary
              </Typography>
              <Grid container spacing={3}>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {roomContext.financialSummary.totalInvoices}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Total Invoices
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {roomContext.financialSummary.totalPayments}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Total Payments
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" color="success.main">
                        {roomContext.financialSummary.totalInvoicedAmount.toFixed(2)} {roomContext.financialSummary.currency}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Total Invoiced
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography variant="h6" color="success.main">
                        {roomContext.financialSummary.totalPaidAmount.toFixed(2)} {roomContext.financialSummary.currency}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Total Paid
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Paper>
          </>
        )}
      </Container>
    </LocalizationProvider>
  );
};

export default RoomContext;
