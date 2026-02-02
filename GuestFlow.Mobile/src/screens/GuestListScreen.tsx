import React, { useState, useEffect } from 'react';
import { View, FlatList, StyleSheet } from 'react-native';
import {
    Appbar,
    Searchbar,
    List,
    Avatar,
    Divider,
    ActivityIndicator,
    Text,
    useTheme
} from 'react-native-paper';
import { guestService } from '../api/guests';
import { cacheService } from '../utils/cache';
import { Guest } from '../types/guest';

const GuestListScreen = ({ navigation }: any) => {
    const theme = useTheme();
    const [searchQuery, setSearchQuery] = useState('');
    const [guests, setGuests] = useState<Guest[]>([]);
    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);

    const loadGuests = async (search = '') => {
        try {
            // Load from cache initially if no search query
            if (!search) {
                const cached = await cacheService.get<Guest[]>('guest_list');
                if (cached) {
                    setGuests(cached);
                    setLoading(false);
                }
            }

            const data = await guestService.getGuests(1, 40, search);
            setGuests(data.items);

            // Update cache only for empty search (full list)
            if (!search) {
                await cacheService.save('guest_list', data.items);
            }
        } catch (error) {
            console.error('Guest load error:', error);
        } finally {
            setLoading(false);
            setRefreshing(false);
        }
    };

    useEffect(() => {
        loadGuests();
    }, []);

    const handleSearch = (query: string) => {
        setSearchQuery(query);
        loadGuests(query);
    };

    const onRefresh = () => {
        setRefreshing(true);
        loadGuests(searchQuery);
    };

    const renderItem = ({ item }: { item: Guest }) => (
        <List.Item
            title={item.fullName}
            description={`${item.roomNumber ? `Oda: ${item.roomNumber}` : 'Oda Atanmamış'} | ${item.phoneNumber || 'Telefon yok'}`}
            left={props => (
                <Avatar.Text
                    {...props}
                    size={48}
                    label={item.fullName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase()}
                    style={{ backgroundColor: item.isVIP ? '#ffd700' : theme.colors.primaryContainer }}
                    labelStyle={{ color: item.isVIP ? '#000' : theme.colors.onPrimaryContainer }}
                />
            )}
            right={props => <List.Icon {...props} icon="chevron-right" />}
            onPress={() => navigation.navigate('GuestDetail', { guestId: item.id })}
        />
    );

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.BackAction onPress={() => navigation.goBack()} />
                <Appbar.Content title="Misafirler" />
            </Appbar.Header>

            <Searchbar
                placeholder="Ad, oda veya tel ara..."
                onChangeText={handleSearch}
                value={searchQuery}
                style={styles.searchBar}
            />

            {loading ? (
                <View style={styles.center}>
                    <ActivityIndicator size="large" />
                </View>
            ) : (
                <FlatList
                    data={guests}
                    renderItem={renderItem}
                    keyExtractor={(item) => item.id.toString()}
                    refreshing={refreshing}
                    onRefresh={onRefresh}
                    ItemSeparatorComponent={() => <Divider />}
                    contentContainerStyle={guests.length === 0 && styles.listEmpty}
                    ListEmptyComponent={
                        <Text style={styles.emptyText}>Sonuç bulunamadı.</Text>
                    }
                />
            )}
        </View>
    );
};

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: 'white' },
    searchBar: { margin: 12, borderRadius: 12, elevation: 0, backgroundColor: '#f0f0f0' },
    center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
    listEmpty: { flex: 1, justifyContent: 'center' },
    emptyText: { textAlign: 'center', color: '#888' }
});

export default GuestListScreen;
