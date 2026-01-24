import http from './http';
import type { ApiResponse } from '@/types/common.types';

export interface ChartDataPoint {
    label: string;
    value: number;
}

export interface DashboardSummary {
    totalDocuments: number;
    totalCompanies: number;
    totalUsers: number;
    totalRevenue: number;
    documentsTrend: number;
    companiesTrend: number;
    revenueTrend: number;
    monthlyDocuments: ChartDataPoint[];
    monthlyRevenue: ChartDataPoint[];
    documentTypes: ChartDataPoint[];
}

/**
 * Service API pour le reporting et les KPIs.
 */
export const reportingApi = {
    /**
     * Récupère le résumé complet des statistiques pour le dashboard.
     */
    async getDashboardSummary(): Promise<DashboardSummary> {
        const response = await http.get<ApiResponse<DashboardSummary>>('/api/reporting/dashboard/summary');
        if (response.data.success && response.data.data) {
            console.log(response.data);
            return response.data.data;
        }
        throw new Error(response.data.message || 'Impossible de charger les statistiques');
    }
};
