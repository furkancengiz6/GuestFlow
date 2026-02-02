import React, { useState } from 'react';
import { StyleSheet, View, ScrollView } from 'react-native';
import { TextInput, Button, Title, Text, Card, Portal, Dialog, IconButton } from 'react-native-paper';
import { useRoute, useNavigation } from '@react-navigation/native';
import { reviewService, CreateGuestReviewDto } from '../api/reviews';

const AddReviewScreen = () => {
    const route = useRoute<any>();
    const navigation = useNavigation<any>();
    const { guestId, guestFullName } = route.params;

    const [rating, setRating] = useState(5);
    const [cleanliness, setCleanliness] = useState(5);
    const [quality, setQuality] = useState(5);
    const [staff, setStaff] = useState(5);
    const [comment, setComment] = useState('');
    const [loading, setLoading] = useState(false);
    const [visible, setVisible] = useState(false);

    const StarRating = ({ value, onValueChange, label }: { value: number, onValueChange: (v: number) => void, label: string }) => (
        <View style={styles.ratingRow}>
            <Text style={styles.ratingLabel}>{label}</Text>
            <View style={styles.stars}>
                {[1, 2, 3, 4, 5].map((s) => (
                    <IconButton
                        key={s}
                        icon={s <= value ? 'star' : 'star-outline'}
                        iconColor={s <= value ? '#fbc02d' : 'gray'}
                        size={24}
                        onPress={() => onValueChange(s)}
                    />
                ))}
            </View>
        </View>
    );

    const handleSubmit = async () => {
        setLoading(true);
        try {
            const dto: CreateGuestReviewDto = {
                guestId,
                rating,
                cleanlinessRating: cleanliness,
                serviceQualityRating: quality,
                staffRating: staff,
                comment,
            };
            await reviewService.createReview(dto);
            setVisible(true);
        } catch (error) {
            console.error('Error submitting review:', error);
            // Toast error handling could be added here
        } finally {
            setLoading(false);
        }
    };

    return (
        <ScrollView style={styles.container}>
            <Card style={styles.card}>
                <Card.Content>
                    <Title style={styles.title}>{guestFullName} için Değerlendirme</Title>

                    <StarRating label="Genel Puan" value={rating} onValueChange={setRating} />
                    <StarRating label="Temizlik" value={cleanliness} onValueChange={setCleanliness} />
                    <StarRating label="Hizmet Kalitesi" value={quality} onValueChange={setQuality} />
                    <StarRating label="Personel" value={staff} onValueChange={setStaff} />

                    <TextInput
                        label="Yorumunuz"
                        value={comment}
                        onChangeText={setComment}
                        multiline
                        numberOfLines={4}
                        style={styles.input}
                        mode="outlined"
                    />

                    <Button
                        mode="contained"
                        onPress={handleSubmit}
                        loading={loading}
                        disabled={loading}
                        style={styles.button}
                    >
                        Gönder
                    </Button>
                </Card.Content>
            </Card>

            <Portal>
                <Dialog visible={visible} onDismiss={() => {
                    setVisible(false);
                    navigation.goBack();
                }}>
                    <Dialog.Title>Teşekkürler!</Dialog.Title>
                    <Dialog.Content>
                        <Text>Geri bildiriminiz başarıyla iletildi. Deneyiminizi iyileştirmek için çalışmaya devam edeceğiz.</Text>
                    </Dialog.Content>
                    <Dialog.Actions>
                        <Button onPress={() => {
                            setVisible(false);
                            navigation.goBack();
                        }}>Tamam</Button>
                    </Dialog.Actions>
                </Dialog>
            </Portal>
        </ScrollView>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#f5f5f5',
        padding: 16,
    },
    card: {
        marginBottom: 20,
        borderRadius: 12,
    },
    title: {
        marginBottom: 20,
        textAlign: 'center',
        fontSize: 18,
    },
    ratingRow: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        marginBottom: 10,
    },
    ratingLabel: {
        fontSize: 16,
        flex: 1,
    },
    stars: {
        flexDirection: 'row',
    },
    input: {
        marginTop: 10,
        backgroundColor: 'white',
    },
    button: {
        marginTop: 20,
        paddingVertical: 4,
        borderRadius: 8,
    },
});

export default AddReviewScreen;
