import React, { useState } from 'react';
import { View, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native';
import { TextInput, Button, Title, Text, Surface } from 'react-native-paper';
import { useAuthStore } from '../store/authStore';
import apiClient from '../api/client';

const LoginScreen = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const login = useAuthStore((state) => state.login);

    const handleLogin = async () => {
        if (!email || !password) {
            setError('Lütfen tüm alanları doldurun.');
            return;
        }

        setLoading(true);
        setError('');

        try {
            const response = await apiClient.post('/auth/login', { email, password });
            const { accessToken, user } = response.data.data;
            await login(accessToken, user);
        } catch (err: any) {
            setError(err.response?.data?.message || 'Giriş yapılamadı. Lütfen bilgilerinizi kontrol edin.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <KeyboardAvoidingView
            behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
            style={styles.container}
        >
            <Surface style={styles.surface} elevation={4}>
                <Title style={styles.title}>GuestFlow</Title>
                <Text style={styles.subtitle}>Personel Girişi</Text>

                <TextInput
                    label="E-posta"
                    value={email}
                    onChangeText={setEmail}
                    mode="outlined"
                    keyboardType="email-address"
                    autoCapitalize="none"
                    style={styles.input}
                />

                <TextInput
                    label="Şifre"
                    value={password}
                    onChangeText={setPassword}
                    mode="outlined"
                    secureTextEntry
                    style={styles.input}
                />

                {error ? <Text style={styles.error}>{error}</Text> : null}

                <Button
                    mode="contained"
                    onPress={handleLogin}
                    loading={loading}
                    disabled={loading}
                    style={styles.button}
                >
                    Giriş Yap
                </Button>
            </Surface>
        </KeyboardAvoidingView>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        justifyContent: 'center',
        padding: 20,
        backgroundColor: '#f8f9fa',
    },
    surface: {
        padding: 30,
        borderRadius: 16,
        backgroundColor: 'white',
    },
    title: {
        fontSize: 28,
        fontWeight: 'bold',
        textAlign: 'center',
        color: '#1976d2',
        marginBottom: 8,
    },
    subtitle: {
        fontSize: 16,
        textAlign: 'center',
        color: '#666',
        marginBottom: 30,
    },
    input: {
        marginBottom: 16,
    },
    button: {
        marginTop: 10,
        paddingVertical: 5,
        borderRadius: 8,
    },
    error: {
        color: '#d32f2f',
        marginBottom: 16,
        textAlign: 'center',
    },
});

export default LoginScreen;
