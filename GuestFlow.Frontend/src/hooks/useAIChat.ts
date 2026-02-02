import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../stores/authStore';
import { env } from '../config/env';
import { ChatMessage, AIChatRequest, AIChatResponse } from '../types/ai';

export const useAIChat = () => {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [isProcessing, setIsProcessing] = useState(false);
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const { accessToken, user } = useAuthStore();

    const baseUrl = env.apiBaseUrl.replace('/api/v1.0', '');
    const hubUrl = `${baseUrl}/hubs/ai-chat`;

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

        connection.on('ReceiveAIResponse', (response: AIChatResponse) => {
            const aiMessage: ChatMessage = {
                id: Date.now().toString(),
                text: response.response,
                sender: 'ai',
                timestamp: new Date(),
                actions: response.suggestedActions
            };
            setMessages(prev => [...prev, aiMessage]);
        });

        connection.on('AIProcessing', (processing: boolean) => {
            setIsProcessing(processing);
        });

        const startConnection = async () => {
            try {
                await connection.start();
                setIsConnected(true);
                console.log('AI Chat SignalR Connected');
            } catch (err) {
                console.error('AI Chat SignalR Connection Error: ', err);
                setTimeout(startConnection, 5000);
            }
        };

        startConnection();
        connectionRef.current = connection;

        return () => {
            connection.stop();
        };
    }, [accessToken, hubUrl]);

    const sendMessage = useCallback(async (text: string) => {
        if (!connectionRef.current || !isConnected) return;

        const userMessage: ChatMessage = {
            id: Date.now().toString(),
            text,
            sender: 'user',
            timestamp: new Date()
        };
        setMessages(prev => [...prev, userMessage]);

        const request: AIChatRequest = {
            message: text,
            guestId: user?.id,
            metadata: { source: 'web-client' }
        };

        try {
            await connectionRef.current.invoke('SendMessage', request);
        } catch (err) {
            console.error('Error sending AI message: ', err);
        }
    }, [isConnected, user?.id]);

    const clearMessages = useCallback(() => {
        setMessages([]);
    }, []);

    return {
        messages,
        isProcessing,
        isConnected,
        sendMessage,
        clearMessages
    };
};
