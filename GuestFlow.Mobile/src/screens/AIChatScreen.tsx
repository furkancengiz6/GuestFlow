import React, { useState, useRef, useEffect } from 'react';
import {
    View,
    ScrollView,
    StyleSheet,
    KeyboardAvoidingView,
    Platform,
    FlatList,
    TouchableOpacity
} from 'react-native';
import {
    Appbar,
    TextInput,
    IconButton,
    Text,
    Surface,
    Avatar,
    Chip,
    ActivityIndicator,
    useTheme
} from 'react-native-paper';
import { useAIChat, ChatMessage } from '../hooks/useAIChat';
import { useAuthStore } from '../store/authStore';

const AIChatScreen = ({ navigation }: any) => {
    const theme = useTheme();
    const [inputText, setInputText] = useState('');
    const { messages, isProcessing, isConnected, sendMessage } = useAIChat();
    const flatListRef = useRef<FlatList>(null);

    const handleSend = () => {
        if (inputText.trim()) {
            sendMessage(inputText.trim());
            setInputText('');
        }
    };

    useEffect(() => {
        if (messages.length > 0) {
            setTimeout(() => {
                flatListRef.current?.scrollToEnd({ animated: true });
            }, 100);
        }
    }, [messages]);

    const renderMessage = ({ item }: { item: ChatMessage }) => {
        const isAI = item.sender === 'ai';
        return (
            <View style={[styles.messageContainer, isAI ? styles.aiContainer : styles.userContainer]}>
                {isAI && (
                    <Avatar.Icon
                        size={32}
                        icon="robot"
                        style={styles.aiAvatar}
                    />
                )}
                <View style={styles.messageContent}>
                    <Surface
                        style={[
                            styles.bubble,
                            isAI ? styles.aiBubble : styles.userBubble,
                            { backgroundColor: isAI ? '#fff' : theme.colors.primary }
                        ]}
                        elevation={1}
                    >
                        <Text style={[styles.text, { color: isAI ? '#333' : '#fff' }]}>
                            {item.text}
                        </Text>
                    </Surface>

                    {isAI && item.actions && item.actions.length > 0 && (
                        <View style={styles.actionsRow}>
                            {item.actions.map((action, idx) => (
                                <Chip
                                    key={idx}
                                    style={styles.actionChip}
                                    onPress={() => sendMessage(action.description)}
                                    textStyle={styles.actionChipText}
                                >
                                    {action.description}
                                </Chip>
                            ))}
                        </View>
                    )}

                    <Text style={styles.timestamp}>
                        {item.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </Text>
                </View>
            </View>
        );
    };

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.BackAction onPress={() => navigation.goBack()} />
                <Appbar.Content title="Smart Concierge" subtitle={isConnected ? 'Çevrimiçi' : 'Bağlanıyor...'} />
                <Appbar.Action icon="dots-vertical" onPress={() => { }} />
            </Appbar.Header>

            <FlatList
                ref={flatListRef}
                data={messages}
                renderItem={renderMessage}
                keyExtractor={(item) => item.id}
                contentContainerStyle={styles.messagesList}
                ListEmptyComponent={
                    <View style={styles.emptyContainer}>
                        <Avatar.Icon size={64} icon="robot-outline" style={styles.emptyIcon} />
                        <Title style={styles.emptyTitle}>Merhaba {useAuthStore.getState().user?.fullName}! Ben Akıllı Asistanınız.</Title>
                        <Paragraph style={styles.emptyDesc}>
                            Size özel öneriler sunabilir, rezervasyonlarınızı yönetebilir veya operasyonel taleplerinizi asiste edebilirim.
                        </Paragraph>
                    </View>
                }
            />

            {isProcessing && (
                <View style={styles.processing}>
                    <ActivityIndicator size="small" />
                    <Text style={styles.processingText}>AI düşünüyor...</Text>
                </View>
            )}

            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
            >
                <Surface style={styles.inputContainer} elevation={4}>
                    <TextInput
                        placeholder="Asistana sorun..."
                        value={inputText}
                        onChangeText={setInputText}
                        mode="flat"
                        multiline
                        style={styles.input}
                        underlineColor="transparent"
                        activeUnderlineColor="transparent"
                    />
                    <IconButton
                        icon="send"
                        mode="contained"
                        containerColor={theme.colors.primary}
                        iconColor="white"
                        onPress={handleSend}
                        disabled={!inputText.trim() || !isConnected}
                    />
                </Surface>
            </KeyboardAvoidingView>
        </View>
    );
};

// Internal imports to avoid missing Title/Paragraph from react-native-paper if not imported in bulk
import { Title, Paragraph } from 'react-native-paper';

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f0f2f5' },
    messagesList: { padding: 16, paddingBottom: 32 },
    messageContainer: { flexDirection: 'row', marginBottom: 20, maxWidth: '85%' },
    aiContainer: { alignSelf: 'flex-start' },
    userContainer: { alignSelf: 'flex-end', flexDirection: 'row-reverse' },
    aiAvatar: { backgroundColor: '#1976d2', marginRight: 8 },
    messageContent: { flex: 1 },
    bubble: { padding: 12, borderRadius: 16 },
    aiBubble: { borderTopLeftRadius: 0, backgroundColor: 'white' },
    userBubble: { borderTopRightRadius: 0 },
    text: { fontSize: 16, lineHeight: 22 },
    timestamp: { fontSize: 10, color: '#999', marginTop: 4, alignSelf: 'flex-end' },
    actionsRow: { flexDirection: 'row', flexWrap: 'wrap', marginTop: 8, gap: 6 },
    actionChip: { backgroundColor: '#e3f2fd', borderColor: '#bbdefb', borderWidth: 1 },
    actionChipText: { fontSize: 12, color: '#1976d2' },
    processing: { flexDirection: 'row', alignItems: 'center', padding: 8, paddingLeft: 16, backgroundColor: 'rgba(255,255,255,0.8)' },
    processingText: { marginLeft: 8, fontSize: 12, color: '#666' },
    inputContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        padding: 8,
        backgroundColor: 'white',
        borderTopWidth: 1,
        borderTopColor: '#eee'
    },
    input: { flex: 1, backgroundColor: 'transparent', maxHeight: 100 },
    emptyContainer: { flex: 1, alignItems: 'center', justifyContent: 'center', marginTop: 100, padding: 40 },
    emptyIcon: { backgroundColor: 'transparent' },
    emptyTitle: { textAlign: 'center', marginTop: 16, fontSize: 18 },
    emptyDesc: { textAlign: 'center', color: '#666', marginTop: 8 }
});

export default AIChatScreen;
