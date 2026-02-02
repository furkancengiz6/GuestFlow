import React, { useEffect, useState } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Provider as PaperProvider, DefaultTheme, ActivityIndicator, Snackbar, Portal } from 'react-native-paper';
import { View } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useAuthStore } from './src/store/authStore';
import { signalRService } from './src/services/SignalRService';
import { registerForPushNotificationsAsync } from './src/utils/notifications';
import LoginScreen from './src/screens/LoginScreen';
import DashboardScreen from './src/screens/DashboardScreen';
import GuestListScreen from './src/screens/GuestListScreen';
import QRScannerScreen from './src/screens/QRScannerScreen';
import GuestDetailScreen from './src/screens/GuestDetailScreen';
import AIChatScreen from './src/screens/AIChatScreen';
import AddServiceScreen from './src/screens/AddServiceScreen';
import AddReviewScreen from './src/screens/AddReviewScreen';
import SettingsScreen from './src/screens/SettingsScreen';

const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

const theme = {
  ...DefaultTheme,
  colors: {
    ...DefaultTheme.colors,
    primary: '#1976d2',
    accent: '#9c27b0',
    background: '#f8f9fa',
  },
};

function MainTabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarIcon: ({ color, size }) => {
          let iconName: any;
          if (route.name === 'Dashboard') iconName = 'view-dashboard';
          else if (route.name === 'Guests') iconName = 'account-group';
          else if (route.name === 'AI') iconName = 'robot';
          else if (route.name === 'Settings') iconName = 'cog';
          return <MaterialCommunityIcons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: '#1976d2',
        tabBarInactiveTintColor: 'gray',
      })}
    >
      <Tab.Screen name="Dashboard" component={DashboardScreen} />
      <Tab.Screen name="Guests" component={GuestListScreen} options={{ title: 'Misafirler' }} />
      <Tab.Screen name="AI" component={AIChatScreen} options={{ title: 'Asistan' }} />
      <Tab.Screen name="Settings" component={SettingsScreen} options={{ title: 'Ayarlar' }} />
    </Tab.Navigator>
  );
}

export default function App() {
  const { isAuthenticated, isLoading, initialize } = useAuthStore();
  const [notification, setNotification] = useState<any>(null);

  useEffect(() => {
    initialize();
    registerForPushNotificationsAsync();
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      signalRService.start();

      const unsubscribe = signalRService.on('ReceiveNotification', (notif) => {
        setNotification(notif);
      });

      return () => {
        unsubscribe();
        signalRService.stop();
      };
    }
  }, [isAuthenticated]);

  if (isLoading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" color="#1976d2" />
      </View>
    );
  }

  return (
    <PaperProvider theme={theme}>
      <Portal>
        <NavigationContainer>
          <Stack.Navigator screenOptions={{ headerShown: false }}>
            {!isAuthenticated ? (
              <Stack.Screen name="Login" component={LoginScreen} />
            ) : (
              <>
                <Stack.Screen name="Main" component={MainTabNavigator} />
                <Stack.Screen name="GuestDetail" component={GuestDetailScreen} />
                <Stack.Screen name="QRScanner" component={QRScannerScreen} />
                <Stack.Screen name="AddService" component={AddServiceScreen} />
                <Stack.Screen name="AddReview" component={AddReviewScreen} />
              </>
            )}
          </Stack.Navigator>
        </NavigationContainer>

        <Snackbar
          visible={notification !== null}
          onDismiss={() => setNotification(null)}
          duration={5000}
          action={{
            label: 'Tamam',
            onPress: () => setNotification(null),
          }}
          style={{ backgroundColor: '#1976d2' }}
        >
          {notification?.title}: {notification?.content}
        </Snackbar>
      </Portal>
    </PaperProvider>
  );
}
