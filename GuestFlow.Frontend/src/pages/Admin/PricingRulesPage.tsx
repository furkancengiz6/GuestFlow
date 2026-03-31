/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import { useState } from 'react'
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
    IconButton,
    Switch,
    Paper,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    MenuItem,
    CircularProgress,
} from '@mui/material'
import {
    Add,
    Edit,
    Rule,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { pricingService } from '../../services/pricingService'

const PricingRulesPage = () => {
    const [open, setOpen] = useState(false)
    const { data: rules, isLoading } = useQuery({
        queryKey: ['pricingRules'],
        queryFn: pricingService.getRules,
    })

    if (isLoading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}>
                <CircularProgress />
            </Box>
        )
    }

    return (
        <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h4" sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Rule fontSize="large" color="primary" />
                    Dynamic Pricing Rules
                </Typography>
                <Button variant="contained" startIcon={<Add />} onClick={() => setOpen(true)}>
                    Create New Rule
                </Button>
            </Box>

            <Grid container spacing={3}>
                <Grid item xs={12}>
                    <TableContainer component={Paper}>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>Rule Name</TableCell>
                                    <TableCell>Type</TableCell>
                                    <TableCell>Adjustment</TableCell>
                                    <TableCell>Value</TableCell>
                                    <TableCell>Priority</TableCell>
                                    <TableCell>Status</TableCell>
                                    <TableCell align="right">Actions</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {rules?.map((rule) => (
                                    <TableRow key={rule.id} hover>
                                        <TableCell fontWeight="bold">{rule.name}</TableCell>
                                        <TableCell>
                                            <Chip label={rule.ruleType} size="small" variant="outlined" />
                                        </TableCell>
                                        <TableCell>{rule.adjustmentType}</TableCell>
                                        <TableCell color={rule.adjustmentValue > 0 ? 'error.main' : 'success.main'}>
                                            {rule.adjustmentValue > 0 ? '+' : ''}{rule.adjustmentValue}
                                            {rule.adjustmentType === 'Percentage' ? '%' : ' TRY'}
                                        </TableCell>
                                        <TableCell>{rule.priority}</TableCell>
                                        <TableCell>
                                            <Switch checked={rule.isActive} size="small" />
                                        </TableCell>
                                        <TableCell align="right">
                                            <IconButton size="small"><Edit /></IconButton>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                </Grid>
            </Grid>

            {/* Simplified Create Dialog */}
            <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Create Pricing Rule</DialogTitle>
                <DialogContent sx={{ pt: 2 }}>
                    <TextField fullWidth label="Rule Name" sx={{ mb: 2 }} />
                    <Grid container spacing={2}>
                        <Grid item xs={6}>
                            <TextField select fullWidth label="Rule Type">
                                <MenuItem value="Occupancy">Occupancy Based</MenuItem>
                                <MenuItem value="Seasonal">Seasonal</MenuItem>
                                <MenuItem value="LastMinute">Last Minute</MenuItem>
                            </TextField>
                        </Grid>
                        <Grid item xs={6}>
                            <TextField select fullWidth label="Adjustment Type">
                                <MenuItem value="Percentage">Percentage</MenuItem>
                                <MenuItem value="FixedAmount">Fixed Amount</MenuItem>
                            </TextField>
                        </Grid>
                        <Grid item xs={6}>
                            <TextField fullWidth label="Value" type="number" />
                        </Grid>
                        <Grid item xs={6}>
                            <TextField fullWidth label="Priority" type="number" />
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpen(false)}>Cancel</Button>
                    <Button variant="contained">Save Rule</Button>
                </DialogActions>
            </Dialog>
        </Box>
    )
}

export default PricingRulesPage
