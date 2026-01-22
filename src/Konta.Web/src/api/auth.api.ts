import http from './http';
import type { LoginRequest, TokenResponse, RefreshRequest, UserInfo, RegisterRequest } from '@/types/auth.types';
import type { ApiResponse } from '@/types/common.types';

/**
 * Service API pour l'authentification.
 * Regroupe tous les appels vers le microservice Identity via la Gateway.
 */
export const authApi = {
    /**
     * Enregistre un nouveau tenant et son administrateur.
     */
    async register(data: RegisterRequest): Promise<any> {
        console.log(`[Auth API] Tentative d'inscription pour : ${data.email}`);
        const response = await http.post<ApiResponse<any>>('/api/auth/register', data);

        if (response.data.success) {
            return response.data;
        }
        throw new Error(response.data.message || 'Échec de l\'inscription');
    },

    /**
     * Connecte l'utilisateur avec ses identifiants.
     * @param credentials E-mail et mot de passe.
     */
    async login(credentials: LoginRequest): Promise<TokenResponse> {
        console.log(`[Auth API] Tentative de connexion pour : ${credentials.email}`);
        const response = await http.post<ApiResponse<TokenResponse>>('/api/auth/login', credentials);

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Échec de connexion');
    },

    /**
     * Rafraîchit le token d'accès en utilisant le refresh token.
     * @param data Payload contenant le token actuel et le refresh token.
     */
    async refreshToken(data: RefreshRequest): Promise<TokenResponse> {
        console.log('[Auth API] Requête de rafraîchissement du token envoyée.');
        const response = await http.post<ApiResponse<TokenResponse>>('/api/auth/refresh', data);

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Échec du rafraîchissement');
    },

    /**
     * Déconnecte l'utilisateur du serveur.
     */
    async logout(): Promise<void> {
        console.log('[Auth API] Demande de déconnexion serveur.');
        await http.post('/api/auth/logout');
    },

    /**
     * Récupère les informations de l'utilisateur connecté.
     */
    async getUserInfo(): Promise<UserInfo> {
        console.log('[Auth API] Récupération des infos utilisateur.');
        const response = await http.get<ApiResponse<UserInfo>>('/api/auth/me');

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Impossible de récupérer les infos utilisateur');
    },

    /**
     * Met à jour le profil de l'utilisateur.
     * @param data Prénom et nom.
     */
    async updateProfile(data: import('@/types/auth.types').UpdateProfileRequest): Promise<UserInfo> {
        console.log('[Auth API] Mise à jour du profil utilisateur.');
        const response = await http.put<ApiResponse<UserInfo>>('/api/auth/profile', data);

        if (response.data.success && response.data.data) {
            return response.data.data;
        }
        throw new Error(response.data.message || 'Échec de la mise à jour du profil');
    }
};
