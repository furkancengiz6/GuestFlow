import apiClient from './client';
import { reviewService } from './reviews';
import {
    CheckInOutItem,
    ActiveGuest,
    UpcomingServiceItem,
    DashboardSummary
} from '../types/operations';

export const dashboardService = {
    getTodayCheckIns: async () => {
        const response = await apiClient.get('/ConciergeDashboard/today-checkins');
        return response.data.data;
    },

    getTodayCheckOuts: async () => {
        const response = await apiClient.get('/ConciergeDashboard/today-checkouts');
        return response.data.data;
    },

    getActiveGuests: async () => {
        const response = await apiClient.get('/ConciergeDashboard/active-guests');
        return response.data.data;
    },

    getUpcomingServices: async () => {
        const response = await apiClient.get('/ConciergeDashboard/upcoming-services');
        return response.data.data;
    },

    getSummary: async () => {
        const [checkIns, checkOuts, active, services, avgRating] = await Promise.all([
            dashboardService.getTodayCheckIns(),
            dashboardService.getTodayCheckOuts(),
            dashboardService.getActiveGuests(),
            dashboardService.getUpcomingServices(),
            reviewService.getAverageRating()
        ]);

        return {
            todayCheckIns: checkIns.totalCount || 0,
            todayCheckOuts: checkOuts.totalCount || 0,
            activeGuestsCount: active.length || 0,
            pendingServicesCount: services.totalCount || 0,
            averageRating: avgRating || 0,
        } as DashboardSummary;
    }
};
