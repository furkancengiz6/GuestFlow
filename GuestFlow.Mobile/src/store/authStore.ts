import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';

interface User {
    id: number;
    email: string;
    fullName: string;
    role?: string;
}

interface AuthState {
    accessToken: string | null;
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (accessToken: string, user: User) => Promise<void>;
    logout: () => Promise<void>;
    initialize: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
    accessToken: null,
    user: null,
    isAuthenticated: false,
    isLoading: true,

    login: async (accessToken, user) => {
        await SecureStore.setItemAsync('accessToken', accessToken);
        await SecureStore.setItemAsync('user', JSON.stringify(user));
        set({ accessToken, user, isAuthenticated: true });
    },

    logout: async () => {
        await SecureStore.deleteItemAsync('accessToken');
        await SecureStore.deleteItemAsync('user');
        set({ accessToken: null, user: null, isAuthenticated: false });
    },

    initialize: async () => {
        try {
            const accessToken = await SecureStore.getItemAsync('accessToken');
            const userJson = await SecureStore.getItemAsync('user');
            const user = userJson ? JSON.parse(userJson) : null;

            if (accessToken && user) {
                set({ accessToken, user, isAuthenticated: true, isLoading: false });
            } else {
                set({ isLoading: false });
            }
        } catch (error) {
            console.error('Failed to initialize auth store:', error);
            set({ isLoading: false });
        }
    },
}));
