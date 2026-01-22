/**
 * Types et interfaces pour la gestion des utilisateurs, profils et tenants SaaS.
 */

/**
 * Informations détaillées sur un Tenant (entreprise).
 */
export interface Tenant {
    id: string;
    name: string;
    plan: 'Free' | 'Basic' | 'Premium' | 'Enterprise';
    createdAt: string;
    updatedAt?: string;
}

/**
 * Profil utilisateur complet (étendu).
 */
export interface UserProfile {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    roles: string[];
    tenantId: string;
    avatarUrl?: string;
    lastLogin?: string;
}

/**
 * Information sur l'abonnement SaaS.
 */
export interface SubscriptionInfo {
    isActive: boolean;
    expiresAt: string;
    quotaUsed: number;
    quotaLimit: number;
    features: string[];
}
