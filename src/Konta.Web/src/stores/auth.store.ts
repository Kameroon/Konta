import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authApi } from '@/api/auth.api';
import type { LoginRequest, UserInfo } from '@/types/auth.types';

/**
 * Store Pinia pour l'authentification.
 * Gère l'état de connexion, les tokens et les infos utilisateur.
 */
export const useAuthStore = defineStore('auth', () => {
    // --- État (State) ---
    const accessToken = ref<string | null>(localStorage.getItem('access_token'));
    const refreshToken = ref<string | null>(localStorage.getItem('refresh_token'));
    const user = ref<UserInfo | null>(JSON.parse(localStorage.getItem('user_info') || 'null'));
    const loading = ref(false);

    // --- Getters (Computed) ---
    const isAuthenticated = computed(() => !!accessToken.value);
    const currentUser = computed(() => user.value);
    const fullName = computed(() => user.value ? `${user.value.firstName} ${user.value.lastName}` : '');

    // --- Actions ---

    /**
     * Enregistre un nouvel utilisateur.
     * NE connecte PAS automatiquement - redirige vers login.
     */
    async function register(data: import('@/types/auth.types').RegisterRequest) {
        loading.value = true;
        try {
            console.log('[Auth Store] Inscription en cours...');
            await authApi.register(data);

            // On ne connecte plus automatiquement après inscription
            // L'utilisateur sera redirigé vers la page de login
            console.log('[Auth Store] Inscription réussie, redirection vers login...');
            return true;
        } catch (error) {
            console.error('[Auth Store] Erreur lors de l\'inscription', error);
            throw error;
        } finally {
            loading.value = false;
        }
    }

    /**
     * Connecte l'utilisateur et stocke les tokens.
     */
    async function login(credentials: LoginRequest) {
        loading.value = true;
        try {
            console.log('[Auth Store] Tentative de connexion...');
            const response = await authApi.login(credentials);

            // Mise à jour de l'état
            setAuthData(response.token, response.refreshToken, response.user);

            console.log('[Auth Store] Connexion réussie pour :', response.user.email);
            return response;
        } catch (error) {
            console.error('[Auth Store] Erreur lors de la connexion', error);
            throw error;
        } finally {
            loading.value = false;
        }
    }

    /**
     * Rafraîchit le token d'accès.
     */
    async function refreshAccessToken() {
        if (!refreshToken.value) {
            console.warn('[Auth Store] Aucun refresh token disponible.');
            logout();
            return null;
        }

        try {
            console.log('[Auth Store] Rafraîchissement du token en cours...');
            const response = await authApi.refreshToken({
                token: accessToken.value || '',
                refreshToken: refreshToken.value
            });

            // Mise à jour des tokens
            setAuthData(response.token, response.refreshToken, response.user);

            return response.token;
        } catch (error) {
            console.error('[Auth Store] Échec du rafraîchissement', error);
            logout();
            return null;
        }
    }

    /**
     * Déconnecte l'utilisateur et nettoie le stockage local.
     */
    function logout() {
        console.log('[Auth Store] Déconnexion locale.');

        // Nettoyage de l'état
        accessToken.value = null;
        refreshToken.value = null;
        user.value = null;

        // Nettoyage du localStorage
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('user_info');

        // Note : On pourrait appeler authApi.logout() ici si on veut invalider côté serveur
    }

    /**
     * Met à jour le profil de l'utilisateur.
     */
    async function updateProfile(data: import('@/types/auth.types').UpdateProfileRequest) {
        loading.value = true;
        try {
            const updatedUser = await authApi.updateProfile(data);

            // On conserve les tokens et on met à jour uniquement les infos user
            if (accessToken.value && refreshToken.value) {
                setAuthData(accessToken.value, refreshToken.value, updatedUser);
            }

            return updatedUser;
        } catch (error) {
            console.error('[Auth Store] Erreur lors de la mise à jour du profil', error);
            throw error;
        } finally {
            loading.value = false;
        }
    }

    /**
     * Helper pour mettre à jour l'état et le localStorage.
     */
    function setAuthData(at: string, rt: string, userInfo: UserInfo) {
        accessToken.value = at;
        refreshToken.value = rt;
        user.value = userInfo;

        localStorage.setItem('access_token', at);
        localStorage.setItem('refresh_token', rt);
        localStorage.setItem('user_info', JSON.stringify(userInfo));
    }

    return {
        accessToken,
        refreshToken,
        user,
        loading,
        isAuthenticated,
        currentUser,
        fullName,
        login,
        register,
        refreshAccessToken,
        logout,
        updateProfile
    };
});
