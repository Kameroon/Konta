import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { Tenant, SubscriptionInfo } from '@/types/user.types';
import http from '@/api/http';

/**
 * Tenant Store : Gère les informations de l'entreprise (Tenant) et l'abonnement SaaS.
 */
export const useTenantStore = defineStore('tenant', () => {
    // --- ÉTAT (State) ---
    const currentTenant = ref<Tenant | null>(null); // Le tenant actif pour l'utilisateur
    const subscription = ref<SubscriptionInfo | null>(null); // Détails de l'abonnement
    const loading = ref(false); // État de chargement du store

    // --- GETTERS ---
    const isPremium = computed(() => currentTenant.value?.plan === 'Premium' || currentTenant.value?.plan === 'Enterprise');
    const tenantName = computed(() => currentTenant.value?.name || 'Inconnu');

    // --- ACTIONS ---

    /**
     * Charge les informations du tenant depuis l'API Identity.
     */
    const fetchTenantInfo = async (tenantId: string) => {
        console.log(`[Tenant Store] Chargement des infos pour le tenant : ${tenantId}`);
        loading.value = true;
        try {
            // Dans une implémentation réelle, on appellerait /api/tenants/{id}
            const response = await http.get<{ data: Tenant; subscription?: SubscriptionInfo }>(`/api/tenants/${tenantId}`);
            currentTenant.value = response.data.data;

            // Si l'API renvoie les infos d'abonnement directement, on les stocke
            if (response.data.subscription) {
                subscription.value = response.data.subscription;
            } else {
                // Sinon on peut appeler un endpoint dédié ou laisser null si non disponible
                subscription.value = null;
            }

            console.log('[Tenant Store] Informations du tenant chargées avec succès');
        } catch (error) {
            console.error('[Tenant Store] Erreur lors du chargement du tenant', error);
            throw error;
        } finally {
            loading.value = false;
        }
    };

    return {
        currentTenant,
        subscription,
        loading,
        isPremium,
        tenantName,
        fetchTenantInfo
    };
});
