<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { useTenantStore } from '@/stores/tenant.store';
import StatCard from '@/components/ui/StatCard.vue';

const authStore = useAuthStore();
const tenantStore = useTenantStore();

const activities = ref([
  { id: 1, type: 'document', text: 'Nouveau document traité : contrats_clients_2024.pdf', time: 'Il y a 2 heures', icon: 'fas fa-file-pdf', color: '#3b82f6' },
  { id: 2, type: 'user', text: 'Marie Martin s\'est connectée', time: 'Il y a 3 heures', icon: 'fas fa-user', color: '#10b981' },
  { id: 3, type: 'company', text: 'Nouvelle entreprise ajoutée : Innovation Labs', time: 'Il y a 5 heures', icon: 'fas fa-building', color: '#a855f7' },
  { id: 4, type: 'extraction', text: '13 enregistrements extraits de factures_janvier.xlsx', time: 'Il y a 1 jour', icon: 'fas fa-database', color: '#f59e0b' },
]);
</script>

<template>
  <div class="dashboard-view">
    <header class="dashboard-header">
      <div class="welcome">
        <p>Voici un aperçu de votre activité sur {{ tenantStore.currentTenant?.name || 'votre entreprise' }}</p>
      </div>
    </header>

    <!-- Stats Grid -->
    <div class="stats-grid">
      <StatCard label="Documents" :value="156" trend="up" :trendValue="12" icon="fas fa-file-alt" color="#3b82f6" />
      <StatCard label="Entreprises" :value="23" trend="up" :trendValue="12.5" icon="fas fa-building" color="#10b981" />
      <StatCard label="Utilisateurs" :value="8" icon="fas fa-users" color="#a855f7" />
      <StatCard label="Chiffre d'affaires" :value="425000" format="currency" trend="up" :trendValue="15" icon="fas fa-euro-sign" color="#f59e0b" />
    </div>

    <!-- Middle Row: Charts -->
    <div class="charts-row">
      <div class="chart-card bar-chart">
        <div class="card-header">
          <h3>Documents par mois</h3>
        </div>
        <div class="chart-placeholder bar-placeholder">
          <div class="bar" style="height: 40%"><span>Jan</span></div>
          <div class="bar" style="height: 35%"><span>Fév</span></div>
          <div class="bar" style="height: 60%"><span>Mar</span></div>
          <div class="bar" style="height: 50%"><span>Avr</span></div>
          <div class="bar" style="height: 70%"><span>Mai</span></div>
          <div class="bar" style="height: 65%"><span>Juin</span></div>
          <div class="bar" style="height: 85%"><span>Juil</span></div>
          <div class="bar" style="height: 80%"><span>Août</span></div>
        </div>
      </div>
      
      <div class="chart-card pie-chart">
        <div class="card-header">
          <h3>Types de documents</h3>
        </div>
        <div class="chart-placeholder pie-placeholder">
          <div class="donut">
            <div class="slice s1"></div>
            <div class="slice s2"></div>
            <div class="slice s3"></div>
            <div class="donut-inner"></div>
          </div>
          <div class="pie-legend">
            <div class="legend-item"><span class="dot" style="background: #3b82f6"></span> Factures 24%</div>
            <div class="legend-item"><span class="dot" style="background: #10b981"></span> Contrats 21%</div>
            <div class="legend-item"><span class="dot" style="background: #f59e0b"></span> Devis 21%</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Long Chart Row -->
    <div class="chart-card line-chart">
      <div class="card-header">
        <h3>Évolution du chiffre d'affaires</h3>
      </div>
      <div class="chart-placeholder area-placeholder">
        <div class="area-curve"></div>
      </div>
    </div>

    <!-- Bottom Row -->
    <div class="bottom-grid">
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
    </div>
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

/* Area Chart Mockup */
.area-placeholder {
  height: 200px;
  background: linear-gradient(180deg, rgba(59, 130, 246, 0.1) 0%, rgba(255, 255, 255, 0) 100%);
  position: relative;
  border-bottom: 2px solid #e2e8f0;
}

.area-curve {
  position: absolute;
  bottom: 0; left: 0; right: 0;
  height: 80px;
  background: #3b82f6;
  clip-path: polygon(0 80%, 10% 70%, 20% 75%, 30% 60%, 40% 65%, 50% 50%, 60% 55%, 70% 40%, 80% 45%, 90% 30%, 100% 35%, 100% 100%, 0 100%);
  opacity: 0.4;
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
