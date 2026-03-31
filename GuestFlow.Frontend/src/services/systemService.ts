/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import api from './api';

export interface ValidationItem {
    name: string;
    success: boolean;
    status: string;
    severity: 'Info' | 'Warning' | 'Critical';
    message?: string;
}

export interface ProductionConfigurationValidationResult {
    overallSuccess: boolean;
    validationTime: string;
    environment: string;
    secretsResult: {
        success: boolean;
        items: ValidationItem[];
    };
    databaseResult: {
        success: boolean;
        items: ValidationItem[];
    };
    loggingResult: {
        success: boolean;
        items: ValidationItem[];
    };
}

export interface DependencyVulnerabilityResult {
    summary: string;
    highSeverityCount: number;
    mediumSeverityCount: number;
    isClean: boolean;
    vulnerabilities: Array<{
        packageName: string;
        currentVersion: string;
        vulnerability: string;
        severity: string;
        fixVersion?: string;
    }>;
}

export const systemService = {
    validateAll: async (): Promise<ProductionConfigurationValidationResult> => {
        const response = await api.get('/Production/validate');
        return response.data.data;
    },

    getVulnerabilities: async (): Promise<DependencyVulnerabilityResult> => {
        const response = await api.get('/Production/dependencies/vulnerabilities');
        return response.data.data;
    },

    getMigrations: async (): Promise<any> => {
        const response = await api.get('/Production/migrations');
        return response.data.data;
    },

    createBackup: async (): Promise<any> => {
        const response = await api.post('/Production/backup/create');
        return response.data.data;
    }
};
