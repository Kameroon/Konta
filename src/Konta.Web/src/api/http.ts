import axios, { type AxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '@/stores/auth.store';
import { useToast } from 'vue-toastification';

/**
 * Instance Axios configurée pour l'application Konta ERP.
 * Cette instance gère les appels vers l'API Gateway avec intercepteurs.
 */
const http: AxiosInstance = axios.create({
    // URL de base récupérée depuis les variables d'environnement (Vite)
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
    timeout: 10000, // Timeout de 10 secondes
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    },
});

const toast = useToast();

// Variable pour gérer le rafraîchissement du token et éviter les boucles infinies
let isRefreshing = false;
let failedQueue: Array<{
    resolve: (value: unknown) => void;
    reject: (reason?: any) => void;
}> = [];

/**
 * Traite la file d'attente des requêtes en échec pendant le rafraîchissement.
 */
const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });

    failedQueue = [];
};

/**
 * Intercepteur de requête : Ajoute le token JWT à l'en-tête Authorization
 */
http.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const authStore = useAuthStore();
        const token = authStore.accessToken;

        if (token) {
            // Ajout du token dans les headers si présent
            config.headers.Authorization = `Bearer ${token}`;
            console.log(`[HTTP Request] Ajout du token JWT à la requête : ${config.url}`);
        }

        return config;
    },
    (error: AxiosError) => {
        console.error('[HTTP Request Error]', error);
        return Promise.reject(error);
    }
);

/**
 * Intercepteur de réponse : Gère les erreurs 401 (Expire) et le rafraîchissement
 */
http.interceptors.response.use(
    (response) => {
        // Retourne la réponse si tout est ok
        return response;
    },
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
        const authStore = useAuthStore();

        // Cas 1 : Erreur 401 (Non autorisé) - Probablement Token expiré
        if (error.response?.status === 401 && !originalRequest._retry) {
            console.warn(`[HTTP Response] Erreur 401 sur ${originalRequest.url}. Tentative de rafraîchissement...`);

            if (isRefreshing) {
                // En file d'attente si un rafraîchissement est déjà en cours
                return new Promise(function (resolve, reject) {
                    failedQueue.push({ resolve, reject });
                })
                    .then((token) => {
                        originalRequest.headers.Authorization = 'Bearer ' + token;
                        return http(originalRequest);
                    })
                    .catch((err) => {
                        return Promise.reject(err);
                    });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
                // Tentative de rafraîchissement du token via le store
                const newToken = await authStore.refreshAccessToken();

                if (newToken) {
                    console.log('[HTTP Response] Token rafraîchi avec succès. Relance de la requête initiale.');
                    processQueue(null, newToken);
                    originalRequest.headers.Authorization = `Bearer ${newToken}`;
                    return http(originalRequest);
                }
            } catch (refreshError) {
                console.error('[HTTP Response] Échec du rafraîchissement du token.', refreshError);
                processQueue(refreshError, null);

                // Déconnexion forcée si le refresh échoue
                authStore.logout();
                toast.error('Votre session a expiré. Veuillez vous reconnecter.');

                return Promise.reject(refreshError);
            } finally {
                isRefreshing = false;
            }
        }

        // Cas 2 : Erreur 403 (Interdit / Droits insuffisants)
        if (error.response?.status === 403) {
            console.error('[HTTP Response] Accès interdit (403)');
            toast.error("Vous n'avez pas les droits nécessaires pour effectuer cette action.");
        }

        // Cas 3 : Erreurs serveurs (500+)
        if (error.response?.status && error.response.status >= 500) {
            console.error(`[HTTP Response] Erreur serveur (${error.response.status})`);
            toast.error('Une erreur serveur est survenue. Veuillez réessayer plus tard.');
        }

        // Cas 4 : Pas de réponse du serveur
        if (!error.response) {
            console.error('[HTTP Response] Le serveur ne répond pas.');
            toast.error('Impossible de contacter le serveur. Vérifiez votre connexion.');
        }

        return Promise.reject(error);
    }
);

export default http;
