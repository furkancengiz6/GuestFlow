import React, { useState, useEffect } from 'react';
import { View, StyleSheet } from 'react-native';
import { Card, Text, List, Avatar, Chip, ActivityIndicator, useTheme } from 'react-native-paper';
import { intelligenceService, ProactiveRecommendation } from '../../api/intelligence';

const AIInsightsWidget = () => {
    const theme = useTheme();
    const [loading, setLoading] = useState(true);
    const [insights, setInsights] = useState<ProactiveRecommendation[]>([]);

    useEffect(() => {
        loadAIInsights();
    }, []);

    const loadAIInsights = async () => {
        setLoading(true);
        try {
            const data = await intelligenceService.getProactiveRecommendations();
            setInsights(data || []);
        } catch (error) {
            console.error('AI Insights load error:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return <ActivityIndicator style={{ margin: 20 }} />;
    }

    if (insights.length === 0) {
        return null; // Don't show if no insights
    }

    const getIcon = (type: string) => {
        switch (type.toUpperCase()) {
            case 'FLEET_OPTIMIZATION': return 'car-connected';
            case 'REVENUE': return 'currency-usd';
            case 'PROBLEM_PREVENTION': return 'shield-alert';
            default: return 'auto-awesome';
        }
    };

    const getPriorityColor = (priority: number) => {
        if (priority > 0.8) return '#f44336'; // High
        if (priority > 0.5) return '#ff9800'; // Med
        return '#4caf50'; // Low
    };

    return (
        <Card style={styles.card} mode="elevated">
            <Card.Content>
                <View style={styles.header}>
                    <View style={styles.titleRow}>
                        <Avatar.Icon size={24} icon="brain" style={{ backgroundColor: 'transparent' }} color={theme.colors.primary} />
                        <Text variant="titleMedium" style={styles.title}>Yapay Zeka Önerileri</Text>
                    </View>
                    <Chip size="small" icon="auto-awesome" textStyle={{ fontSize: 10 }}>Canlı</Chip>
                </View>

                {insights.slice(0, 3).map((item, index) => (
                    <List.Item
                        key={index}
                        title={item.title}
                        description={item.description}
                        titleStyle={{ fontSize: 14, fontWeight: 'bold' }}
                        descriptionStyle={{ fontSize: 12 }}
                        descriptionNumberOfLines={2}
                        left={props => (
                            <Avatar.Icon
                                {...props}
                                size={36}
                                icon={getIcon(item.recommendationType)}
                                style={{ backgroundColor: '#f5f5f5' }}
                                color={getPriorityColor(item.priority)}
                            />
                        )}
                        style={styles.listItem}
                    />
                ))}
            </Card.Content>
        </Card>
    );
};

const styles = StyleSheet.create({
    card: {
        marginBottom: 16,
        borderRadius: 16,
        backgroundColor: '#fff',
        borderLeftWidth: 4,
        borderLeftColor: '#673ab7'
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: 8
    },
    titleRow: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 8
    },
    title: {
        fontWeight: 'bold',
        color: '#333'
    },
    listItem: {
        paddingHorizontal: 0,
        paddingVertical: 4
    }
});

export default AIInsightsWidget;
