<script setup lang="ts">
import { ref, onMounted, watch, reactive } from 'vue';
import { userApi } from '@/api/user.api';
import { useAuthStore } from '@/stores/auth.store';
import type { UserInfo } from '@/types/auth.types';
import { useToast } from 'vue-toastification';

const toast = useToast();
const authStore = useAuthStore();

const users = ref<UserInfo[]>([]);
const selectedUserId = ref('');
const selectedUser = ref<UserInfo | null>(null);
const loading = ref(false);
const saving = ref(false);

// Detailed Permissions (Mapping real backend fields + some UI mocks as requested)
const permissions = reactive({
  canSeeAllTenantData: false,
  canManageUsers: false,
  canAccessBilling: false,
  canExportData: false,
  canManualValidate: false
});

onMounted(async () => {
  await fetchUsers();
});

const fetchUsers = async () => {
  loading.value = true;
  try {
    const allUsers = await userApi.getUsers();
    // Prioritize non-SuperAdmins since SuperAdmins already have full access
    users.value = allUsers.filter(u => u.role !== 'SuperAdmin');
  } catch (err) {
    toast.error('Erreur lors du chargement des utilisateurs.');
  } finally {
    loading.value = false;
  }
};

watch(selectedUserId, (newId) => {
  if (newId) {
    const user = users.value.find(u => u.id === newId);
    if (user) {
      selectedUser.value = user;
      // Initialize switches with real values from user model
      permissions.canSeeAllTenantData = user.canSeeAllTenantData || false;
      
      // Mock initialization for others (could be mapped to bitwise permissions or separate columns later)
      permissions.canManageUsers = user.role === 'Admin' || user.role === 'Manager';
      permissions.canAccessBilling = user.role === 'Admin';
      permissions.canExportData = true;
      permissions.canManualValidate = true;
    }
  } else {
    selectedUser.value = null;
  }
});

const savePermissions = async () => {
  if (!selectedUser.value) return;
  
  saving.value = true;
  try {
    await userApi.updateUser(selectedUser.value.id, {
      canSeeAllTenantData: permissions.canSeeAllTenantData
      // In a real scenario, we would send all permissions here
    } as any);
    
    // Update local state
    const index = users.value.findIndex(u => u.id === selectedUser.value!.id);
    if (index !== -1) {
      users.value[index].canSeeAllTenantData = permissions.canSeeAllTenantData;
    }
    
    toast.success(`Accès de ${selectedUser.value.firstName} mis à jour avec succès.`);
  } catch (err) {
    toast.error('Échec de la mise à jour des accès.');
  } finally {
    saving.value = false;
  }
};
</script>

<template>
  <div class="access-view">
    <div class="view-header">
      <div class="header-text">
        <h1>Gestion des Accès</h1>
        <p>Configurez les privilèges détaillés pour chaque collaborateur.</p>
      </div>
    </div>

    <div class="content-grid">
      <!-- Left Panel: User Selection -->
      <div class="selection-card">
        <h3><i class="fas fa-user-circle"></i> Sélectionner un utilisateur</h3>
        <div class="form-group">
          <label>Utilisateur à configurer</label>
          <select v-model="selectedUserId" class="user-select">
            <option value="" disabled>Choisir un membre de l'équipe...</option>
            <option v-for="user in users" :key="user.id" :value="user.id">
              {{ user.firstName }} {{ user.lastName }} ({{ user.role }})
            </option>
          </select>
        </div>

        <div v-if="selectedUser" class="user-summary">
          <div class="avatar-lg">
            {{ selectedUser.firstName.charAt(0) }}{{ selectedUser.lastName.charAt(0) }}
          </div>
          <div class="user-info">
            <span class="name">{{ selectedUser.firstName }} {{ selectedUser.lastName }}</span>
            <span class="role-badge">{{ selectedUser.role }}</span>
            <span class="email">{{ selectedUser.email }}</span>
          </div>
        </div>
        
        <div v-else class="empty-selection">
          <i class="fas fa-mouse-pointer"></i>
          <p>Sélectionnez un utilisateur pour commencer le paramétrage.</p>
        </div>
      </div>

      <!-- Right Panel: Permissions Details -->
      <div class="permissions-card" :class="{ disabled: !selectedUser }">
        <div class="card-header">
          <h3><i class="fas fa-shield-alt"></i> Privilèges détaillés</h3>
          <button 
            @click="savePermissions" 
            class="btn-save" 
            :disabled="!selectedUser || saving"
          >
            <i class="fas" :class="saving ? 'fa-spinner fa-spin' : 'fa-check'"></i>
            {{ saving ? 'Enregistrement...' : 'Enregistrer les accès' }}
          </button>
        </div>

        <div class="permissions-list">
          <!-- Data Visibility -->
          <div class="permission-group">
            <div class="group-title">Visibilité des données</div>
            
            <div class="permission-item">
              <div class="item-text">
                <strong>Visibilité Globale de l'entreprise</strong>
                <p>Permet à l'utilisateur de consulter les documents de tous les services.</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="permissions.canSeeAllTenantData" :disabled="!selectedUser" />
                <span class="slider"></span>
              </label>
            </div>

            <div class="permission-item">
              <div class="item-text">
                <strong>Accès aux exports de données</strong>
                <p>Autoriser le téléchargement des fichiers CSV/Excel récapitulatifs.</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="permissions.canExportData" :disabled="!selectedUser" />
                <span class="slider"></span>
              </label>
            </div>
          </div>

          <!-- Administration -->
          <div class="permission-group">
            <div class="group-title">Administration & Gestion</div>
            
            <div class="permission-item">
              <div class="item-text">
                <strong>Gestion des collaborateurs</strong>
                <p>Peut créer, modifier ou supprimer des utilisateurs de son entreprise.</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="permissions.canManageUsers" :disabled="!selectedUser" />
                <span class="slider"></span>
              </label>
            </div>

            <div class="permission-item">
              <div class="item-text">
                <strong>Accès à la facturation</strong>
                <p>Peut changer le forfait ou modifier les informations bancaires.</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="permissions.canAccessBilling" :disabled="!selectedUser" />
                <span class="slider"></span>
              </label>
            </div>
          </div>

          <!-- OCR Specialized -->
          <div class="permission-group">
            <div class="group-title">Traitement Intelligent (OCR)</div>
            
            <div class="permission-item">
              <div class="item-text">
                <strong>Validation Manuelle</strong>
                <p>Autoriser la modification des champs extraits par l'IA avant validation finale.</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="permissions.canManualValidate" :disabled="!selectedUser" />
                <span class="slider"></span>
              </label>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.access-view { display: flex; flex-direction: column; gap: 2rem; }
