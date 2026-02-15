import apiClient from './client';
import { Guest, GuestListResponse } from '../types/guest';

export const guestService = {
    getGuests: async (page = 1, pageSize = 20, search = '') => {
        const response = await apiClient.get('/Guests', {
            params: { page, pageSize, search }
        });
        return response.data.data as GuestListResponse;
    },

    getGuestById: async (id: number) => {
        const response = await apiClient.get(`/Guests/${id}`);
        return response.data.data as Guest;
    },

    validateGuestQr: async (code: string) => {
        const response = await apiClient.get(`/GuestQr/validate/${code}`);
        return response.data.data as { guest: Guest, aiSuggestion: string };
    }
};
