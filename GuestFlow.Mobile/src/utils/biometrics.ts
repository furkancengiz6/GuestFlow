// This utility would normally use expo-local-authentication
// For now, it provides a mock implementation until the library is confirmed
// import * as LocalAuthentication from 'expo-local-authentication';

export const biometrics = {
    isSupported: async (): Promise<boolean> => {
        // Mock implementation
        // return await LocalAuthentication.hasHardwareAsync() && await LocalAuthentication.isEnrolledAsync();
        return true;
    },

    authenticate: async (reason: string = 'Lütfen kimliğinizi doğrulayın'): Promise<boolean> => {
        console.log('Authenticating with biometrics for:', reason);
        // Mock success
        /*
        const result = await LocalAuthentication.authenticateAsync({
            promptMessage: reason,
            fallbackLabel: 'Şifre Kullan',
            disableDeviceFallback: false,
        });
        return result.success;
        */
        return new Promise((resolve) => {
            // Simulate a short delay
            setTimeout(() => resolve(true), 500);
        });
    }
};
