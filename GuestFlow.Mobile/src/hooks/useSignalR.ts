import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';

export const useSignalR = () => {
    const [isConnected, setIsConnected] = useState(false);
    const [notifications, setNotifications] = useState<any[]>([]);
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const { accessToken } = useAuthStore();

    const hubUrl = 'http://localhost:5146/hubs/notifications'; // Dev default

    useEffect(() => {
        if (!accessToken) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => accessToken,
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on('ReceiveNotification', (notification: any) => {
            console.log('New notification received:', notification);
            setNotifications(prev => [notification, ...prev]);
        });

        const startConnection = async () => {
            try {
                await connection.start();
                setIsConnected(true);
                console.log('Mobile SignalR Connected');
            } catch (err) {
                console.error('Mobile SignalR Connection Error: ', err);
                setTimeout(startConnection, 5000);
            }
        };

        startConnection();
        connectionRef.current = connection;

        return () => {
            connection.stop();
        };
    }, [accessToken]);

    return {
        isConnected,
        notifications,
        clearNotifications: () => setNotifications([]),
    };
};
