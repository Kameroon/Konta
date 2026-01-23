import http from './http';
import type { UserInfo } from '@/types/auth.types';
import type { ApiResponse } from '@/types/common.types';

/**
 * Service API pour la gestion des utilisateurs par les administrateurs.
 */
export const userApi = {
    /**
     * Récupère tous les utilisateurs du tenant actuel.
     */
    async getUsers(): Promise<UserInfo[]> {
        console.log('[User API] Récupération de la liste des utilisateurs...');
        const response = await http.get<ApiResponse<UserInfo[]>>('/api/users');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        return [];
    },

    /**
     * Crée un nouvel utilisateur.
     */
    async createUser(user: Partial<UserInfo>): Promise<string> {
        const response = await http.post<ApiResponse<{ id: string }>>('/api/users', user);
        if (response.data.success && response.data.data) {
            return response.data.data.id;
        }
        throw new Error(response.data.message || 'Échec de la création');
    },

    /**
     * Met à jour un utilisateur.
     */
    async updateUser(id: string, user: Partial<UserInfo>): Promise<void> {
        const response = await http.put<ApiResponse<void>>(`/api/users/${id}`, user);
        if (!response.data.success) {
            throw new Error(response.data.message || 'Échec de la mise à jour');
        }
    },

    /**
     * Supprime un utilisateur.
     */
    async deleteUser(id: string): Promise<void> {
        const response = await http.delete<ApiResponse<void>>(`/api/users/${id}`);
        if (!response.data.success) {
            throw new Error(response.data.message || 'Échec de la suppression');
        }
    }
};
