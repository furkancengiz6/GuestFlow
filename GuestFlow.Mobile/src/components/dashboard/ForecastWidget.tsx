import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Dimensions } from 'react-native';
import { Card, Text, Button, useTheme, SegmentedButtons, ActivityIndicator } from 'react-native-paper';
import { analyticsService, OccupancyForecast, RevenueForecast } from '../../services/api/analyticsService';

const ForecastWidget = () => {
    const theme = useTheme();
    const [loading, setLoading] = useState(true);
    const [viewMode, setViewMode] = useState('occupancy'); // occupancy | revenue
    const [occupancyData, setOccupancyData] = useState<OccupancyForecast[]>([]);
    const [revenueData, setRevenueData] = useState<RevenueForecast[]>([]);

    useEffect(() => {
        loadForecasts();
    }, []);

    const loadForecasts = async () => {
        setLoading(true);
        try {
            const start = new Date();
            const end = new Date();
            end.setDate(end.getDate() + 7); // Next 7 days

            const [occ, rev] = await Promise.all([
                analyticsService.getOccupancyForecast(start, end),
                analyticsService.getRevenueForecast(start, end)
            ]);

            setOccupancyData(occ);
            setRevenueData(rev);
        } catch (error) {
            console.error('Forecast load error:', error);
        } finally {
            setLoading(false);
        }
    };

    const renderChart = () => {
        // Simplified chart visualization using Views since we don't have a charting library installed yet
        // In a real app, use react-native-chart-kit or victory-native
        const data = viewMode === 'occupancy' ? occupancyData : revenueData;
        if (!data.length) return <Text>No data available</Text>;

        const maxValue = viewMode === 'occupancy'
            ? 100 // Occupancy is %
            : Math.max(...(revenueData.map(d => d.forecastedRevenue))) * 1.2;

        return (
            <View style={styles.chartContainer}>
                {data.map((item, index) => {
                    const value = viewMode === 'occupancy'
                        ? (item as OccupancyForecast).forecastedOccupancyRate * 100
                        : (item as RevenueForecast).forecastedRevenue;

                    const height = (value / maxValue) * 100;
                    const dateLabel = new Date((item as any).date).getDate().toString();

                    return (
                        <View key={index} style={styles.barContainer}>
                            <View style={[styles.bar, {
                                height: `${Math.max(height, 5)}%`,
                                backgroundColor: viewMode === 'occupancy' ? theme.colors.primary : theme.colors.tertiary
                            }]} />
                            <Text variant="labelSmall" style={styles.label}>{dateLabel}</Text>
                        </View>
                    );
                })}
            </View>
        );
    };

    return (
        <Card style={styles.card} mode="elevated">
            <Card.Content>
                <View style={styles.header}>
                    <Text variant="titleMedium" style={styles.title}>
                        🔮 {viewMode === 'occupancy' ? 'Doluluk Tahmini' : 'Gelir Tahmini'}
                    </Text>
                    <Text variant="labelSmall" style={{ color: '#666' }}>7 Gün</Text>
                </View>

                <SegmentedButtons
                    value={viewMode}
                    onValueChange={setViewMode}
                    buttons={[
                        { value: 'occupancy', label: 'Doluluk' },
                        { value: 'revenue', label: 'Gelir' },
                    ]}
                    style={styles.toggles}
                    density="small"
                />

                {loading ? (
                    <ActivityIndicator style={{ padding: 20 }} />
                ) : (
                    <>
                        <View style={styles.insight}>
                            <Text variant="displaySmall" style={{ fontWeight: 'bold', color: theme.colors.secondary }}>
                                {viewMode === 'occupancy'
                                    ? `%${(occupancyData[0]?.forecastedOccupancyRate * 100).toFixed(0)}`
                                    : `₺${revenueData[0]?.forecastedRevenue.toFixed(0)}`
                                }
                            </Text>
                            <Text variant="bodySmall">Yarın Beklenen</Text>
                        </View>
                        {renderChart()}
                    </>
                )}
            </Card.Content>
        </Card>
    );
};

const styles = StyleSheet.create({
    card: {
        marginBottom: 16,
        borderRadius: 16,
        backgroundColor: '#fff',
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: 12
    },
    title: { fontWeight: 'bold' },
    toggles: { marginBottom: 16 },
    insight: { alignItems: 'center', marginBottom: 16 },
    chartContainer: {
        flexDirection: 'row',
        justifyContent: 'space-around',
        alignItems: 'flex-end',
        height: 120,
        paddingBottom: 8,
        borderBottomWidth: 1,
        borderBottomColor: '#eee'
    },
    barContainer: {
        alignItems: 'center',
        height: '100%',
        justifyContent: 'flex-end',
        width: 20
    },
    bar: {
        width: 12,
        borderRadius: 4,
        minHeight: 4
    },
    label: {
        marginTop: 4,
        color: '#888'
    }
});

export default ForecastWidget;
