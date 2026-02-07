<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useToast } from 'vue-toastification';
import { navigationApi, type NavigationItem } from '@/api/navigation.api';

/**
 * Vue des paramètres système - Réservée aux SuperAdmins.
 * Permet de configurer les aspects globaux de la plateforme Konta.
 */

const toast = useToast();

// Paramètres fictifs pour démonstration
const settings = ref({
  maintenanceMode: false,
  allowNewRegistrations: true,
  maxUsersPerTenant: 50,
  maxDocumentsPerMonth: 100,
  defaultPlan: 'Free'
});

const plans = ['Free', 'Basique', 'Avancée', 'Premium', 'Expertise'];

// Navigation Management
const menuItems = ref<NavigationItem[]>([]);
const loadingMenu = ref(false);

onMounted(async () => {
    await fetchMenuItems();
});

const fetchMenuItems = async () => {
    loadingMenu.value = true;
    try {
        menuItems.value = await navigationApi.getNavigationItems();
    } catch (err) {
        toast.error('Erreur lors du chargement du menu.');
    } finally {
        loadingMenu.value = false;
    }
};

const toggleVisibility = async (item: NavigationItem) => {
    try {
        await navigationApi.updateNavigationItem(item.id, { isVisible: item.isVisible });
        toast.success(`Visibilité de "${item.label}" mise à jour.`);
    } catch (err) {
        item.isVisible = !item.isVisible; // Revert
        toast.error('Échec de la mise à jour.');
    }
};

const updateItem = async (item: NavigationItem) => {
    try {
        await navigationApi.updateNavigationItem(item.id, { ...item });
        toast.success(`Élément "${item.label}" enregistré.`);
    } catch (err) {
        toast.error('Échec de l\'enregistrement.');
    }
};

const saveSettings = () => {
  // TODO: Appeler l'API pour sauvegarder les paramètres
  toast.success('Paramètres enregistrés avec succès !');
};
</script>

