import React, { useState, useEffect } from 'react';
import { View, ScrollView, StyleSheet, Linking, Platform, Alert } from 'react-native';
import {
    Appbar,
    Avatar,
    Title,
    Text,
    Card,
    Button,
    List,
    Divider,
    ActivityIndicator,
    useTheme
} from 'react-native-paper';
import { guestService } from '../api/guests';
import { Guest } from '../types/guest';

const GuestDetailScreen = ({ route, navigation }: any) => {
    const theme = useTheme();
    const { guestId } = route.params;
    const [guest, setGuest] = useState<Guest | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadGuest = async () => {
            try {
                const data = await guestService.getGuestById(guestId);
                setGuest(data);
            } catch (error) {
                console.error('Guest detail load error:', error);
                Alert.alert('Hata', 'Misafir bilgileri yüklenemedi.');
                navigation.goBack();
            } finally {
                setLoading(false);
            }
        };
        loadGuest();
    }, [guestId]);

    const handleCall = () => {
        if (guest?.phoneNumber) {
            Linking.openURL(`tel:${guest.phoneNumber}`);
        } else {
            Alert.alert('Hata', 'Telefon numarası bulunamadı.');
        }
    };

    const handleWhatsApp = () => {
        if (guest?.phoneNumber) {
            // Basic cleanup for phone number
            const cleanPhone = guest.phoneNumber.replace(/[^0-9]/g, '');
            Linking.openURL(`whatsapp://send?phone=${cleanPhone}`);
        } else {
            Alert.alert('Hata', 'Telefon numarası bulunamadı.');
        }
    };

    const handleEmail = () => {
        if (guest?.email) {
            Linking.openURL(`mailto:${guest.email}`);
        } else {
            Alert.alert('Hata', 'E-posta adresi bulunamadı.');
        }
    };

    if (loading) {
        return (
            <View style={styles.center}>
                <ActivityIndicator size="large" />
            </View>
        );
    }

    if (!guest) return null;

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.BackAction onPress={() => navigation.goBack()} />
                <Appbar.Content title="Misafir Detayı" />
            </Appbar.Header>

            <ScrollView contentContainerStyle={styles.content}>
                <View style={styles.header}>
                    <Avatar.Text
                        size={80}
                        label={guest.fullName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase()}
                        style={{ backgroundColor: guest.isVIP ? '#ffd700' : theme.colors.primaryContainer }}
                    />
                    <Title style={styles.name}>{guest.fullName}</Title>
                    <View style={styles.chipRow}>
                        {guest.isVIP && <Text style={styles.vipBadge}>VIP</Text>}
                        <Text style={styles.statusBadge}>{guest.status || 'Aktif'}</Text>
                    </View>
                </View>

                <Card style={styles.actionCard}>
                    <Card.Content style={styles.actionRow}>
                        <TouchableOpacity style={styles.actionItem} onPress={handleCall}>
                            <Avatar.Icon size={40} icon="phone" style={styles.callIcon} />
                            <Text variant="labelSmall">Ara</Text>
                        </TouchableOpacity>
                        <TouchableOpacity style={styles.actionItem} onPress={handleWhatsApp}>
                            <Avatar.Icon size={40} icon="whatsapp" style={styles.waIcon} />
                            <Text variant="labelSmall">WhatsApp</Text>
                        </TouchableOpacity>
                        <TouchableOpacity style={styles.actionItem} onPress={handleEmail}>
                            <Avatar.Icon size={40} icon="email" style={styles.emailIcon} />
                            <Text variant="labelSmall">E-posta</Text>
                        </TouchableOpacity>
                    </Card.Content>
                </Card>

                <List.Section>
                    <List.Subheader>İletişim Bilgileri</List.Subheader>
                    <List.Item
                        title="E-posta"
                        description={guest.email || 'Belirtilmemiş'}
                        left={props => <List.Icon {...props} icon="email-outline" />}
                    />
                    <List.Item
                        title="Telefon"
                        description={guest.phoneNumber || 'Belirtilmemiş'}
                        left={props => <List.Icon {...props} icon="phone-outline" />}
                    />
                    <Divider />
                    <List.Subheader>Konaklama Bilgileri</List.Subheader>
                    <List.Item
                        title="Oda Numarası"
                        description={guest.roomNumber || 'Atanmamış'}
                        left={props => <List.Icon {...props} icon="room-service-outline" />}
                    />
                    <List.Item
                        title="Milliyet"
                        description={guest.nationality || 'Belirtilmemiş'}
                        left={props => <List.Icon {...props} icon="flag-outline" />}
                    />
                    <List.Item
                        title="PMS ID"
                        description={guest.pmsGuestId || '-'}
                        left={props => <List.Icon {...props} icon="sync" />}
                    />
                </List.Section>

                <View style={styles.buttonRow}>
                    <Button
                        mode="contained"
                        style={[styles.serviceButton, { flex: 1 }]}
                        onPress={() => navigation.navigate('AddService', { guestId: guest.id, guestName: guest.fullName })}
                    >
                        Hizmet Ekle
                    </Button>
                    <Button
                        mode="outlined"
                        style={[styles.serviceButton, { flex: 1 }]}
                        onPress={() => navigation.navigate('AddReview', { guestId: guest.id, guestFullName: guest.fullName })}
                    >
                        Değerlendirme Yap
                    </Button>
                </View>
            </ScrollView>
        </View>
    );
};

// Internal TouchableOpacity since it's not imported from react-native above
import { TouchableOpacity as RNTouchableOpacity } from 'react-native';
const TouchableOpacity: any = RNTouchableOpacity;

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f8f9fa' },
    center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
    content: { paddingBottom: 30 },
    header: { alignItems: 'center', padding: 24, backgroundColor: 'white' },
    name: { marginTop: 12, fontSize: 24, fontWeight: 'bold' },
    chipRow: { flexDirection: 'row', marginTop: 8, gap: 8 },
    vipBadge: { backgroundColor: '#ffd700', paddingHorizontal: 12, paddingVertical: 4, borderRadius: 12, fontWeight: 'bold', fontSize: 12 },
    statusBadge: { backgroundColor: '#e0e0e0', paddingHorizontal: 12, paddingVertical: 4, borderRadius: 12, fontSize: 12 },
    actionCard: { margin: 16, borderRadius: 16 },
    actionRow: { flexDirection: 'row', justifyContent: 'space-around', paddingVertical: 8 },
    actionItem: { alignItems: 'center', gap: 4 },
    callIcon: { backgroundColor: '#4caf50' },
    waIcon: { backgroundColor: '#25d366' },
    emailIcon: { backgroundColor: '#1976d2' },
    serviceButton: { marginHorizontal: 4, borderRadius: 8, paddingVertical: 4 },
    buttonRow: { flexDirection: 'row', padding: 12 },
});

export default GuestDetailScreen;
