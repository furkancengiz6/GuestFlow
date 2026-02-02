import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';
import { Platform } from 'react-native';

const BASE_URL = 'http://localhost:5146'; // Update this for real device/emulator

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private handlers: Map<string, ((data: any) => void)[]> = new Map();

    async start() {
        if (this.connection) return;

        const token = useAuthStore.getState().accessToken;
        if (!token) return;

        const hubUrl = `${BASE_URL}/hubs/notifications`;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => token,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.on('ReceiveNotification', (notification: any) => {
            console.log('Notification received:', notification);
            this.notifyHandlers('ReceiveNotification', notification);
        });

        this.connection.on('ReceiveLiveUpdate', (update: any) => {
            console.log('Live update received:', update);
            this.notifyHandlers('ReceiveLiveUpdate', update);
        });

        try {
            await this.connection.start();
            console.log('SignalR connected');
        } catch (err) {
            console.error('SignalR connection error:', err);
            setTimeout(() => this.start(), 5000);
        }
    }

    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    on(event: string, handler: (data: any) => void) {
        if (!this.handlers.has(event)) {
            this.handlers.set(event, []);
        }
        this.handlers.get(event)?.push(handler);

        return () => {
            const list = this.handlers.get(event);
            if (list) {
                this.handlers.set(event, list.filter(h => h !== handler));
            }
        };
    }

    private notifyHandlers(event: string, data: any) {
        this.handlers.get(event)?.forEach(handler => handler(data));
    }
}

export const signalRService = new SignalRService();
