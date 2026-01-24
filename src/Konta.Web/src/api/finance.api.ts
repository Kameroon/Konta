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
            console.log(response.data);
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
    },

    /**
     * Récupère la liste des tiers (Clients/Fournisseurs/Entreprises).
     */
    async getTiers(): Promise<any[]> {
        console.log('[Finance API] Récupération de la liste des tiers...');
        const response = await http.get<ApiResponse<any[]>>('/api/finance-core/tiers');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        return [];
    },

    /**
     * Crée un nouveau tiers (Client/Fournisseur).
     */
    async createTier(tier: any): Promise<string> {
        const response = await http.post<ApiResponse<{ id: string }>>('/api/finance-core/tiers', tier);
        if (response.data.success && response.data.data) {
            return response.data.data.id;
        }
        throw new Error(response.data.message || 'Échec de la création du tiers');
    },

    /**
     * Met à jour les informations d'un tiers.
     */
    async updateTier(id: string, tier: any): Promise<void> {
        const response = await http.put<ApiResponse<void>>(`/api/finance-core/tiers/${id}`, tier);
        if (!response.data.success) {
            throw new Error(response.data.message || 'Échec de la mise à jour du tiers');
        }
    },

    /**
     * Supprime un tiers par son ID.
     */
    async deleteTier(id: string): Promise<void> {
        const response = await http.delete<ApiResponse<void>>(`/api/finance-core/tiers/${id}`);
        if (!response.data.success) {
            throw new Error(response.data.message || 'Échec de la suppression du tiers');
        }
    }
};
