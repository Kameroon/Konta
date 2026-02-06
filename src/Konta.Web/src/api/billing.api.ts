import axios from 'axios';
import type { ApiResponse } from '@/types/common.types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export interface SubscriptionPlan {
    id: string;
    code: string;
    name: string;
    description: string;
    price: number;
    currency: string;
    interval: string;
    maxUsers: number | null;
    storageGb: number | null;
    hasPrioritySupport: boolean;
    hasApiAccess: boolean;
    features: string | string[]; // Can be JSON string or parsed array
}

export const billingApi = {
    /**
     * Récupère la liste des plans d'abonnement actifs depuis le microservice de facturation.
     * Cet appel est public et utilisé pour l'affichage du catalogue des tarifs.
     * @returns {Promise<ApiResponse<SubscriptionPlan[]>>} La liste des plans structurés.
     */
    async getPlans(): Promise<ApiResponse<SubscriptionPlan[]>> {
        const response = await axios.get(`${API_BASE_URL}/api/billing/plans`);
        return response.data;
    },

    /**
     * Initialise une session de paiement Stripe.
     */
    async createCheckout(planCode: string, priceId?: string): Promise<ApiResponse<{ url: string }>> {
        const response = await axios.post(`${API_BASE_URL}/api/billing/checkout`, {
            planCode,
            priceId
        });
        return response.data;
    }
};
