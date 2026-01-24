/**
 * Types et interfaces pour les données financières (Finance & Reporting).
 * Correspondant aux modèles C# du backend.
 */

export enum TierType {
    Client = 'Client',
    Supplier = 'Supplier'
}

/**
 * Représente un tiers (Client ou Fournisseur).
 */
export interface Tier {
    id: string;
    tenantId: string;
    name: string;
    type: TierType;
    email?: string;
    taxId?: string;
    address?: string;
    isActive: boolean;
    createdAt: string;
}

/**
 * Résumé financier agrégé pour le tableau de bord.
 * @see FinancialSummary in Konta.Reporting
 */
export interface FinancialSummary {
    id: string;
    tenantId: string;
    totalRevenue: number;
    totalExpenses: number;
    grossMargin: number;
    profitabilityPercentage: number;
    period: string;
}

/**
 * KPI Individuel pour affichage sous forme de carte.
 */
export interface DashboardKpi {
    id: string;
    label: string;
    value: number;
    previousValue: number;
    trend: 'up' | 'down' | 'stable';
    format: 'currency' | 'percentage' | 'number';
    color: string;
}

/**
 * Tendance de trésorerie.
 */
export interface CashFlowTrend {
    period: string;
    inflow: number;
    outflow: number;
    balance: number;
}
