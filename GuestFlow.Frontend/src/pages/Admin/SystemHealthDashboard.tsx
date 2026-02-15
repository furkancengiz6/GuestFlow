/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import React from 'react';
import {
    Box,
    Typography,
    Grid,
    Card,
    CardContent,
    Chip,
    LinearProgress,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Alert,
    Button,
    Divider
} from '@mui/material';
import {
    CheckCircle,
    Error,
    CloudQueue,
    Security,
    Storage,
    RestartAlt,
    Backup
} from '@mui/icons-material';
import { useQuery, useMutation } from '@tanstack/react-query';
import { systemService } from '../../services/systemService';

const SystemHealthDashboard: React.FC = () => {

    const { data: health, isLoading, refetch } = useQuery({
        queryKey: ['systemHealth'],
        queryFn: systemService.validateAll,
        refetchInterval: 30000 // Refresh every 30 seconds
    });

    const { data: vulnerabilities } = useQuery({
        queryKey: ['vulnerabilities'],
        queryFn: systemService.getVulnerabilities
    });

    const backupMutation = useMutation({
        mutationFn: systemService.createBackup,
        onSuccess: () => {
            alert('Backup created successfully');
        }
    });

    if (isLoading) return <Box sx={{ width: '100%', mt: 4 }}><LinearProgress /></Box>;

    return (
        <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Box>
                    <Typography variant="h4" gutterBottom sx={{ fontWeight: 'bold', color: 'primary.main' }}>
                        System Health & Security
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Real-time production readiness and integration connectivity status
                    </Typography>
                </Box>
                <Box sx={{ display: 'flex', gap: 2 }}>
                    <Button startIcon={<RestartAlt />} onClick={() => refetch()} variant="outlined">Re-Check</Button>
                    <Button startIcon={<Backup />} onClick={() => backupMutation.mutate()} variant="contained" color="secondary">Instant Backup</Button>
                </Box>
            </Box>

            {/* Connectivity Overview */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid item xs={12} md={4}>
                    <HealthCard
                        title="External Integrations"
                        subtitle="PMS, WhatsApp, Stripe"
                        icon={<CloudQueue color="primary" />}
                        success={health?.overallSuccess || false}
                        items={health?.secretsResult.items || []}
                    />
                </Grid>
                <Grid item xs={12} md={4}>
                    <HealthCard
                        title="Database & Data"
                        subtitle="Persistence layer health"
                        icon={<Storage color="info" />}
                        success={health?.databaseResult.success || false}
                        items={health?.databaseResult.items || []}
                    />
                </Grid>
                <Grid item xs={12} md={4}>
                    <HealthCard
                        title="Security & Secrets"
                        subtitle="JWT, CORS, SSL"
                        icon={<Security color="secondary" />}
                        success={health?.secretsResult.success || false}
                        items={health?.secretsResult.items.filter(i => i.name.includes('JWT') || i.name.includes('CORS')) || []}
                    />
                </Grid>
            </Grid>

            {/* Dependency Vulnerabilities */}
            <Card sx={{ mb: 4 }}>
                <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                        <Security color="error" />
                        <Typography variant="h6">Vulnerability Assessment</Typography>
                    </Box>
                    {vulnerabilities?.isClean ? (
                        <Alert severity="success">All dependencies (Backend & Frontend) are verified and clean.</Alert>
                    ) : (
                        <Box>
                            <Alert severity="warning" sx={{ mb: 2 }}>
                                Found {vulnerabilities?.highSeverityCount} high and {vulnerabilities?.mediumSeverityCount} medium vulnerabilities.
                            </Alert>
                            <List dense>
                                {vulnerabilities?.vulnerabilities.map((v, i) => (
                                    <ListItem key={i} divider={i !== vulnerabilities.vulnerabilities.length - 1}>
                                        <ListItemText
                                            primary={`${v.packageName} @ ${v.currentVersion}`}
                                            secondary={`${v.vulnerability} - Severity: ${v.severity}`}
                                        />
                                        <Chip size="small" label={v.severity} color={v.severity === 'High' ? 'error' : 'warning'} />
                                    </ListItem>
                                ))}
                            </List>
                        </Box>
                    )}
                </CardContent>
            </Card>

            {!health?.overallSuccess && (
                <Alert severity="error" sx={{ mb: 2 }} variant="filled">
                    System is NOT production ready. Please resolve the critical warnings above before deploying.
                </Alert>
            )}
        </Box>
    );
};

interface HealthCardProps {
    title: string;
    subtitle: string;
    icon: React.ReactNode;
    success: boolean;
    items: any[];
}

const HealthCard: React.FC<HealthCardProps> = ({ title, subtitle, icon, success, items }) => (
    <Card sx={{ height: '100%', borderTop: '4px solid', borderColor: success ? 'success.main' : 'error.main' }}>
        <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                {icon}
                <Typography variant="h6">{title}</Typography>
            </Box>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 2 }}>
                {subtitle}
            </Typography>

            <Divider sx={{ my: 1 }} />

            <List dense sx={{ maxHeight: 200, overflow: 'auto' }}>
                {items.map((item, idx) => (
                    <ListItem key={idx} disableGutters>
                        <ListItemIcon sx={{ minWidth: 32 }}>
                            {item.success ? <CheckCircle fontSize="small" color="success" /> : <Error fontSize="small" color="error" />}
                        </ListItemIcon>
                        <ListItemText
                            primary={item.name}
                            secondary={item.message || item.status}
                            primaryTypographyProps={{ variant: 'body2', fontWeight: item.severity === 'Critical' ? 'bold' : 'normal' }}
                        />
                    </ListItem>
                ))}
            </List>
        </CardContent>
    </Card>
);

export default SystemHealthDashboard;
