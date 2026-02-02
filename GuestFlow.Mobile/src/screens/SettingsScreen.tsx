import React, { useEffect } from 'react';
import { View, StyleSheet } from 'react-native';
import { Appbar, List, Button, Title, Divider, Avatar, useTheme, Switch, Text } from 'react-native-paper';
import { useAuthStore } from '../store/authStore';
import { useSettingsStore } from '../store/settingsStore';

const SettingsScreen = () => {
    const theme = useTheme();
    const { user, logout } = useAuthStore();
    const {
        isBiometricEnabled,
        isDarkMode,
        setBiometricEnabled,
        setDarkMode,
        loadSettings
    } = useSettingsStore();

    useEffect(() => {
        loadSettings();
    }, []);

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.Content title="Ayarlar" />
            </Appbar.Header>

            <View style={styles.profileSection}>
                <Avatar.Text
                    size={80}
                    label={user?.fullName?.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase() || 'U'}
                    style={{ backgroundColor: theme.colors.primaryContainer }}
                />
                <Title style={styles.userName}>{user?.fullName}</Title>
                <Text style={styles.userEmail}>{user?.email}</Text>
            </View>

            <Divider />

            <List.Section>
                <List.Subheader>Kanal Yöneticisi (Channel Manager)</List.Subheader>
                <List.Item
                    title="Expedia"
                    description="Bağlı - Aktif"
                    left={props => <Avatar.Icon {...props} icon="earth" size={40} style={{ backgroundColor: '#e3f2fd' }} color="#0277bd" />}
                    right={props => <List.Icon {...props} icon="check-circle" color="#4caf50" />}
                />
                <List.Item
                    title="Booking.com"
                    description="Bağlı - Aktif"
                    left={props => <Avatar.Icon {...props} icon="briefcase" size={40} style={{ backgroundColor: '#e8eaf6' }} color="#1a237e" />}
                    right={props => <List.Icon {...props} icon="check-circle" color="#4caf50" />}
                />
            </List.Section>

            <List.Section>
                <List.Subheader>Uygulama Ayarları</List.Subheader>
                <List.Item
                    title="Biyometrik Kilit"
                    description="Girişte parmak izi/yüz tanıma iste"
                    left={props => <List.Icon {...props} icon="fingerprint" />}
                    right={() => (
                        <Switch
                            value={isBiometricEnabled}
                            onValueChange={setBiometricEnabled}
                        />
                    )}
                />
                <List.Item
                    title="Koyu Tema"
                    left={props => <List.Icon {...props} icon="theme-light-dark" />}
                    right={() => (
                        <Switch
                            value={isDarkMode}
                            onValueChange={setDarkMode}
                        />
                    )}
                />
                <List.Item
                    title="Dil / Language"
                    description="Türkçe"
                    left={props => <List.Icon {...props} icon="translate" />}
                    right={props => <List.Icon {...props} icon="chevron-right" />}
                    onPress={() => { }}
                />
            </List.Section>

            <Divider />

            <List.Section>
                <List.Subheader>Hesap</List.Subheader>
                <List.Item
                    title="Şifre Değiştir"
                    left={props => <List.Icon {...props} icon="lock-outline" />}
                    onPress={() => { }}
                />
                <List.Item
                    title="Oturumu Kapat"
                    titleStyle={{ color: '#d32f2f' }}
                    left={props => <List.Icon {...props} icon="logout" color="#d32f2f" />}
                    onPress={logout}
                />
            </List.Section>

            <View style={styles.footer}>
                <Text style={styles.versionText}>v1.0.0 (Sprint 8)</Text>
            </View>
        </View>
    );
};

// Styles
const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f8f9fa' },
    profileSection: { alignItems: 'center', padding: 30, backgroundColor: 'white' },
    userName: { marginTop: 12, fontSize: 22, fontWeight: 'bold' },
    userEmail: { color: '#666' },
    footer: { padding: 20, alignItems: 'center' },
    versionText: { color: '#999', fontSize: 12 }
});

export default SettingsScreen;
