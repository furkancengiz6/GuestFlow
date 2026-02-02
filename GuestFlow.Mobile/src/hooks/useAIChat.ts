import { useState, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';

export interface ChatMessage {
    id: string;
    text: string;
    sender: 'user' | 'ai';
    timestamp: Date;
    actions?: AIAction[];
}

export interface AIAction {
    type: string;
    description: string;
    payload?: any;
}

export const useAIChat = () => {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [isProcessing, setIsProcessing] = useState(false);
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const { accessToken } = useAuthStore();

    const hubUrl = 'http://localhost:5146/hubs/ai-chat'; // Dev default

    useEffect(() => {
        if (!accessToken) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => accessToken,
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveMessage', (message: string, actionsJson?: string) => {
            setIsProcessing(false);
            const actions = actionsJson ? JSON.parse(actionsJson) : [];

            const newMessage: ChatMessage = {
                id: Math.random().toString(36).substring(7),
                text: message,
                sender: 'ai',
                timestamp: new Date(),
                actions
            };
            setMessages(prev => [...prev, newMessage]);
        });

        connection.on('SetProcessing', (processing: boolean) => {
            setIsProcessing(processing);
        });

        const startConnection = async () => {
            try {
                await connection.start();
                setIsConnected(true);
                console.log('Mobile AI Chat Connected');
            } catch (err) {
                console.error('Mobile AI Chat Connection Error: ', err);
                setTimeout(startConnection, 5000);
            }
        };

        startConnection();
        connectionRef.current = connection;

        return () => {
            connection.stop();
        };
    }, [accessToken]);

    const sendMessage = async (text: string) => {
        if (!connectionRef.current || !isConnected) return;

        const userMsg: ChatMessage = {
            id: Math.random().toString(36).substring(7),
            text,
            sender: 'user',
            timestamp: new Date()
        };

        setMessages(prev => [...prev, userMsg]);
        setIsProcessing(true);

        try {
            await connectionRef.current.invoke('SendMessage', text);
        } catch (err) {
            console.error('Failed to send AI message:', err);
            setIsProcessing(false);
        }
    };

    return {
        messages,
        isProcessing,
        isConnected,
        sendMessage,
        clearMessages: () => setMessages([])
    };
};