.view-header h1 { font-size: 1.8rem; font-weight: 800; color: #1a202c; margin: 0; }
.view-header p { color: #718096; margin-top: 5px; }

.content-grid { display: grid; grid-template-columns: 350px 1fr; gap: 2rem; align-items: start; }

.selection-card, .permissions-card {
  background: white; border-radius: 20px; padding: 1.5rem;
  border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);
}

h3 { margin-top: 0; margin-bottom: 1.5rem; font-size: 1.1rem; color: #2d3748; display: flex; align-items: center; gap: 10px; }
h3 i { color: #3182ce; }

.form-group { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1.5rem; }
.form-group label { font-size: 0.85rem; font-weight: 700; color: #4a5568; }

.user-select {
  padding: 0.75rem; border: 1.5px solid #e2e8f0; border-radius: 12px; outline: none;
  font-size: 0.95rem; background: #f8fafc; cursor: pointer; transition: all 0.2s;
}
.user-select:focus { border-color: #3182ce; background: white; }

.user-summary {
  display: flex; align-items: center; gap: 1.25rem; padding: 1.25rem;
  background: #f7fafc; border-radius: 16px; border: 1px dashed #cbd5e0;
}
.avatar-lg {
  width: 56px; height: 56px; background: #3182ce; color: white;
  border-radius: 16px; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 1.2rem;
}
.user-info { display: flex; flex-direction: column; }
.user-info .name { font-weight: 800; color: #1a202c; font-size: 1rem; }
.user-info .role-badge {
  font-size: 0.7rem; font-weight: 800; background: #ebf8ff; color: #3182ce;
  padding: 2px 8px; border-radius: 6px; width: max-content; margin: 4px 0; text-transform: uppercase;
}
.user-info .email { font-size: 0.8rem; color: #718096; }

.empty-selection {
  text-align: center; padding: 3rem 1rem; color: #a0aec0;
  display: flex; flex-direction: column; align-items: center; gap: 1rem;
}
.empty-selection i { font-size: 2.5rem; opacity: 0.3; }
.empty-selection p { font-size: 0.9rem; line-height: 1.4; }

.permissions-card.disabled { opacity: 0.6; }

.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; border-bottom: 1px solid #f1f5f9; padding-bottom: 1rem; }
.card-header h3 { margin-bottom: 0; }

.btn-save {
  padding: 0.75rem 1.25rem; background: #1a202c; color: white; border: none; border-radius: 12px;
  font-weight: 700; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: all 0.2s;
}
.btn-save:hover:not(:disabled) { background: #2d3748; transform: translateY(-1px); }
.btn-save:disabled { opacity: 0.5; cursor: not-allowed; }

.permission-group { margin-bottom: 2rem; }
.group-title { font-size: 0.75rem; font-weight: 800; color: #a0aec0; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 1rem; }

.permission-item {
  display: flex; justify-content: space-between; align-items: center; padding: 1rem 0;
  border-bottom: 1px solid #f8fafc;
}
.item-text { display: flex; flex-direction: column; gap: 2px; }
.item-text strong { font-size: 0.95rem; color: #2d3748; }
.item-text p { font-size: 0.8rem; color: #718096; margin: 0; }

/* Switch Style */
.switch { position: relative; display: inline-block; width: 44px; height: 24px; }
.switch input { opacity: 0; width: 0; height: 0; }
.slider {
  position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0;
  background-color: #e2e8f0; transition: .4s; border-radius: 34px;
}
.slider:before {
  position: absolute; content: ""; height: 18px; width: 18px;
  left: 3px; bottom: 3px; background-color: white; transition: .4s; border-radius: 50%;
}
input:checked + .slider { background-color: #38a169; }
input:focus + .slider { box-shadow: 0 0 1px #38a169; }
input:checked + .slider:before { transform: translateX(20px); }
input:disabled + .slider { opacity: 0.5; cursor: not-allowed; }

@media (max-width: 1024px) {
  .content-grid { grid-template-columns: 1fr; }
}
</style>
