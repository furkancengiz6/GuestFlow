import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '../stores/authStore'
import { env } from '../config/env'

const API_BASE_URL = env.apiBaseUrl.replace('/api/v1.0', '') || 'http://localhost:5146'
const HUB_URL = `${API_BASE_URL}${env.signalRUrl}`

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 3000

  /**
   * Initialize SignalR connection
   */
  async start(): Promise<void> {
    // If already connected, don't start again
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return
    }

    // If connecting or reconnecting, wait for it to complete
    if (this.connection?.state === signalR.HubConnectionState.Connecting || 
        this.connection?.state === signalR.HubConnectionState.Reconnecting) {
      return
    }

    // If connection exists but not connected, stop it first
    if (this.connection) {
      try {
        await this.connection.stop()
      } catch (error) {
        // Ignore stop errors
      }
      this.connection = null
    }

    const authStore = useAuthStore.getState()
    const token = authStore.accessToken

    if (!token) {
      console.warn('SignalR: No access token available')
      return
    }

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, {
          accessTokenFactory: () => token,
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
          headers: {
            Authorization: `Bearer ${token}`,
          },
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectDelay * Math.pow(2, retryContext.previousRetryCount)
            }
            return null // Stop reconnecting
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // Connection event handlers
      this.connection.onclose((error) => {
        console.log('SignalR: Connection closed', error)
        this.reconnectAttempts = 0
      })

      this.connection.onreconnecting((error) => {
        console.log('SignalR: Reconnecting...', error)
        this.reconnectAttempts++
      })

      this.connection.onreconnected((connectionId) => {
        console.log('SignalR: Reconnected', connectionId)
        this.reconnectAttempts = 0
      })

      await this.connection.start()
      console.log('SignalR: Connected successfully')
    } catch (error) {
      console.error('SignalR: Connection failed', error)
      throw error
    }
  }

  /**
   * Stop SignalR connection
   */
  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      console.log('SignalR: Connection stopped')
    }
  }

  /**
   * Check if connection is active
   */
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }

  /**
   * Get connection state
   */
  getState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null
  }

  /**
   * Register handler for receiving notifications
   */
  onNotificationReceived(callback: (notification: any) => void): void {
    if (!this.connection) {
      console.warn('SignalR: Connection not initialized')
      return
    }

    this.connection.on('ReceiveNotification', callback)
  }

  /**
   * Register handler for receiving live updates
   */
  onLiveUpdate(callback: (update: any) => void): void {
    if (!this.connection) {
      console.warn('SignalR: Connection not initialized')
      return
    }

    this.connection.on('ReceiveLiveUpdate', callback)
  }

  /**
   * Register handler for dashboard updates
   */
  onDashboardUpdate(callback: (update: any) => void): void {
    if (!this.connection) {
      console.warn('SignalR: Connection not initialized')
      return
    }

    this.connection.on('ReceiveDashboardUpdate', callback)
  }

  /**
   * Register handler for entity updates (guests, transfers, etc.)
   */
  onEntityUpdate(entityType: string, callback: (update: any) => void): void {
    if (!this.connection) {
      console.warn('SignalR: Connection not initialized')
      return
    }

    this.connection.on(`Receive${entityType}Update`, callback)
  }

  /**
   * Remove handler
   */
  off(eventName: string, callback?: (...args: any[]) => void): void {
    if (!this.connection) {
      return
    }

    if (callback) {
      this.connection.off(eventName, callback)
    }
  }

  /**
   * Send message to hub (if needed)
   */
  async send(methodName: string, ...args: any[]): Promise<any> {
    if (!this.connection || !this.isConnected()) {
      throw new Error('SignalR: Connection not established')
    }

    return this.connection.invoke(methodName, ...args)
  }

  /**
   * Join group (for targeted notifications)
   */
  async joinGroup(groupName: string): Promise<void> {
    if (!this.connection || !this.isConnected()) {
      throw new Error('SignalR: Connection not established')
    }

    await this.connection.invoke('JoinGroup', groupName)
  }

  /**
   * Leave group
   */
  async leaveGroup(groupName: string): Promise<void> {
    if (!this.connection || !this.isConnected()) {
      throw new Error('SignalR: Connection not established')
    }

    await this.connection.invoke('LeaveGroup', groupName)
  }
}

// Singleton instance
export const signalRService = new SignalRService()

