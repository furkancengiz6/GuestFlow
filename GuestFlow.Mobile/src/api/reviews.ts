import apiClient from './client';

export interface CreateGuestReviewDto {
    guestId: number;
    reservationId?: number;
    serviceId?: number;
    serviceType?: string;
    rating: number;
    comment: string;
    cleanlinessRating?: number;
    serviceQualityRating?: number;
    staffRating?: number;
}

export const reviewService = {
    createReview: async (dto: CreateGuestReviewDto) => {
        const response = await apiClient.post('/GuestReviews', dto);
        return response.data;
    },
    getAverageRating: async (serviceType?: string) => {
        const response = await apiClient.get('/GuestReviews/average', {
            params: { serviceType }
        });
        return response.data.data as number;
    }
};
