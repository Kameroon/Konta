import http from './http';
import type { ApiResponse } from '@/types/common.types';

export interface TenantResponse {
    id: string;
    name: string;
    plan: string;
    industry?: string;
    address?: string;
    createdAt: string;
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
    }
};
