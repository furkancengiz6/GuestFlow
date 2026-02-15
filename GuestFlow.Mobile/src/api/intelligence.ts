import apiClient from './client';

export interface ProactiveRecommendation {
    recommendationType: string;
    title: string;
    description: string;
    priority: number;
    recommendedAction?: string;
    context?: any;
}

export interface ProblemPreventionAlert {
    title: string;
    description: string;
    priority: number;
    severity: string;
    suggestedAction?: string;
}

export const intelligenceService = {
    getProactiveRecommendations: async (guestId?: number): Promise<ProactiveRecommendation[]> => {
        const url = guestId
            ? `/Intelligence/guests/${guestId}/proactive-recommendations`
            : `/Intelligence/guests/0/proactive-recommendations`; // Assuming 0 or endpoint supports global
        const response = await apiClient.get(url);
        return response.data.data;
    },

    getProblemPreventionAlerts: async (): Promise<ProblemPreventionAlert[]> => {
        const response = await apiClient.get('/Intelligence/problem-prevention-alerts');
        return response.data.data;
    },

    getEarlyWarningSignals: async (): Promise<any[]> => {
        const response = await apiClient.get('/Intelligence/early-warning-signals');
        return response.data.data;
    }
};
