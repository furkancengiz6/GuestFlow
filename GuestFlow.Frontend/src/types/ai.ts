export interface AIAction {
    actionType: string;
    description: string;
    parameters?: Record<string, any>;
}

export interface AIChatResponse {
    response: string;
    suggestedActions?: AIAction[];
    confidenceScoreLevel: number;
}

export interface AIChatRequest {
    message: string;
    guestId?: number;
    metadata?: Record<string, string>;
}

export interface ChatMessage {
    id: string;
    text: string;
    sender: 'user' | 'ai';
    timestamp: Date;
    actions?: AIAction[];
}
