<script setup lang="ts">
import { ref } from 'vue';
/**
 * AdminView : Espace de Contrôle Global
 * Réservé aux administrateurs système.
 */

const activeTab = ref('users');
const searchUser = ref('');

const users = ref([
  { id: 1, name: 'Jean Dupont', email: 'admin@konta.com', role: 'Admin', tenant: 'Konta Corp', status: 'Actif' },
  { id: 2, name: 'Marie Curie', email: 'marie@science.fr', role: 'User', tenant: 'Radium SA', status: 'Actif' },
  { id: 3, name: 'Alan Turing', email: 'alan@enigma.io', role: 'User', tenant: 'Bletchley Inc', status: 'Suspendu' },
]);

const tenants = ref([
  { id: 1, name: 'Konta Corp', identifier: 'konta-corp', plan: 'Premium', users: 12, status: 'Actif' },
  { id: 2, name: 'Radium SA', identifier: 'radium-sa', plan: 'Expertise', users: 45, status: 'Actif' },
  { id: 3, name: 'Bletchley Inc', identifier: 'bletchley', plan: 'Advanced', users: 8, status: 'Inactif' },
]);
</script>

<template>
  <div class="admin-container">
    <div class="admin-header">
      <div class="header-info">
        <h1>Console Système Elite</h1>
        <p>Contrôle global des utilisateurs, des tenants et de l'infrastructure.</p>
      </div>
      <div class="header-actions">
        <button class="btn-refresh"><i class="fas fa-sync-alt"></i></button>
        <button class="btn-primary">Déployer Nouveau Tenant</button>
      </div>
    </div>

    <!-- Stats Overview -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon u"><i class="fas fa-users"></i></div>
        <div class="stat-data">
          <span class="label">Total Utilisateurs</span>
          <span class="value">1,284</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon t"><i class="fas fa-building"></i></div>
        <div class="stat-data">
          <span class="label">Tenants Actifs</span>
          <span class="value">342</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon r"><i class="fas fa-euro-sign"></i></div>
        <div class="stat-data">
          <span class="label">Revenus MRR</span>
          <span class="value">42.5k €</span>
        </div>
      </div>
    </div>

    <!-- Main Content Area -->
    <div class="admin-card">
      <div class="card-tabs">
        <button 
          :class="['tab-btn', { active: activeTab === 'users' }]" 
          @click="activeTab = 'users'"
        >
          <i class="fas fa-user-shield"></i> Utilisateurs
        </button>
        <button 
          :class="['tab-btn', { active: activeTab === 'tenants' }]" 
          @click="activeTab = 'tenants'"
        >
          <i class="fas fa-city"></i> Tenants
        </button>
        <button 
          :class="['tab-btn', { active: activeTab === 'settings' }]" 
          @click="activeTab = 'settings'"
        >
          <i class="fas fa-sliders-h"></i> Système
        </button>
      </div>

      <div class="card-body">
        <!-- Users Management -->
        <div v-if="activeTab === 'users'" class="tab-content">
          <div class="search-bar">
            <i class="fas fa-search"></i>
            <input v-model="searchUser" placeholder="Rechercher un utilisateur (Email, Nom, Tenant)..." />
          </div>
          
          <table class="premium-table">
            <thead>
              <tr>
                <th>Utilisateur</th>
                <th>Tenant</th>
                <th>Rôle</th>
                <th>Statut</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in users" :key="user.id">
                <td>
                  <div class="user-cell">
                    <div class="avatar-sm">{{ user.name.charAt(0) }}</div>
                    <div class="user-info">
                      <span class="name">{{ user.name }}</span>
                      <span class="email">{{ user.email }}</span>
                    </div>
                  </div>
                </td>
                <td>{{ user.tenant }}</td>
                <td><span class="role-badge" :class="user.role.toLowerCase()">{{ user.role }}</span></td>
                <td><span class="status-dot" :class="user.status.toLowerCase()"></span> {{ user.status }}</td>
                <td class="actions">
                  <button class="btn-icon"><i class="fas fa-edit"></i></button>
                  <button class="btn-icon danger"><i class="fas fa-ban"></i></button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Tenants Management -->
        <div v-if="activeTab === 'tenants'" class="tab-content">
          <table class="premium-table">
            <thead>
              <tr>
                <th>Entreprise</th>
                <th>ID</th>
                <th>Forfait</th>
                <th>Users</th>
                <th>Statut</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="tenant in tenants" :key="tenant.id">
                <td class="bold">{{ tenant.name }}</td>
                <td class="mono">{{ tenant.identifier }}</td>
                <td><span class="plan-badge">{{ tenant.plan }}</span></td>
                <td>{{ tenant.users }}</td>
                <td><span class="status-dot" :class="tenant.status.toLowerCase()"></span> {{ tenant.status }}</td>
                <td class="actions">
                  <button class="btn-icon"><i class="fas fa-eye"></i></button>
                  <button class="btn-icon"><i class="fas fa-cog"></i></button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-container {
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
}

