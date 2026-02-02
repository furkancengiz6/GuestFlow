import AsyncStorage from '@react-native-async-storage/async-storage';

export const cacheService = {
    save: async (key: string, data: any) => {
        try {
            const jsonValue = JSON.stringify({
                data,
                timestamp: Date.now()
            });
            await AsyncStorage.setItem(key, jsonValue);
        } catch (e) {
            console.error('Cache save error:', e);
        }
    },

    get: async <T>(key: string): Promise<T | null> => {
        try {
            const jsonValue = await AsyncStorage.getItem(key);
            if (jsonValue != null) {
                const parsed = JSON.parse(jsonValue);
                return parsed.data as T;
            }
            return null;
        } catch (e) {
            console.error('Cache get error:', e);
            return null;
        }
    },

    clear: async (key: string) => {
        try {
            await AsyncStorage.removeItem(key);
        } catch (e) {
            console.error('Cache clear error:', e);
        }
    },

    isStale: async (key: string, minutes: number = 30): Promise<boolean> => {
        const jsonValue = await AsyncStorage.getItem(key);
        if (!jsonValue) return true;
        const parsed = JSON.parse(jsonValue);
        const age = (Date.now() - parsed.timestamp) / 1000 / 60;
        return age > minutes;
    }
};
