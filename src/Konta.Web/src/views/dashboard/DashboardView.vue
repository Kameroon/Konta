<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { useTenantStore } from '@/stores/tenant.store';
import { reportingApi, type DashboardSummary, type ChartDataPoint } from '@/api/reporting.api';
import StatCard from '@/components/ui/StatCard.vue';

const authStore = useAuthStore();
const tenantStore = useTenantStore();

const isSuperAdmin = computed(() => authStore.user?.role === 'SuperAdmin');

const summary = ref<DashboardSummary>({
  totalDocuments: 0,
  totalCompanies: 0,
  totalUsers: 0,
  totalRevenue: 0,
  documentsTrend: 0,
  companiesTrend: 0,
  revenueTrend: 0,
  monthlyDocuments: [],
  monthlyRevenue: [],
  documentTypes: []
});

const loading = ref(true);

const calculatePercentage = (value: number, all: ChartDataPoint[]) => {
  const total = all.reduce((acc, curr) => acc + Number(curr.value), 0);
  return total > 0 ? Math.round((Number(value) / total) * 100) : 0;
};

const getChartColor = (index: number) => {
  const colors = ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899'];
  return colors[index % colors.length];
};

const generateDonutGradient = (data: ChartDataPoint[]) => {
  if (data.length === 0) return '#e2e8f0';
  let lastPos = 0;
  const total = data.reduce((acc, curr) => acc + Number(curr.value), 0);
  if (total === 0) return '#e2e8f0';

  const parts = data.map((item, index) => {
    const percentage = (Number(item.value) / total) * 100;
    const start = lastPos;
    lastPos += percentage;
    return `${getChartColor(index)} ${start}% ${lastPos}%`;
  });
  return `conic-gradient(${parts.join(', ')})`;
};

const activities = ref([
  { id: 1, type: 'document', text: 'Nouveau document traité : contrats_clients_2024.pdf', time: 'Il y a 2 heures', icon: 'fas fa-file-pdf', color: '#3b82f6' },
  { id: 2, type: 'user', text: 'Marie Martin s\'est connectée', time: 'Il y a 3 heures', icon: 'fas fa-user', color: '#10b981' },
  { id: 3, type: 'company', text: 'Nouvelle entreprise ajoutée : Innovation Labs', time: 'Il y a 5 heures', icon: 'fas fa-building', color: '#a855f7' },
  { id: 4, type: 'extraction', text: '13 enregistrements extraits de factures_janvier.xlsx', time: 'Il y a 1 jour', icon: 'fas fa-database', color: '#f59e0b' },
]);

