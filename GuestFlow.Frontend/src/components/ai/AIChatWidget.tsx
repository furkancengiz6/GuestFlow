import React, { useState, useRef, useEffect } from 'react';
import {
    Box,
    Fab,
    Paper,
    Typography,
    IconButton,
    TextField,
    List,
    ListItem,
    Avatar,
    Fade,
    CircularProgress,
    Button,
    Tooltip,
    styled,
    useTheme
} from '@mui/material';
import {
    SmartToy as RobotIcon,
    Close as CloseIcon,
    Send as SendIcon,
    Person as PersonIcon,
    DeleteOutline as DeleteIcon
} from '@mui/icons-material';
import { useAIChat } from '../../hooks/useAIChat';
import { ChatMessage, AIAction } from '../../types/ai';

const StyledFab = styled(Fab)(({ theme }) => ({
    position: 'fixed',
    bottom: 24,
    right: 24,
    zIndex: 1000,
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.primary.contrastText,
    '&:hover': {
        backgroundColor: theme.palette.primary.dark,
        transform: 'scale(1.1)',
    },
    transition: 'all 0.3s ease-in-out',
}));

const ChatWindow = styled(Paper)(({ theme }) => ({
    position: 'fixed',
    bottom: 96,
    right: 24,
    width: 360,
    height: 500,
    display: 'flex',
    flexDirection: 'column',
    zIndex: 1000,
    overflow: 'hidden',
    borderRadius: 16,
    boxShadow: '0 8px 32px rgba(0,0,0,0.15)',
    background: 'rgba(255, 255, 255, 0.95)',
    backdropFilter: 'blur(10px)',
    [theme.breakpoints.down('sm')]: {
        width: 'calc(100% - 48px)',
        height: 'calc(100% - 120px)',
    },
}));

const MessageBubble = styled(Box, {
    shouldForwardProp: (prop) => prop !== 'isAi',
})<{ isAi: boolean }>(({ theme, isAi }) => ({
    maxWidth: '85%',
    padding: theme.spacing(1.5),
    borderRadius: isAi ? '16px 16px 16px 4px' : '16px 16px 4px 16px',
    backgroundColor: isAi ? theme.palette.grey[100] : theme.palette.primary.main,
    color: isAi ? theme.palette.text.primary : theme.palette.primary.contrastText,
    marginBottom: theme.spacing(1),
    alignSelf: isAi ? 'flex-start' : 'flex-end',
    position: 'relative',
    boxShadow: '0 2px 4px rgba(0,0,0,0.05)',
}));

const AIChatWidget: React.FC = () => {
    const theme = useTheme();
    const [isOpen, setIsOpen] = useState(false);
    const [inputText, setInputText] = useState('');
    const { messages, isProcessing, sendMessage, clearMessages } = useAIChat();
    const scrollRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (scrollRef.current) {
            scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
        }
    }, [messages, isProcessing]);

    const handleSend = () => {
        if (!inputText.trim()) return;
        sendMessage(inputText.trim());
        setInputText('');
    };

    const handleActionClick = (action: AIAction) => {
        // In a real app, this would trigger custom logic or navigation
        console.log('Action triggered:', action);
        sendMessage(`Bilgi talep ediyorum: ${action.description}`);
    };

    return (
        <>
            <Tooltip title="AI Asistan" placement="left">
                <StyledFab onClick={() => setIsOpen(!isOpen)}>
                    {isOpen ? <CloseIcon /> : <RobotIcon />}
                </StyledFab>
            </Tooltip>

            <Fade in={isOpen}>
                <ChatWindow elevation={3}>
                    {/* Header */}
                    <Box sx={{
                        p: 2,
                        bgcolor: 'primary.main',
                        color: 'primary.contrastText',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between'
                    }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Avatar sx={{ bgcolor: 'rgba(255,255,255,0.2)', width: 32, height: 32 }}>
                                <RobotIcon fontSize="small" />
                            </Avatar>
                            <Typography variant="subtitle1" fontWeight="bold">
                                Smart Concierge
                            </Typography>
                        </Box>
                        <IconButton size="small" color="inherit" onClick={clearMessages}>
                            <DeleteIcon fontSize="small" />
                        </IconButton>
                    </Box>

                    {/* Messages List */}
                    <Box
                        ref={scrollRef}
                        sx={{
                            flexGrow: 1,
                            overflowY: 'auto',
                            p: 2,
                            display: 'flex',
                            flexDirection: 'column',
                            bgcolor: '#f8f9fa'
                        }}
                    >
                        {messages.length === 0 && (
                            <Box sx={{ textAlign: 'center', mt: 4, color: 'text.secondary' }}>
                                <Typography variant="body2">
                                    Merhaba! Ben GuestFlow Smart Concierge. Rezervasyonlarınız, tercihleriniz veya otel hizmetleri hakkında size yardımcı olabilirim.
                                </Typography>
                            </Box>
                        )}

                        {messages.map((msg) => (
                            <Box key={msg.id} sx={{ display: 'flex', flexDirection: 'column', alignItems: msg.sender === 'ai' ? 'flex-start' : 'flex-end', mb: 1 }}>
                                <MessageBubble isAi={msg.sender === 'ai'}>
                                    <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>{msg.text}</Typography>
                                </MessageBubble>

                                {msg.actions && msg.actions.length > 0 && (
                                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 1 }}>
                                        {msg.actions.map((action, idx) => (
                                            <Button
                                                key={idx}
                                                variant="outlined"
                                                size="small"
                                                onClick={() => handleActionClick(action)}
                                                sx={{
                                                    borderRadius: 4,
                                                    textTransform: 'none',
                                                    fontSize: '0.75rem',
                                                    borderColor: theme.palette.primary.main,
                                                    color: theme.palette.primary.main
                                                }}
                                            >
                                                {action.description}
                                            </Button>
                                        ))}
                                    </Box>
                                )}
                            </Box>
                        ))}

                        {isProcessing && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                                <Avatar sx={{ width: 24, height: 24, bgcolor: 'grey.300' }}>
                                    <RobotIcon sx={{ fontSize: 14 }} />
                                </Avatar>
                                <CircularProgress size={16} thickness={5} />
                                <Typography variant="caption" color="text.secondary">AI yazıyor...</Typography>
                            </Box>
                        )}
                    </Box>

                    {/* Input Area */}
                    <Box sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider', bgcolor: 'white' }}>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                            <TextField
                                fullWidth
                                size="small"
                                placeholder="Mesajınızı yazın..."
                                value={inputText}
                                onChange={(e) => setInputText(e.target.value)}
                                onKeyPress={(e) => e.key === 'Enter' && handleSend()}
                                variant="outlined"
                                sx={{ '& .MuiOutlinedInput-root': { borderRadius: 8 } }}
                            />
                            <IconButton
                                color="primary"
                                onClick={handleSend}
                                disabled={!inputText.trim() || isProcessing}
                                sx={{ bgcolor: 'rgba(25, 118, 210, 0.04)' }}
                            >
                                <SendIcon />
                            </IconButton>
                        </Box>
                    </Box>
                </ChatWindow>
            </Fade>
        </>
    );
};

export default AIChatWidget;
