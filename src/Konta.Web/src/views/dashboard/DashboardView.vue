<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { financeApi } from '@/api/finance.api';
import type { DashboardKpi, FinancialSummary } from '@/types/finance.types';
import StatCard from '@/components/ui/StatCard.vue';
import LoadingOverlay from '@/components/ui/LoadingOverlay.vue';
import ErrorState from '@/components/ui/ErrorState.vue';

/**
 * DashboardView : Cœur financier de l'application.
 * Affiche les KPIs en temps réel via le microservice Reporting.
 */

const authStore = useAuthStore();
const kpis = ref<DashboardKpi[]>([]);
const summary = ref<FinancialSummary | null>(null);
const loading = ref(true);
const error = ref<string | null>(null);

/**
 * Charge les données depuis les APIs financières.
 */
const fetchData = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    console.log('[Dashboard] Initialisation du chargement des données financières...');
    
    // Appels concurrents pour optimiser le temps de chargement
    const [kpiData, summaryData] = await Promise.all([
      financeApi.getMainKpis(),
      financeApi.getDashboardSummary()
    ]);

    kpis.value = kpiData;
    summary.value = summaryData;
    
    console.log('[Dashboard] Données récupérées avec succès.');
  } catch (err: any) {
    console.error('[Dashboard] Erreur lors du chargement des données', err);
    error.value = "Impossible de récupérer les indicateurs financiers. Vérifiez la connexion aux microservices.";
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>

<template>
  <div class="dashboard-view">
    <!-- En-tête du Dashboard -->
    <header class="dashboard-header">
      <div class="welcome">
        <h1>Dashboard Financier</h1>
        <p>Situation de <strong>{{ authStore.user?.tenantId }}</strong> pour la période {{ summary?.period || 'actuelle' }}</p>
      </div>
      
      <div class="actions">
        <button @click="fetchData" class="refresh-btn" :disabled="loading">
          🔄 Actualiser
        </button>
      </div>
    </header>

    <!-- Zone de contenu avec gestion d'état -->
    <div class="dashboard-content">
      <LoadingOverlay :active="loading" message="Calcul des indicateurs financiers..." />
      
      <ErrorState v-if="error" :error="error" :retry="fetchData" />

      <template v-else-if="!loading">
        <!-- Grille des KPIs Cardinaux -->
        <div class="kpi-grid">
          <StatCard 
            v-for="kpi in kpis" 
            :key="kpi.id"
            :label="kpi.label"
            :value="kpi.value"
            :trend="kpi.trend"
            :trend-value="Math.abs(((kpi.value - kpi.previousValue) / kpi.previousValue) * 100)"
            :format="kpi.format"
            :color="kpi.color"
            icon="💰"
          />
        </div>

        <!-- Section Analyse de Marge -->
        <div v-if="summary" class="summary-section">
          <div class="summary-card">
            <h3>Analyse de la rentabilité ({{ summary.period }})</h3>
            <div class="summary-stats">
              <div class="progress-item">
                <div class="progress-info">
                  <span>Marge brute</span>
                  <strong>{{ (summary.profitabilityPercentage).toFixed(1) }}%</strong>
                </div>
                <div class="progress-bar">
                  <div class="fill" :style="{ width: summary.profitabilityPercentage + '%', backgroundColor: '#42b883' }"></div>
                </div>
              </div>
              
              <div class="stat-details">
                <div class="detail">
                  <span class="dot revenue"></span>
                  <label>Revenus :</label>
                  <span>{{ new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(summary.totalRevenue) }}</span>
                </div>
                <div class="detail">
                  <span class="dot expenses"></span>
                  <label>Dépenses :</label>
                  <span>{{ new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(summary.totalExpenses) }}</span>
                </div>
              </div>
            </div>
          </div>
          
          <div class="info-card">
            <h3>💡 Conseil Expert</h3>
            <p v-if="summary.profitabilityPercentage > 20">
              Votre rentabilité est excellente ce mois-ci. C'est le moment idéal pour envisager de nouveaux investissements ou renforcer votre trésorerie.
            </p>
            <p v-else>
              Votre marge est sous pression. Nous vous conseillons de vérifier vos factures d'achats récentes dans le module OCR pour identifier des hausses de coûts.
            </p>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.welcome h1 {
  font-size: 2.2rem;
  color: #1e293b;
  font-weight: 800;
  letter-spacing: -1px;
}

.welcome p {
  color: #64748b;
  font-size: 1.1rem;
}

.refresh-btn {
  padding: 0.8rem 1.2rem;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-weight: 600;
  color: #475569;
  cursor: pointer;
  transition: all 0.2s;
}

.refresh-btn:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.dashboard-content {
  position: relative;
  min-height: 400px;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2.5rem;
}

.summary-section {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 1.5rem;
}

.summary-card, .info-card {
  background: white;
  padding: 2rem;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
}

.summary-card h3, .info-card h3 {
  margin-bottom: 1.5rem;
  font-size: 1.1rem;
  color: #1e293b;
}

.progress-item {
  margin-bottom: 2rem;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.8rem;
}

.progress-bar {
  height: 12px;
  background: #f1f5f9;
  border-radius: 6px;
  overflow: hidden;
}

.fill {
  height: 100%;
  transition: width 1s ease-out;
}

.stat-details {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.detail {
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.dot.revenue { background-color: #42b883; }
.dot.expenses { background-color: #ef4444; }

.detail label {
  color: #64748b;
  font-weight: 500;
}

.info-card {
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
}

.info-card p {
  line-height: 1.6;
  color: #166534;
}

@media (max-width: 1024px) {
  .summary-section {
    grid-template-columns: 1fr;
  }
}
</style>
