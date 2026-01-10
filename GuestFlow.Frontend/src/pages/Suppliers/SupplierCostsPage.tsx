import React from 'react';
import {
  Button,
  Container,
  Typography,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Checkbox,
  FormControlLabel
} from '@mui/material';
import supplierCostService, {
  getAllSupplierCosts,
  createSupplierCost,
  updateSupplierCost,
  deleteSupplierCost,
  syncSupplierCosts
} from '../../services/supplierCostService';

const SupplierCostsPage: React.FC = () => {
  const [loading, setLoading] = React.useState(false);
  const [message, setMessage] = React.useState<string | null>(null);
  const [items, setItems] = React.useState<any[]>([]);
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editingId, setEditingId] = React.useState<number | null>(null);
  const [form, setForm] = React.useState<any>({
    supplierId: 0,
    transferId: null,
    cityTourId: null,
    yachtTourId: null,
    costAmount: 0,
    currency: 'USD',
    costType: '',
    description: '',
    validFrom: '',
    validTo: '',
    isActive: true
  });

  const load = async () => {
    setLoading(true);
    try {
      const res = await getAllSupplierCosts();
      if (res && res.success) {
        setItems(res.data ?? []);
      } else {
        setMessage(res?.message ?? 'Failed to load supplier costs');
      }
    } catch (err: any) {
      setMessage(err?.message ?? 'Failed to load supplier costs');
    } finally {
      setLoading(false);
    }
  };

  React.useEffect(() => {
    load();
  }, []);

  const handleSync = async () => {
    setLoading(true);
    setMessage(null);
    try {
      const res = await syncSupplierCosts();
      setMessage(res?.message ?? 'Sync started');
      await load();
    } catch (err: any) {
      setMessage(err?.message ?? 'Sync failed');
    } finally {
      setLoading(false);
    }
  };

  const openCreate = () => {
    setEditingId(null);
    setForm({
      supplierId: 0,
      transferId: null,
      cityTourId: null,
      yachtTourId: null,
      costAmount: 0,
      currency: 'USD',
      costType: '',
      description: '',
      validFrom: '',
      validTo: '',
      isActive: true
    });
    setDialogOpen(true);
  };

  const openEdit = (item: any) => {
    setEditingId(item.id);
    setForm({
      supplierId: item.supplierId,
      transferId: item.transferId,
      cityTourId: item.cityTourId,
      yachtTourId: item.yachtTourId,
      costAmount: item.costAmount,
      currency: item.currency,
      costType: item.costType,
      description: item.description,
      validFrom: item.validFrom ? item.validFrom.split('T')[0] : '',
      validTo: item.validTo ? item.validTo.split('T')[0] : '',
      isActive: item.isActive
    });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    setLoading(true);
    try {
      if (editingId) {
        const res = await updateSupplierCost(editingId, form);
        setMessage(res?.message ?? 'Updated');
      } else {
        const res = await createSupplierCost(form);
        setMessage(res?.message ?? 'Created');
      }
      setDialogOpen(false);
      await load();
    } catch (err: any) {
      setMessage(err?.message ?? 'Save failed');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure to delete this supplier cost?')) return;
    setLoading(true);
    try {
      const res = await deleteSupplierCost(id);
      setMessage(res?.message ?? 'Deleted');
      await load();
    } catch (err: any) {
      setMessage(err?.message ?? 'Delete failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container>
      <Typography variant="h4" gutterBottom>
        Supplier Costs
      </Typography>

      <Button variant="contained" color="primary" onClick={handleSync} disabled={loading} sx={{ mr: 2 }}>
        {loading ? 'Synchronizing...' : 'Sync Supplier Costs'}
      </Button>
      <Button variant="outlined" onClick={openCreate} sx={{ mr: 2 }}>
        Create Supplier Cost
      </Button>

      {message && <Typography variant="body1" sx={{ mt: 2 }}>{message}</Typography>}

      <Table sx={{ mt: 3 }}>
        <TableHead>
          <TableRow>
            <TableCell>ID</TableCell>
            <TableCell>SupplierId</TableCell>
            <TableCell>TransferId</TableCell>
            <TableCell>Amount</TableCell>
            <TableCell>Currency</TableCell>
            <TableCell>Type</TableCell>
            <TableCell>ValidFrom</TableCell>
            <TableCell>Active</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {items.map((it: any) => (
            <TableRow key={it.id}>
              <TableCell>{it.id}</TableCell>
              <TableCell>{it.supplierId}</TableCell>
              <TableCell>{it.transferId ?? '-'}</TableCell>
              <TableCell>{it.costAmount}</TableCell>
              <TableCell>{it.currency}</TableCell>
              <TableCell>{it.costType}</TableCell>
              <TableCell>{it.validFrom ? it.validFrom.split('T')[0] : '-'}</TableCell>
              <TableCell>{it.isActive ? 'Yes' : 'No'}</TableCell>
              <TableCell>
                <IconButton size="small" onClick={() => openEdit(it)}>Edit</IconButton>
                <IconButton size="small" onClick={() => handleDelete(it.id)}>Delete</IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>{editingId ? 'Edit Supplier Cost' : 'Create Supplier Cost'}</DialogTitle>
        <DialogContent>
          <TextField margin="dense" label="SupplierId" fullWidth value={form.supplierId} onChange={(e) => setForm({ ...form, supplierId: Number(e.target.value) })} />
          <TextField margin="dense" label="TransferId" fullWidth value={form.transferId ?? ''} onChange={(e) => setForm({ ...form, transferId: e.target.value ? Number(e.target.value) : null })} />
          <TextField margin="dense" label="CostAmount" fullWidth value={form.costAmount} onChange={(e) => setForm({ ...form, costAmount: Number(e.target.value) })} />
          <TextField margin="dense" label="Currency" fullWidth value={form.currency} onChange={(e) => setForm({ ...form, currency: e.target.value })} />
          <TextField margin="dense" label="CostType" fullWidth value={form.costType} onChange={(e) => setForm({ ...form, costType: e.target.value })} />
          <TextField margin="dense" label="Description" fullWidth multiline rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          <TextField margin="dense" label="ValidFrom" type="date" fullWidth value={form.validFrom} onChange={(e) => setForm({ ...form, validFrom: e.target.value })} InputLabelProps={{ shrink: true }} />
          <FormControlLabel control={<Checkbox checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />} label="Is Active" />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">Save</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default SupplierCostsPage;

