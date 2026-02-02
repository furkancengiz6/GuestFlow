import { create } from 'zustand';
import AsyncStorage from '@react-native-async-storage/async-storage';

interface SettingsState {
    isBiometricEnabled: boolean;
    isDarkMode: boolean;
    setBiometricEnabled: (enabled: boolean) => Promise<void>;
    setDarkMode: (enabled: boolean) => Promise<void>;
    loadSettings: () => Promise<void>;
}

export const useSettingsStore = create<SettingsState>((set) => ({
    isBiometricEnabled: false,
    isDarkMode: false,

    setBiometricEnabled: async (enabled) => {
        await AsyncStorage.setItem('settings_biometric', JSON.stringify(enabled));
        set({ isBiometricEnabled: enabled });
    },

    setDarkMode: async (enabled) => {
        await AsyncStorage.setItem('settings_darkmode', JSON.stringify(enabled));
        set({ isDarkMode: enabled });
    },

    loadSettings: async () => {
        try {
            const bio = await AsyncStorage.getItem('settings_biometric');
            const dark = await AsyncStorage.getItem('settings_darkmode');
            set({
                isBiometricEnabled: bio ? JSON.parse(bio) : false,
                isDarkMode: dark ? JSON.parse(dark) : false
            });
        } catch (error) {
            console.error('Failed to load settings:', error);
        }
    }
}));