onMounted(async () => {
  try {
    summary.value = await reportingApi.getDashboardSummary();
  } catch (err) {
    console.error('Erreur dashboard:', err);
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div class="dashboard-view">
    <header class="dashboard-header">
      <div class="welcome">
        <h1>{{ isSuperAdmin ? 'Tableau de bord SuperAdmin' : ('Bienvenue, ' + authStore.user?.firstName) }}</h1>
        <p>Voici un aperçu de votre activité sur {{ isSuperAdmin ? 'Konta Platform' : (tenantStore.currentTenant?.name || 'votre entreprise') }}</p>
      </div>
    </header>

    <!-- Stats Grid -->
    <div class="stats-grid">
      <StatCard label="Documents" :value="summary.totalDocuments" trend="up" :trendValue="summary.documentsTrend" icon="fas fa-file-alt" color="#3b82f6" />
      
      <template v-if="isSuperAdmin">
        <StatCard label="Entreprises" :value="summary.totalCompanies" trend="up" :trendValue="summary.companiesTrend" icon="fas fa-building" color="#10b981" />
        <StatCard label="Utilisateurs" :value="summary.totalUsers" icon="fas fa-users" color="#a855f7" />
      </template>

      <StatCard label="Chiffre d'affaires" :value="summary.totalRevenue" format="currency" trend="up" :trendValue="summary.revenueTrend" icon="fas fa-euro-sign" color="#f59e0b" />
    </div>

    <!-- Middle Row: Charts -->
    <div class="charts-row" v-if="!loading">
      <div class="chart-card bar-chart">
        <div class="card-header">
          <h3>Documents par mois</h3>
        </div>
        <div class="chart-placeholder bar-placeholder">
          <div v-for="point in summary.monthlyDocuments" :key="point.label" class="bar" 
               :style="{ height: (point.value / (Math.max(...summary.monthlyDocuments.map(p => p.value)) || 1) * 100) + '%' }">
            <span>{{ point.label }}</span>
          </div>
          <div v-if="summary.monthlyDocuments.length === 0" class="empty-chart">Aucune donnée</div>
        </div>
      </div>
      
      <div class="chart-card pie-chart">
        <div class="card-header">
          <h3>Types de documents</h3>
        </div>
        <div class="chart-placeholder pie-placeholder">
          <div class="donut" :style="{ background: generateDonutGradient(summary.documentTypes) }">
            <div class="donut-inner"></div>
          </div>
          <div class="pie-legend">
            <div v-for="(item, index) in summary.documentTypes" :key="item.label" class="legend-item">
              <span class="dot" :style="{ background: getChartColor(index) }"></span> 
              {{ item.label }} {{ calculatePercentage(item.value, summary.documentTypes) }}%
            </div>
            <div v-if="summary.documentTypes.length === 0" class="empty-legend">Aucune donnée</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Long Chart Row -->
    <div class="chart-card line-chart" v-if="!loading">
      <div class="card-header">
        <h3>Évolution du chiffre d'affaires</h3>
      </div>
      <div class="chart-placeholder area-placeholder">
        <div class="revenue-grid">
            <div v-for="point in summary.monthlyRevenue" :key="point.label" class="rev-point" 
                 :style="{ height: (point.value / (Math.max(...summary.monthlyRevenue.map(p => p.value)) || 1) * 80 + 10) + '%' }">
                <div class="point-glow"></div>
                <span class="p-val">{{ (point.value / 1000).toFixed(1) }}k</span>
                <span class="p-label">{{ point.label }}</span>
            </div>
            <div v-if="summary.monthlyRevenue.length === 0" class="empty-chart">Aucun revenu disponible</div>
        </div>
        <div class="area-curve"></div>
      </div>
    </div>

    <!-- Bottom Row -->
    <!--<div class="bottom-grid">
      <div class="actions-section">
        <h3>Actions rapides</h3>
        <div class="actions-grid">
          <div class="action-card">
            <div class="icon-box blue"><i class="fas fa-upload"></i></div>
            <div class="text">
              <h4>Télécharger un document</h4>
              <p>Ajouter un nouveau document à traiter</p>
            </div>
          </div>
          <div class="action-card">
            <div class="icon-box green"><i class="fas fa-file-invoice"></i></div>
            <div class="text">
              <h4>Voir les documents</h4>
              <p>Consulter tous vos documents</p>
            </div>
          </div>
          <div class="action-card">
            <div class="icon-box purple"><i class="fas fa-database"></i></div>
            <div class="text">
              <h4>Données extraites</h4>
              <p>Analyser les données extraites</p>
            </div>
          </div>
          <div class="action-card">
            <div class="icon-box orange"><i class="fas fa-building"></i></div>
            <div class="text">
              <h4>Gérer les entreprises</h4>
              <p>Ajouter ou modifier des entreprises</p>
            </div>
          </div>
        </div>
      </div>

      <div class="activity-section">
        <h3>Activités récentes</h3>
        <div class="activity-list">
          <div v-for="act in activities" :key="act.id" class="activity-item">
            <div class="act-icon" :style="{ color: act.color, backgroundColor: act.color + '15' }">
              <i :class="act.icon"></i>
            </div>
            <div class="act-details">
              <p class="act-text">{{ act.text }}</p>
              <span class="act-time">{{ act.time }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>-->
  </div>
</template>

<style scoped>
.dashboard-view {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.dashboard-header .welcome p {
  color: #64748b;
  font-size: 1rem;
  margin: 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.5rem;
}

.charts-row {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 1.5rem;
}

.chart-card {
  background: white;
  padding: 1.5rem;
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.chart-card h3 {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 1.5rem 0;
}

/* Bar Chart Mockup */
.bar-placeholder {
  height: 250px;
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  padding: 0 1rem;
  gap: 10px;
}

.bar {
  flex: 1;
  background: #3b82f6;
  border-radius: 4px 4px 0 0;
  position: relative;
  min-width: 30px;
  transition: opacity 0.2s;
}

.bar:nth-child(even) { background: #10b981; }

.bar span {
  position: absolute;
  bottom: -25px;
  left: 50%;
  transform: translateX(-50%);
  font-size: 0.75rem;
  color: #94a3b8;
}

/* Pie Chart Mockup */
.pie-placeholder {
  height: 250px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
}

.donut {
  width: 140px;
  height: 140px;
  border-radius: 50%;
  background: conic-gradient(#3b82f6 0% 24%, #10b981 24% 45%, #f59e0b 45% 66%, #e2e8f0 66% 100%);
  position: relative;
}

.donut-inner {
  position: absolute;
  top: 25px; left: 25px;
  width: 90px; height: 90px;
  background: white;
  border-radius: 50%;
}

.pie-legend {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 0.85rem;
  width: 100%;
}

.legend-item { display: flex; align-items: center; gap: 8px; color: #64748b; }
.legend-item .dot { width: 10px; height: 10px; border-radius: 50%; }

/* Area Chart Mockup & Real Points */
.area-placeholder {
  height: 250px;
  background: linear-gradient(180deg, rgba(59, 130, 246, 0.05) 0%, rgba(255, 255, 255, 0) 100%);
  position: relative;
  overflow: hidden;
  border-radius: 0 0 16px 16px;
}

.revenue-grid {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 40px;
  display: flex;
  justify-content: space-around;
  align-items: flex-end;
  padding: 0 2rem;
  z-index: 5;
}

.rev-point {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  transition: all 0.5s ease;
}

.point-glow {
  width: 8px; height: 8px;
  background: #3b82f6;
  border-radius: 50%;
  box-shadow: 0 0 10px #3b82f6;
  margin-bottom: 8px;
}

.p-val {
  font-size: 0.7rem;
  font-weight: 700;
  color: #1e293b;
  position: absolute;
  top: -20px;
}

.p-label {
  position: absolute;
  bottom: -30px;
  font-size: 0.75rem;
  color: #94a3b8;
  font-weight: 600;
}

.area-curve {
  position: absolute;
  bottom: 40px; left: 0; right: 0;
  height: 120px;
  background: linear-gradient(180deg, rgba(59, 130, 246, 0.2) 0%, rgba(255, 255, 255, 0) 100%);
  clip-path: polygon(0 80%, 10% 70%, 20% 75%, 30% 60%, 40% 65%, 50% 50%, 60% 55%, 70% 40%, 80% 45%, 90% 30%, 100% 35%, 100% 100%, 0 100%);
  opacity: 0.3;
}

/* Bottom Grid */
.bottom-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 1.5rem;
}

.bottom-grid h3 {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 1.5rem;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.action-card {
  background: white;
  padding: 1.25rem;
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  gap: 15px;
  cursor: pointer;
  transition: all 0.2s;
}

.action-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  border-color: #3b82f6;
}

.icon-box {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}

.icon-box.blue { background: #eff6ff; color: #3b82f6; }
.icon-box.green { background: #f0fdf4; color: #10b981; }
.icon-box.purple { background: #faf5ff; color: #a855f7; }
.icon-box.orange { background: #fff7ed; color: #f59e0b; }

.action-card h4 {
  font-size: 0.95rem;
  font-weight: 700;
  margin: 0;
  color: #1e293b;
}

.action-card p {
  font-size: 0.8rem;
  color: #64748b;
  margin: 2px 0 0 0;
}

/* Activity Section */
.activity-section {
  background: white;
  padding: 1.5rem;
  border-radius: 16px;
  border: 1px solid #e2e8f0;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.act-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  flex-shrink: 0;
}

.act-details {
  display: flex;
  flex-direction: column;
}

.act-text {
  font-size: 0.9rem;
  font-weight: 600;
  color: #475569;
  margin: 0;
  line-height: 1.4;
}

.act-time {
  font-size: 0.75rem;
  color: #94a3b8;
  margin-top: 2px;
}

@media (max-width: 1200px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
  .charts-row, .bottom-grid { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
  .actions-grid { grid-template-columns: 1fr; }
}
</style>
