import http from './http';
import type { ApiResponse } from '@/types/common.types';

export interface TenantResponse {
    id: string;
    name: string;
    plan: string;
    industry?: string;
    address?: string;
    siret?: string;
    createdAt: string;
    updatedAt?: string;
}

export const tenantApi = {
    /**
     * Récupère la liste de tous les tenants (réservé aux admins).
     */
    async getTenants(): Promise<TenantResponse[]> {
        const response = await http.get<ApiResponse<TenantResponse[]>>('/api/tenants');
        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Erreur lors de la récupération des tenants');
    },
    /**
     * Met à jour le plan d'un tenant.
     */
    async updateTenantPlan(tenantId: string, plan: string): Promise<boolean> {
        const response = await http.patch<ApiResponse<any>>(`/api/tenants/${tenantId}/plan`, JSON.stringify(plan), {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data.success;
    },

    /**
     * Met à jour les informations d'un tenant.
     */
    async updateTenant(tenantId: string, data: Partial<TenantResponse>): Promise<boolean> {
        const response = await http.put<ApiResponse<any>>(`/api/tenants/${tenantId}`, data);
        return response.data.success;
    },

    /**
     * Supprime un tenant (attention : opération critique).
     */
    async deleteTenant(tenantId: string): Promise<boolean> {
        const response = await http.delete<ApiResponse<any>>(`/api/tenants/${tenantId}`);
        return response.data.success;
    }
};
