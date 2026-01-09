import axios from 'axios';

export async function syncSupplierCosts() {
  const res = await axios.post('/api/SupplierCosts/sync');
  return res.data;
}

export default {
  syncSupplierCosts
};

export async function getAllSupplierCosts() {
  const res = await axios.get('/api/SupplierCosts');
  return res.data;
}

export async function createSupplierCost(request: any) {
  const res = await axios.post('/api/SupplierCosts', request);
  return res.data;
}

export async function updateSupplierCost(id: number, request: any) {
  const res = await axios.put(`/api/SupplierCosts/${id}`, request);
  return res.data;
}

export async function deleteSupplierCost(id: number) {
  const res = await axios.delete(`/api/SupplierCosts/${id}`);
  return res.data;
}
