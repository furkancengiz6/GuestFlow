/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import React, { useMemo } from 'react';
import { styled } from '@mui/material/styles';
import { Box, Typography } from '@mui/material';
import { Psychology, Person, Star } from '@mui/icons-material';
import { RelationshipNetwork } from '../../services/intelligenceService';

interface RelationshipNetworkGraphProps {
    network: RelationshipNetwork;
}

const StyledCircle = styled('circle')(() => ({
    cursor: 'pointer',
    transition: 'all 0.3s ease',
    '&:hover': {
        filter: 'brightness(1.2)',
    }
}));

const StyledForeignObject = styled('foreignObject')({
    pointerEvents: 'none',
});

const StyledText = styled('text')(({ theme }) => ({
    fontSize: '12px',
    fontWeight: 'bold',
    pointerEvents: 'none',
    fill: theme.palette.text.primary,
}));

export const RelationshipNetworkGraph: React.FC<RelationshipNetworkGraphProps> = ({ network }) => {
    const { guestNode, staffNodes, serviceNodes, edges } = network;

    // Simple circle-based layout calculation
    const centerX = 400;
    const centerY = 300;
    const radius = 220;

    const nodes = useMemo(() => {
        const allNodes = [
            { ...guestNode, x: centerX, y: centerY, type: 'guest' },
            ...staffNodes.map((s, i) => {
                const angle = (i / (staffNodes.length + serviceNodes.length)) * 2 * Math.PI;
                return {
                    ...s,
                    x: centerX + radius * Math.cos(angle),
                    y: centerY + radius * Math.sin(angle),
                    type: 'staff'
                };
            }),
            ...serviceNodes.map((s, i) => {
                const angle = ((i + staffNodes.length) / (staffNodes.length + serviceNodes.length)) * 2 * Math.PI;
                return {
                    ...s,
                    x: centerX + radius * Math.cos(angle),
                    y: centerY + radius * Math.sin(angle),
                    type: 'service'
                };
            })
        ];
        return allNodes;
    }, [guestNode, staffNodes, serviceNodes]);

    return (
        <Box sx={{ width: '100%', height: 600, position: 'relative', bgcolor: 'background.paper', borderRadius: 2, overflow: 'hidden', border: '1px solid', borderColor: 'divider' }}>
            <svg width="100%" height="100%" viewBox="0 0 800 600">
                <defs>
                    <linearGradient id="edgeGradient" x1="0%" y1="0%" x2="100%" y2="100%">
                        <stop offset="0%" stopColor="#2196f3" stopOpacity="0.6" />
                        <stop offset="100%" stopColor="#9c27b0" stopOpacity="0.2" />
                    </linearGradient>
                    <filter id="glow" x="-20%" y="-20%" width="140%" height="140%">
                        <feGaussianBlur stdDeviation="3" result="blur" />
                        <feComposite in="SourceGraphic" in2="blur" operator="over" />
                    </filter>
                </defs>

                {/* Connections (Edges) */}
                {edges.map((edge, i) => {
                    const source = nodes.find(n => n.id === edge.sourceId);
                    const target = nodes.find(n => n.id === edge.targetId);
                    if (!source || !target) return null;

                    return (
                        <line
                            key={`edge-${i}`}
                            x1={source.x}
                            y1={source.y}
                            x2={target.x}
                            y2={target.y}
                            stroke="url(#edgeGradient)"
                            strokeWidth={edge.weight * 3 || 1}
                            strokeDasharray={edge.relationshipType === 'Suggested' ? '5,5' : 'none'}
                            opacity={0.6}
                        />
                    );
                })}

                {/* Nodes */}
                {nodes.map((node) => (
                    <g key={node.id} transform={`translate(${node.x}, ${node.y})`}>
                        <StyledCircle
                            r={node.type === 'guest' ? 45 : 35}
                            fill={node.type === 'guest' ? '#1976d2' : node.type === 'staff' ? '#4caf50' : '#ff9800'}
                            filter="url(#glow)"
                        />
                        <StyledForeignObject x="-30" y="-30" width="60" height="60">
                            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', color: 'white' }}>
                                {node.type === 'guest' && <Psychology sx={{ fontSize: 35 }} />}
                                {node.type === 'staff' && <Person sx={{ fontSize: 25 }} />}
                                {node.type === 'service' && <Star sx={{ fontSize: 25 }} />}
                            </Box>
                        </StyledForeignObject>

                        {/* Labels */}
                        <StyledText
                            y={node.type === 'guest' ? 65 : 55}
                            textAnchor="middle"
                        >
                            {node.name}
                        </StyledText>
                    </g>
                ))}
            </svg>

            {/* Legend */}
            <Box sx={{ position: 'absolute', bottom: 16, right: 16, p: 2, bgcolor: 'rgba(255,255,255,0.8)', borderRadius: 1, border: '1px solid divider', backdropFilter: 'blur(4px)' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                    <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: '#1976d2' }} />
                    <Typography variant="caption">Guest (Anchor)</Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                    <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: '#4caf50' }} />
                    <Typography variant="caption">Staff Affinity</Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: '#ff9800' }} />
                    <Typography variant="caption">Service Context</Typography>
                </Box>
            </Box>
        </Box>
    );
};
