<script setup lang="ts">
import { ref } from 'vue';
import { useToast } from 'vue-toastification';

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
</style>
</script>
