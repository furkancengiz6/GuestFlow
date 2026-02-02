import React, { useState } from 'react';
import { View, StyleSheet, ScrollView, Alert } from 'react-native';
import {
    Appbar,
    TextInput,
    Button,
    RadioButton,
    Text,
    Surface,
    Divider,
    Title
} from 'react-native-paper';
import { format } from 'date-fns';
import apiClient from '../api/client';

const AddServiceScreen = ({ route, navigation }: any) => {
    const { guestId, guestName } = route.params;
    const [serviceType, setServiceType] = useState('Transfer');
    const [description, setDescription] = useState('');
    const [serviceDate, setServiceDate] = useState(new Date());
    const [loading, setLoading] = useState(false);

    // Form fields for Transfer
    const [pickupLocation, setPickupLocation] = useState('');
    const [dropoffLocation, setDropoffLocation] = useState('');

    // Form fields for Tour
    const [tourName, setTourName] = useState('');

    const handleSave = async () => {
        setLoading(true);
        try {
            const payload = {
                guestId,
                serviceType,
                description,
                serviceDate: serviceDate.toISOString(),
                metadata: JSON.stringify({
                    pickupLocation,
                    dropoffLocation,
                    tourName
                })
            };

            await apiClient.post('/Operations/services', payload);
            Alert.alert('Başarılı', 'Hizmet rezervasyonu oluşturuldu.');
            navigation.goBack();
        } catch (error) {
            console.error('Service save error:', error);
            Alert.alert('Hata', 'Hizmet kaydedilemedi.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.BackAction onPress={() => navigation.goBack()} />
                <Appbar.Content title="Hizmet Ekle" subtitle={guestName} />
            </Appbar.Header>

            <ScrollView contentContainerStyle={styles.content}>
                <Surface style={styles.surface} elevation={1}>
                    <Title style={styles.sectionTitle}>Hizmet Tipi</Title>
                    <RadioButton.Group onValueChange={value => setServiceType(value)} value={serviceType}>
                        <View style={styles.radioRow}>
                            <View style={styles.radioItem}>
                                <RadioButton value="Transfer" />
                                <Text>Transfer</Text>
                            </View>
                            <View style={styles.radioItem}>
                                <RadioButton value="Tour" />
                                <Text>Tur</Text>
                            </View>
                            <View style={styles.radioItem}>
                                <RadioButton value="Restaurant" />
                                <Text>Restoran</Text>
                            </View>
                        </View>
                    </RadioButton.Group>
                </Surface>

                <Surface style={styles.formSurface} elevation={1}>
                    <TextInput
                        label="Açıklama / Not"
                        value={description}
                        onChangeText={setDescription}
                        mode="outlined"
                        multiline
                        numberOfLines={3}
                        style={styles.input}
                    />

                    <Divider style={styles.divider} />

                    {serviceType === 'Transfer' && (
                        <>
                            <TextInput
                                label="Alış Noktası"
                                value={pickupLocation}
                                onChangeText={setPickupLocation}
                                mode="outlined"
                                style={styles.input}
                            />
                            <TextInput
                                label="Varış Noktası"
                                value={dropoffLocation}
                                onChangeText={setDropoffLocation}
                                mode="outlined"
                                style={styles.input}
                            />
                        </>
                    )}

                    {serviceType === 'Tour' && (
                        <TextInput
                            label="Tur Adı / Tipi"
                            value={tourName}
                            onChangeText={setTourName}
                            mode="outlined"
                            style={styles.input}
                        />
                    )}

                    <Text style={styles.dateText}>Tarih: {format(serviceDate, 'dd.MM.yyyy HH:mm')}</Text>
                    <Text style={styles.hint}>Mock: Tarih seçici gerçek cihazda açılacaktır.</Text>

                    <Button
                        mode="contained"
                        onPress={handleSave}
                        loading={loading}
                        disabled={loading}
                        style={styles.saveButton}
                    >
                        Rezervasyonu Kaydet
                    </Button>
                </Surface>
            </ScrollView>
        </View>
    );
};

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f8f9fa' },
    content: { padding: 16 },
    surface: { padding: 16, borderRadius: 12, marginBottom: 16, backgroundColor: 'white' },
    formSurface: { padding: 16, borderRadius: 12, backgroundColor: 'white' },
    sectionTitle: { fontSize: 16, marginBottom: 8 },
    radioRow: { flexDirection: 'row', justifyContent: 'space-around' },
    radioItem: { flexDirection: 'row', alignItems: 'center' },
    input: { marginBottom: 16 },
    divider: { marginBottom: 16 },
    dateText: { fontSize: 16, fontWeight: 'bold', marginBottom: 4 },
    hint: { fontSize: 12, color: '#888', marginBottom: 20 },
    saveButton: { paddingVertical: 6, borderRadius: 8 }
});

export default AddServiceScreen;
