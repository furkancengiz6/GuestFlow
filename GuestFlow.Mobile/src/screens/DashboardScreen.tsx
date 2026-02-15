import React, { useState, useEffect } from 'react';
import { View, ScrollView, StyleSheet, RefreshControl } from 'react-native';
import {
    Appbar,
    Card,
    Title,
    Paragraph,
    Button,
    Text,
    ActivityIndicator,
    List,
    Avatar,
    Chip,
    Divider,
    useTheme
} from 'react-native-paper';
import { useAuthStore } from '../store/authStore';
import { dashboardService } from '../api/dashboard';
import { revenueService, RevenueDashboard } from '../api/revenue';
import { cacheService } from '../utils/cache';
import { DashboardSummary, UpcomingServiceItem } from '../types/operations';
import ForecastWidget from '../components/dashboard/ForecastWidget';
import AIInsightsWidget from '../components/dashboard/AIInsightsWidget';

const DashboardScreen = ({ navigation }: any) => {
    const theme = useTheme();
    const user = useAuthStore((state) => state.user);
    const logout = useAuthStore((state) => state.logout);

    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);
    const [summary, setSummary] = useState<DashboardSummary | null>(null);
    const [upcomingServices, setUpcomingServices] = useState<UpcomingServiceItem[]>([]);
    const [revenueDashboard, setRevenueDashboard] = useState<RevenueDashboard | null>(null);

    const loadData = async () => {
        try {
            // Priority 1: Try to load from cache first for instant UI
            const cachedSummary = await cacheService.get<DashboardSummary>('dashboard_summary');
            const cachedServices = await cacheService.get<UpcomingServiceItem[]>('upcoming_services');
            const cachedRevenue = await cacheService.get<RevenueDashboard>('revenue_dashboard');

            if (cachedSummary && cachedServices) {
                setSummary(cachedSummary);
                setUpcomingServices(cachedServices);
                setLoading(false);
            }
            if (cachedRevenue) {
                setRevenueDashboard(cachedRevenue);
            }

            // Priority 2: Fetch fresh data from API
            const summaryData = await dashboardService.getSummary();
            const servicesData = await dashboardService.getUpcomingServices();

            setSummary(summaryData);
            setUpcomingServices(servicesData.items || []);

            // Priority 3: Update Cache
            await cacheService.save('dashboard_summary', summaryData);
            await cacheService.save('upcoming_services', servicesData.items || []);

            // Load Revenue Data (non-blocking)
            try {
                const revenueData = await revenueService.getRevenueDashboard();
                setRevenueDashboard(revenueData);
                await cacheService.save('revenue_dashboard', revenueData);
            } catch (revenueError) {
                console.warn('Revenue data load error:', revenueError);
            }
        } catch (error) {
            console.error('Data load error:', error);
        } finally {
            setLoading(false);
            setRefreshing(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const onRefresh = () => {
        setRefreshing(true);
        loadData();
    };

    if (loading) {
        return (
            <View style={styles.center}>
                <ActivityIndicator size="large" color={theme.colors.primary} />
            </View>
        );
    }

    return (
        <View style={styles.container}>
            <Appbar.Header elevated>
                <Appbar.Content title="GuestFlow Ops" subtitle={`Hoş geldin, ${user?.fullName}`} />
                <Appbar.Action icon="logout" onPress={logout} />
            </Appbar.Header>

            <ScrollView
                contentContainerStyle={styles.content}
                refreshControl={
                    <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
                }
            >
                {/* Quick Stats */}
                <View style={styles.summaryContainer}>
                    <Card style={[styles.summaryCard, { backgroundColor: '#e3f2fd' }]}>
                        <Card.Content>
                            <Text variant="titleLarge" style={styles.statVal}>{summary?.todayCheckIns}</Text>
                            <Text variant="bodySmall">Check-in</Text>
                        </Card.Content>
                    </Card>
                    <Card style={[styles.summaryCard, { backgroundColor: '#fce4ec' }]}>
                        <Card.Content>
                            <Text variant="titleLarge" style={styles.statVal}>{summary?.todayCheckOuts}</Text>
                            <Text variant="bodySmall">Check-out</Text>
                        </Card.Content>
                    </Card>
                    <Card style={[styles.summaryCard, { backgroundColor: '#f1f8e9' }]}>
                        <Card.Content>
                            <Text variant="titleLarge" style={styles.statVal}>{summary?.activeGuestsCount}</Text>
                            <Text variant="bodySmall">Aktifler</Text>
                        </Card.Content>
                    </Card>
                </View>

                {/* Guest Satisfaction Summary */}
                <Card style={styles.satisfactionCard} mode="elevated">
                    <Card.Content style={styles.satisfactionContent}>
                        <View>
                            <Text variant="titleMedium" style={{ color: '#1a237e' }}>Misafir Memnuniyeti</Text>
                            <View style={styles.ratingBrief}>
                                <Text variant="displaySmall" style={styles.ratingVal}>{summary?.averageRating || 0}</Text>
                                <View style={{ marginLeft: 12 }}>
                                    <View style={styles.starsSmall}>
                                        {[1, 2, 3, 4, 5].map(s => (
                                            <Avatar.Icon
                                                key={s}
                                                size={20}
                                                icon={s <= Math.round(summary?.averageRating || 0) ? "star" : "star-outline"}
                                                style={{ backgroundColor: 'transparent' }}
                                                color="#fbc02d"
                                            />
                                        ))}
                                    </View>
                                    <Text variant="bodySmall">Genel Puan Ortalaması</Text>
                                </View>
                            </View>
                        </View>
                        <Avatar.Icon size={48} icon="emoticon-happy-outline" style={{ backgroundColor: '#e8eaf6' }} color="#1a237e" />
                    </Card.Content>
                </Card>

                {/* Forecast Widget (Analytics) */}
                <ForecastWidget />

                {/* AI & Proactive Intelligence */}
                <AIInsightsWidget />

                {/* Revenue KPI Summary */}
                {revenueDashboard && (
                    <Card style={styles.revenueCard} mode="elevated">
                        <Card.Content>
                            <View style={styles.rowBetween}>
                                <Title style={[styles.smallTitle, { color: '#2e7d32' }]}>💰 Gelir Metrikleri</Title>
                                <Text variant="labelSmall">Son 30 Gün</Text>
                            </View>
                            <Divider style={{ marginVertical: 10 }} />
                            <View style={styles.revenueGrid}>
                                <View style={styles.revenueItem}>
                                    <Text variant="titleLarge" style={{ fontWeight: 'bold', color: '#1976d2' }}>
                                        ₺{revenueDashboard.adr.toFixed(0)}
                                    </Text>
                                    <Text variant="bodySmall">ADR</Text>
                                </View>
                                <View style={styles.revenueItem}>
                                    <Text variant="titleLarge" style={{ fontWeight: 'bold', color: '#7b1fa2' }}>
                                        ₺{revenueDashboard.revPar.toFixed(0)}
                                    </Text>
                                    <Text variant="bodySmall">RevPAR</Text>
                                </View>
                                <View style={styles.revenueItem}>
                                    <Text variant="titleLarge" style={{ fontWeight: 'bold', color: '#388e3c' }}>
                                        %{(revenueDashboard.occupancyRate * 100).toFixed(1)}
                                    </Text>
                                    <Text variant="bodySmall">Doluluk</Text>
                                </View>
                            </View>
                        </Card.Content>
                    </Card>
                )}

                {/* VIP Activity Summary */}
                <Card style={styles.vipSummaryCard}>
                    <Card.Content>
                        <View style={styles.rowBetween}>
                            <Title style={[styles.smallTitle, { color: '#bf360c' }]}>⭐ VIP Aktivite Özeti</Title>
                            <Text variant="labelSmall">Son 24 Saat</Text>
                        </View>
                        <Divider style={{ marginVertical: 8 }} />
                        <View style={styles.vipRow}>
                            <Avatar.Text size={32} label="FC" style={{ backgroundColor: '#ffd700' }} />
                            <View style={{ marginLeft: 10, flex: 1 }}>
                                <Text variant="bodyMedium" style={{ fontWeight: 'bold' }}>Furkan Cengiz</Text>
                                <Text variant="bodySmall">QR Tarandı - Spa Merkezi</Text>
                            </View>
                            <Text variant="labelSmall">14:20</Text>
                        </View>
                    </Card.Content>
                </Card>

                <Title style={styles.sectionTitle}>Bugünkü Görevlerim</Title>
                {upcomingServices.length > 0 ? (
                    upcomingServices.map((service, index) => (
                        <Card key={index} style={styles.serviceCard} mode="contained">
                            <Card.Content>
                                <View style={styles.serviceHeader}>
                                    <Chip
                                        icon={service.serviceType === 'Transfer' ? 'car' : 'map-marker'}
                                        selectedColor={service.isUrgent ? 'red' : theme.colors.primary}
                                        style={styles.serviceChip}
                                    >
                                        {service.serviceType}
                                    </Chip>
                                    <Text variant="labelMedium">{new Date(service.serviceDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</Text>
                                </View>
                                <Title style={styles.guestTitle}>{service.guestName}</Title>
                                <View style={[styles.row, { marginTop: 4 }]}>
                                    <Avatar.Icon size={20} icon="home-outline" style={{ backgroundColor: 'transparent' }} color="#666" />
                                    <Text variant="bodyMedium" style={{ marginLeft: 4 }}>Oda: {service.roomNumber || 'Bekliyor'}</Text>
                                </View>
                                {service.cityName && (
                                    <View style={[styles.row, { marginTop: 4 }]}>
                                        <Avatar.Icon size={20} icon="map-marker-outline" style={{ backgroundColor: 'transparent' }} color="#666" />
                                        <Text variant="bodyMedium" style={{ marginLeft: 4 }}>{service.cityName}</Text>
                                    </View>
                                )}
                            </Card.Content>
                            <Card.Actions>
                                <Button mode="text" onPress={() => { }}>Ayrıntılar</Button>
                                <Button mode="contained-tonal" onPress={() => { }}>Tamamla</Button>
                            </Card.Actions>
                        </Card>
                    ))
                ) : (
                    <Text style={styles.emptyText}>Şu an planlanmış bir görev bulunmuyor.</Text>
                )}

                <Divider style={{ marginVertical: 10 }} />

                <Title style={styles.sectionTitle}>Hızlı Erişim</Title>
                <View style={styles.actionsGrid}>
                    <Button icon="qrcode-scan" mode="outlined" style={styles.actionButton} onPress={() => navigation.navigate('QRScanner')}>QR Kod</Button>
                    <Button icon="account-group" mode="outlined" style={styles.actionButton} onPress={() => navigation.navigate('Guests')}>Misafirler</Button>
                    <Button icon="phone" mode="outlined" style={styles.actionButton} onPress={() => { }}>Rehber</Button>
                    <Button icon="chat-processing" mode="outlined" style={styles.actionButton} onPress={() => navigation.navigate('AI')}>Asistan</Button>
                </View>
            </ScrollView>
        </View>
    );
};

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#fcfcfc' },
    center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
    content: { padding: 16 },
    summaryContainer: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 20
    },
    summaryCard: {
        flex: 0.31,
        borderRadius: 12,
        elevation: 0,
        borderWidth: 1,
        borderColor: '#eee'
    },
    statVal: {
        fontWeight: 'bold',
        color: '#333'
    },
    sectionTitle: {
        fontSize: 18,
        fontWeight: 'bold',
        marginBottom: 12,
        marginTop: 8
    },
    serviceCard: {
        marginBottom: 12,
        borderRadius: 16,
        borderLeftWidth: 4,
        borderLeftColor: '#1976d2'
    },
    serviceHeader: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: 8
    },
    serviceChip: { backgroundColor: 'white', borderColor: '#eee', borderWidth: 1 },
    guestTitle: { fontSize: 18 },
    row: { flexDirection: 'row', alignItems: 'center' },
    actionsGrid: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        justifyContent: 'space-between'
    },
    actionButton: {
        width: '48%',
        marginBottom: 10,
        borderRadius: 12
    },
    emptyText: { textAlign: 'center', color: '#888', marginVertical: 20 },
    satisfactionCard: {
        marginBottom: 20,
        borderRadius: 16,
        backgroundColor: '#ffffff',
        elevation: 2
    },
    satisfactionContent: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center'
    },
    ratingBrief: {
        flexDirection: 'row',
        alignItems: 'center',
        marginTop: 8
    },
    ratingVal: {
        fontWeight: 'bold',
        color: '#1a237e'
    },
    starsSmall: {
        flexDirection: 'row',
        marginLeft: -4
    },
    trendCard: {
        marginBottom: 16,
        borderRadius: 16,
        backgroundColor: '#f5f5f5',
        borderWidth: 0
    },
    vipSummaryCard: {
        marginBottom: 20,
        borderRadius: 16,
        borderLeftWidth: 4,
        borderLeftColor: '#ffd700',
        elevation: 1
    },
    rowBetween: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center'
    },
    smallTitle: {
        fontSize: 16,
        fontWeight: 'bold'
    },
    chartMock: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        alignItems: 'flex-end',
        height: 100,
        paddingVertical: 10
    },
    bar: {
        width: 20,
        backgroundColor: '#3f51b5',
        borderRadius: 4
    },
    chartLabels: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        marginTop: 4
    },
    chartLabel: {
        color: '#888'
    },
    vipRow: {
        flexDirection: 'row',
        alignItems: 'center',
        marginTop: 4
    },
    revenueCard: {
        marginBottom: 20,
        borderRadius: 16,
        borderLeftWidth: 4,
        borderLeftColor: '#4caf50',
        elevation: 2
    },
    revenueGrid: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        alignItems: 'center'
    },
    revenueItem: {
        alignItems: 'center',
        flex: 1
    }
});

export default DashboardScreen;
