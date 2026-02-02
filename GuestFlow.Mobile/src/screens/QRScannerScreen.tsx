import React, { useState, useEffect } from 'react';
import { Text, View, StyleSheet, TouchableOpacity } from 'react-native';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { Appbar, Button, Title, Portal, Dialog, Paragraph } from 'react-native-paper';

import { guestService } from '../api/guests';

const QRScannerScreen = ({ navigation }: any) => {
    const [permission, requestPermission] = useCameraPermissions();
    const [scanned, setScanned] = useState(false);
    const [scannedData, setScannedData] = useState('');
    const [visible, setVisible] = useState(false);
    const [loading, setLoading] = useState(false);

    if (!permission) {
        // Camera permissions are still loading.
        return <View />;
    }

    if (!permission.granted) {
        // Camera permissions are not granted yet.
        return (
            <View style={styles.container}>
                <Text style={{ textAlign: 'center' }}>Kamera iznine ihtiyacımız var.</Text>
                <Button onPress={requestPermission}>İzin Ver</Button>
            </View>
        );
    }

    const handleBarCodeScanned = ({ type, data }: any) => {
        setScanned(true);
        setScannedData(data);
        setVisible(true);
    };

    const hideDialog = () => {
        setVisible(false);
        setScanned(false);
    };

    const handleProcess = async () => {
        try {
            setLoading(true);
            // Assuming data is the guest code directly or GUEST_{id}
            const guestCode = scannedData.startsWith('GUEST_') ? scannedData : scannedData;

            const guest = await guestService.validateGuestQr(guestCode);

            hideDialog();

            // Navigate to guest detail
            navigation.navigate('GuestDetail', { guestId: guest.id });

        } catch (error) {
            console.error('QR Validation failed', error);
            alert('Geçersiz QR kod veya misafir bulunamadı.');
            setScanned(false);
        } finally {
            setLoading(false);
        }
    };

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.BackAction onPress={() => navigation.goBack()} />
                <Appbar.Content title="QR Kod Tara" />
            </Appbar.Header>

            <View style={styles.cameraContainer}>
                <CameraView
                    style={StyleSheet.absoluteFillObject}
                    onBarcodeScanned={scanned ? undefined : handleBarCodeScanned}
                    barcodeScannerSettings={{
                        barcodeTypes: ["qr"],
                    }}
                />
                <View style={styles.overlay}>
                    <View style={styles.unfocusedContainer}></View>
                    <View style={styles.focusedContainer}>
                        <View style={styles.unfocusedContainer}></View>
                        <View style={styles.marker}></View>
                        <View style={styles.unfocusedContainer}></View>
                    </View>
                    <View style={styles.unfocusedContainer}></View>
                </View>
            </View>

            <View style={styles.footer}>
                <Paragraph style={styles.hint}>QR kodu merkezdeki kareye hizalayın.</Paragraph>
            </View>

            <Portal>
                <Dialog visible={visible} onDismiss={hideDialog}>
                    <Dialog.Title>QR Kod Algılandı</Dialog.Title>
                    <Dialog.Content>
                        <Paragraph>Okunan Veri: {scannedData}</Paragraph>
                    </Dialog.Content>
                    <Dialog.Actions>
                        <Button onPress={hideDialog}>Kapat</Button>
                        <Button
                            mode="contained"
                            onPress={handleProcess}
                            loading={loading}
                            disabled={loading}
                        >İşle</Button>
                    </Dialog.Actions>
                </Dialog>
            </Portal>
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#000',
    },
    cameraContainer: {
        flex: 1,
        justifyContent: 'center',
    },
    overlay: {
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
    },
    unfocusedContainer: {
        flex: 1,
        backgroundColor: 'rgba(0,0,0,0.5)',
    },
    focusedContainer: {
        flex: 2,
        flexDirection: 'row',
    },
    marker: {
        flex: 8,
        borderWidth: 2,
        borderColor: '#1976d2',
        backgroundColor: 'transparent',
        borderRadius: 16,
    },
    footer: {
        padding: 30,
        backgroundColor: '#fff',
        alignItems: 'center',
    },
    hint: {
        color: '#666',
        textAlign: 'center',
    },
});

export default QRScannerScreen;
