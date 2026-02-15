import apiClient from './api'

export const pmsService = {
    syncGuest: async (integrationId: number, pmsGuestId: string): Promise<boolean> => {
        const response = await apiClient.post(`/PMS/integrations/${integrationId}/sync/guests/${pmsGuestId}`)
        return response.data.success
    }
}
