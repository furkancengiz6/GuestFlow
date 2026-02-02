import api from '../../api/client';

export interface OccupancyForecast {
    date: string;
    forecastedOccupancyRate: number;
    forecastedBookedRooms: number;
    confidenceIntervalLower: number;
    confidenceIntervalUpper: number;
}

export interface RevenueForecast {
    date: string;
    forecastedRevenue: number;
    forecastedRevPAR: number;
    currency: string;
}

export const analyticsService = {
    getOccupancyForecast: async (startDate: Date, endDate: Date): Promise<OccupancyForecast[]> => {
        const response = await api.get('/analytics/predict/occupancy', {
            params: {
                startDate: startDate.toISOString(),
                endDate: endDate.toISOString(),
            },
        });
        return response.data;
    },

    getRevenueForecast: async (startDate: Date, endDate: Date): Promise<RevenueForecast[]> => {
        const response = await api.get('/analytics/predict/revenue', {
            params: {
                startDate: startDate.toISOString(),
                endDate: endDate.toISOString(),
            },
        });
        return response.data;
    },
};