.admin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
  padding: 2.5rem;
  background: #0f172a; /* Elite Dark Style */
  border-radius: 24px;
  color: white;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.2);
}

.header-info h1 {
  font-size: 2.2rem;
  font-weight: 800;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.header-info p {
  color: #94a3b8;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.btn-refresh {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  border: 1px solid #334155;
  background: #1e293b;
  color: #94a3b8;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-refresh:hover {
  background: #334155;
  color: white;
}

.btn-primary {
  padding: 0 1.5rem;
  height: 48px;
  border-radius: 12px;
  border: none;
  background: #3b82f6;
  color: white;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary:hover {
  background: #2563eb;
  transform: translateY(-2px);
}

/* Stats */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  margin-bottom: 2.5rem;
}

.stat-card {
  background: white;
  padding: 1.8rem;
  border-radius: 20px;
  display: flex;
  align-items: center;
  gap: 1.5rem;
  border: 1px solid #f1f5f9;
  box-shadow: 0 4px 6px rgba(0,0,0,0.02);
  transition: transform 0.2s;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.stat-icon.u { background: #eff6ff; color: #3b82f6; }
.stat-icon.t { background: #f0fdf4; color: #22c55e; }
.stat-icon.r { background: #faf5ff; color: #a855f7; }

.stat-data .label {
  display: block;
  font-size: 0.875rem;
  color: #64748b;
  margin-bottom: 4px;
  font-weight: 500;
}

.stat-data .value {
  font-size: 1.5rem;
  font-weight: 800;
  color: #0f172a;
}

/* Main Card */
.admin-card {
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05);
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.card-tabs {
  display: flex;
  background: #f1f5f9;
  padding: 8px;
  gap: 8px;
}

.tab-btn {
  padding: 12px 24px;
  border-radius: 12px;
  border: none;
  background: transparent;
  color: #64748b;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.2s;
}

.tab-btn.active {
  background: white;
  color: #3b82f6;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.tab-btn:hover:not(.active) {
  background: rgba(0,0,0,0.02);
  color: #0f172a;
}

.card-body {
  padding: 2.5rem;
}

/* Search Bar */
.search-bar {
  position: relative;
  margin-bottom: 32px;
}

.search-bar i {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
}

.search-bar input {
  width: 100%;
  padding: 14px 16px 14px 48px;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  background: #f8fafc;
  outline: none;
  transition: all 0.2s;
}

.search-bar input:focus {
  border-color: #3b82f6;
  background: white;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.05);
}

/* Table */
.premium-table {
  width: 100%;
  border-collapse: collapse;
}

.premium-table th {
  text-align: left;
  padding: 16px;
  background: #f8fafc;
  color: #64748b;
  font-weight: 700;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  border-bottom: 2px solid #e2e8f0;
}

.premium-table td {
  padding: 16px;
  border-bottom: 1px solid #f1f5f9;
  color: #475569;
  font-size: 0.95rem;
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.avatar-sm {
  width: 38px;
  height: 38px;
  background: #e2e8f0;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  color: #3b82f6;
}

.user-info .name {
  display: block;
  font-weight: 700;
  color: #0f172a;
}

.user-info .email {
  font-size: 0.8rem;
  color: #94a3b8;
}

.role-badge {
  padding: 4px 12px;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 800;
  text-transform: uppercase;
}

.role-badge.admin { background: #eff6ff; color: #1d4ed8; }
.role-badge.user { background: #f8fafc; color: #64748b; }

.status-dot {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  margin-right: 10px;
}

.status-dot.actif { background: #10b981; }
.status-dot.suspendu, .status-dot.inactif { background: #ef4444; }

.bold { font-weight: 700; color: #0f172a; }
.mono { font-family: 'JetBrains Mono', monospace; font-size: 0.85rem; color: #64748b; }

.plan-badge {
  background: #f0f9ff;
  color: #0369a1;
  padding: 6px 12px;
  border-radius: 10px;
  font-weight: 800;
  font-size: 0.75rem;
}

.btn-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #64748b;
  cursor: pointer;
  margin-right: 8px;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-icon:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.btn-icon.danger:hover {
  background: #fee2e2;
  color: #ef4444;
}

@media (max-width: 1024px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .admin-header { flex-direction: column; align-items: flex-start; gap: 20px; padding: 1.5rem; }
  .stats-grid { grid-template-columns: 1fr; }
  .premium-table th:nth-child(2), .premium-table td:nth-child(2) { display: none; }
  .card-tabs { overflow-x: auto; padding: 0 1rem; }
}
</style>
