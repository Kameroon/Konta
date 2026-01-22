import http from './http';
import type { FinancialSummary, DashboardKpi, CashFlowTrend } from '@/types/finance.types';
import type { ApiResponse } from '@/types/common.types';

/**
 * Service API pour les finances et le reporting.
 * Centralise les appels vers Konta.Finance et Konta.Reporting via la Gateway.
 */
export const financeApi = {
    /**
     * Récupère le résumé financier global pour le tableau de bord.
     */
    async getDashboardSummary(): Promise<FinancialSummary> {
        console.log('[Finance API] Récupération du résumé financier global...');
        const response = await http.get<ApiResponse<FinancialSummary>>('/api/reporting/dashboard/summary');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Erreur lors du chargement du résumé financier');
    },

    /**
     * Récupère la liste des KPIs principaux (Trésorerie, Ventes, etc.).
     */
    async getMainKpis(): Promise<DashboardKpi[]> {
        console.log('[Finance API] Récupération des KPIs cardinaux...');
        const response = await http.get<ApiResponse<DashboardKpi[]>>('/api/reporting/kpi/main');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        return []; // Retourne une liste vide par défaut si pas de données
    },

    /**
     * Récupère l'historique de trésorerie pour les graphiques.
     */
    async getCashFlowTrend(): Promise<CashFlowTrend[]> {
        console.log('[Finance API] Récupération des tendances de flux de trésorerie...');
        const response = await http.get<ApiResponse<CashFlowTrend[]>>('/api/reporting/dashboard/cashflow');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        return [];
    }
};
