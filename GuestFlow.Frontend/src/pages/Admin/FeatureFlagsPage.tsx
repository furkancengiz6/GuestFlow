/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import {
    Box,
    Typography,
    Grid,
    Button,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Chip,
    Switch,
    Paper,
    IconButton,
} from '@mui/material'
import {
    Flag,
    Delete,
    SettingsSuggest,
} from '@mui/icons-material'
import { useQuery, useMutation } from '@tanstack/react-query'
import { featureFlagService } from '../../services/featureFlagService'

const FeatureFlagsPage = () => {
    const { data: flags, isLoading, refetch } = useQuery({
        queryKey: ['featureFlags'],
        queryFn: featureFlagService.getAll,
    })

    const toggleMutation = useMutation({
        mutationFn: async ({ name, enable }: { name: string, enable: boolean }) => {
            return enable ? featureFlagService.enable(name) : featureFlagService.disable(name)
        },
        onSuccess: () => refetch(),
    })

    const deleteMutation = useMutation({
        mutationFn: featureFlagService.delete,
        onSuccess: () => refetch(),
    })

    if (isLoading) return <Box sx={{ p: 5, textAlign: 'center' }}>Loading Flags...</Box>

    return (
        <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h4" sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Flag fontSize="large" color="primary" />
                    Feature Management
                </Typography>
                <Button variant="contained" startIcon={<SettingsSuggest />}>
                    Register Feature
                </Button>
            </Box>

            <Grid container spacing={3}>
                <Grid item xs={12}>
                    <TableContainer component={Paper}>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>Feature Name</TableCell>
                                    <TableCell>Environment</TableCell>
                                    <TableCell>Description</TableCell>
                                    <TableCell>Activation</TableCell>
                                    <TableCell align="right">Actions</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {flags?.map((flag) => (
                                    <TableRow key={flag.name} hover>
                                        <TableCell>
                                            <Typography variant="subtitle2" fontWeight="bold">{flag.name}</Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Chip label={flag.environment || 'Production'} size="small" color="primary" variant="outlined" />
                                        </TableCell>
                                        <TableCell>{flag.description || 'No description provided.'}</TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <Switch
                                                    checked={flag.isEnabled}
                                                    onChange={(e) => toggleMutation.mutate({ name: flag.name, enable: e.target.checked })}
                                                    disabled={toggleMutation.isPending}
                                                />
                                                <Chip
                                                    label={flag.isEnabled ? 'Active' : 'Disabled'}
                                                    size="small"
                                                    color={flag.isEnabled ? 'success' : 'default'}
                                                />
                                            </Box>
                                        </TableCell>
                                        <TableCell align="right">
                                            <IconButton
                                                size="small"
                                                color="error"
                                                onClick={() => deleteMutation.mutate(flag.name)}
                                                disabled={deleteMutation.isPending}
                                            >
                                                <Delete />
                                            </IconButton>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                </Grid>
            </Grid>
        </Box>
    )
}

export default FeatureFlagsPage