<template>
  <div class="settings-view">
    <div class="settings-header">
      <h1>Paramètres de la Plateforme</h1>
      <p class="subtitle">Configuration globale réservée aux Super Administrateurs</p>
    </div>

    <div class="settings-grid">
      <!-- Paramètres Système -->
      <div class="settings-card">
        <h2><i class="fas fa-cog"></i> Système</h2>

        <div class="setting-item">
          <div class="setting-info">
            <label>Mode Maintenance</label>
            <span class="hint">Désactive temporairement l'accès utilisateur</span>
          </div>
          <label class="toggle">
            <input type="checkbox" v-model="settings.maintenanceMode" />
            <span class="slider"></span>
          </label>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <label>Nouvelles Inscriptions</label>
            <span class="hint">Autoriser les nouvelles entreprises à s'inscrire</span>
          </div>
          <label class="toggle">
            <input type="checkbox" v-model="settings.allowNewRegistrations" />
            <span class="slider"></span>
          </label>
        </div>
      </div>

      <!-- Paramètres Tenants -->
      <div class="settings-card">
        <h2><i class="fas fa-building"></i> Tenants</h2>

        <div class="setting-item">
          <div class="setting-info">
            <label>Utilisateurs Max par Tenant</label>
            <span class="hint">Limite par défaut pour les nouvelles entreprises</span>
          </div>
          <input type="number" v-model="settings.maxUsersPerTenant" class="number-input" min="1" max="1000" />
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <label>Documents Max par Mois</label>
            <span class="hint">Quota mensuel de traitement OCR</span>
          </div>
          <input type="number" v-model="settings.maxDocumentsPerMonth" class="number-input" min="1" max="10000" />
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <label>Plan par Défaut</label>
            <span class="hint">Forfait attribué aux nouvelles inscriptions</span>
          </div>
          <select v-model="settings.defaultPlan" class="select-input">
            <option v-for="plan in plans" :key="plan" :value="plan">{{ plan }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Gestion du Menu de Navigation -->
    <div class="settings-full-card">
      <h2><i class="fas fa-bars"></i> Gestion du Menu de Navigation</h2>
      <p class="hint">Activez/Désactivez les boutons du menu et configurez les accès par rôle.</p>
      
      <div v-if="loadingMenu" class="loading-state">
        <i class="fas fa-spinner fa-spin"></i> Chargement du menu...
      </div>
      
      <div v-else class="menu-manager">
        <table class="menu-table">
          <thead>
            <tr>
              <th>Élément</th>
              <th>Chemin</th>
              <th>Rôle Requis</th>
              <th>Ordre</th>
              <th>Visibilité</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in menuItems" :key="item.id">
              <td>
                <div class="item-label-cell">
                  <i :class="item.icon" class="item-icon"></i>
                  <input v-model="item.label" class="inline-input" />
                </div>
              </td>
              <td><input v-model="item.path" class="inline-input" /></td>
              <td>
                <select v-model="item.requiredRole" class="inline-select">
                  <option :value="null">Tout le monde</option>
                  <option value="User">Utilisateur</option>
                  <option value="Admin">Administrateur</option>
                  <option value="SuperAdmin">Super Administrateur</option>
                </select>
              </td>
              <td class="order-cell">
                <input type="number" v-model="item.displayOrder" class="inline-input-sm" />
              </td>
              <td>
                <label class="toggle-sm">
                  <input type="checkbox" v-model="item.isVisible" @change="toggleVisibility(item)" />
                  <span class="slider"></span>
                </label>
              </td>
              <td>
                <button class="btn-icon-save" @click="updateItem(item)" title="Enregistrer">
                  <i class="fas fa-save"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="settings-actions">
      <button class="btn-save" @click="saveSettings">
        <i class="fas fa-save"></i> Enregistrer les Modifications
      </button>
    </div>
  </div>
</template>

<style scoped>
.settings-view {
  max-width: 1000px;
  margin: 0 auto;
}

.settings-header {
  margin-bottom: 2rem;
}

.settings-header h1 {
  font-size: 1.8rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 0.5rem 0;
}

.subtitle {
  color: #64748b;
  font-size: 0.95rem;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.settings-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border: 1px solid #e2e8f0;
}

.settings-card h2 {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 1.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.settings-card h2 i {
  color: #3b82f6;
}

.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.setting-item:last-child {
  border-bottom: none;
}

.setting-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.setting-info label {
  font-weight: 600;
  color: #1e293b;
  font-size: 0.95rem;
}

.hint {
  font-size: 0.8rem;
  color: #94a3b8;
}

/* Toggle Switch */
.toggle {
  position: relative;
  width: 48px;
  height: 26px;
}

.toggle input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #cbd5e1;
  transition: 0.3s;
  border-radius: 26px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 20px;
  width: 20px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: 0.3s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: #3b82f6;
}

input:checked + .slider:before {
  transform: translateX(22px);
}

/* Number Input */
.number-input {
  width: 100px;
  padding: 0.5rem 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.95rem;
  text-align: right;
  color: #1e293b;
}

.number-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Select Input */
.select-input {
  padding: 0.5rem 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.95rem;
  color: #1e293b;
  min-width: 140px;
}

.select-input:focus {
  outline: none;
  border-color: #3b82f6;
}

/* Actions */
.settings-actions {
  display: flex;
  justify-content: flex-end;
}

.btn-save {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border: none;
  padding: 0.85rem 1.5rem;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.95rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s;
}

.btn-save:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
}

/* Menu Manager Styles */
.settings-full-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border: 1px solid #e2e8f0;
  margin-bottom: 2rem;
}

.settings-full-card h2 {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 0.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.menu-manager {
  margin-top: 1.5rem;
  overflow-x: auto;
}

.menu-table {
  width: 100%;
  border-collapse: collapse;
}

.menu-table th {
  text-align: left;
  font-size: 0.8rem;
  text-transform: uppercase;
  color: #64748b;
  padding: 0.75rem 1rem;
  border-bottom: 2px solid #f1f5f9;
}

.menu-table td {
  padding: 1rem;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.item-label-cell {
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.item-icon {
  width: 20px;
  text-align: center;
  color: #94a3b8;
}

.inline-input {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 0.4rem 0.6rem;
  font-size: 0.9rem;
  width: 100%;
}

.inline-input-sm {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 0.4rem 0.6rem;
  font-size: 0.9rem;
  width: 60px;
  text-align: center;
}

.inline-select {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 0.4rem 0.6rem;
  font-size: 0.9rem;
  background: white;
}

.toggle-sm {
  position: relative;
  width: 40px;
  height: 22px;
  display: inline-block;
}

.toggle-sm input { display: none; }

.toggle-sm .slider { border-radius: 22px; }
.toggle-sm .slider:before {
  height: 16px;
  width: 16px;
  left: 3px;
  bottom: 3px;
}

input:checked + .toggle-sm .slider:before {
  transform: translateX(18px);
}

.btn-icon-save {
  background: #f1f5f9;
  color: #3b82f6;
  border: none;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-icon-save:hover {
  background: #3b82f6;
  color: white;
}

.loading-state {
  padding: 2rem;
  text-align: center;
  color: #64748b;
}
</style>
