<script setup lang="ts">
import { ref, onMounted, computed, reactive } from 'vue';
import { userApi } from '@/api/user.api';
import { tenantApi, type TenantResponse } from '@/api/tenant.api';
import { useAuthStore } from '@/stores/auth.store';
import type { UserInfo } from '@/types/auth.types';
import StatCard from '@/components/ui/StatCard.vue';
import BaseModal from '@/components/ui/BaseModal.vue';
import { useToast } from 'vue-toastification';

const toast = useToast();
const authStore = useAuthStore();

/**
 * AdminView : Gestion Professionnelle des Utilisateurs.
 */

const users = ref<UserInfo[]>([]);
const tenants = ref<TenantResponse[]>([]);
const searchUser = ref('');
const loading = ref(true);
const saving = ref(false);
const selectedTenantFilter = ref('');

const isSuperAdmin = computed(() => {
  return authStore.user?.role === 'SuperAdmin';
});

// Modale & Formulaire
const showModal = ref(false);
const isEditing = ref(false);
const userForm = reactive<Partial<UserInfo>>({
  id: '',
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  role: 'User',
  isActive: true
});

// Pagination et Tri
const currentPage = ref(1);
const itemsPerPage = ref(10);
const sortBy = ref('createdAt');
const sortOrder = ref<'asc' | 'desc'>('desc');

onMounted(async () => {
  if (isSuperAdmin.value) {
    await fetchTenants();
  }
  await fetchUsers();
});

const fetchTenants = async () => {
  try {
    tenants.value = await tenantApi.getTenants();
  } catch (err) {
    console.warn('Impossible de charger les tenants', err);
  }
};

const fetchUsers = async () => {
  loading.value = true;
  try {
    users.value = await userApi.getUsers();
  } catch (err) {
    toast.error('Erreur lors du chargement des utilisateurs.');
  } finally {
    loading.value = false;
  }
};

const sortedAndFilteredUsers = computed(() => {
  let result = [...users.value];

  if (selectedTenantFilter.value) {
    result = result.filter(u => u.tenantId === selectedTenantFilter.value);
  }

  if (searchUser.value) {
    const s = searchUser.value.toLowerCase();
    result = result.filter(u => 
      u.firstName?.toLowerCase().includes(s) || 
      u.lastName?.toLowerCase().includes(s) || 
      u.email.toLowerCase().includes(s)
    );
  }

  result.sort((a: any, b: any) => {
    const valA = a[sortBy.value];
    const valB = b[sortBy.value];
    if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1;
    return 0;
  });

  return result;
});

const paginatedUsers = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value;
  return sortedAndFilteredUsers.value.slice(start, start + itemsPerPage.value);
});

const totalPages = computed(() => Math.ceil(sortedAndFilteredUsers.value.length / itemsPerPage.value));

const handleSort = (field: string) => {
  if (sortBy.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortBy.value = field;
    sortOrder.value = 'desc';
  }
};

const openAddModal = () => {
  isEditing.value = false;
  Object.assign(userForm, {
    id: undefined, email: '', password: '', firstName: '', lastName: '', role: 'User', isActive: true
  });
  showModal.value = true;
};

const openEditModal = (user: UserInfo) => {
  isEditing.value = true;
  Object.assign(userForm, { ...user });
  showModal.value = true;
};

const saveUser = async () => {
  saving.value = true;
  try {
    // Nettoyage de l'objet pour éviter les erreurs de binding Guid ("" n'est pas un Guid valide)
    const payload = { ...userForm };
    if (!isEditing.value || !payload.id) {
      delete payload.id;
    }
    // On ne renvoie jamais le tenantId tel quel si on veut laisser le backend s'en charger 
    // ou s'assurer qu'il est valide.
    if (payload.tenantId === '') delete payload.tenantId;
    
    // Supprimer le mot de passe si on est en mode édition (pour l'instant on ne gère pas le reset ici)
    if (isEditing.value) delete payload.password;

    if (isEditing.value && payload.id) {
      await userApi.updateUser(payload.id, payload);
      toast.success('Utilisateur mis à jour.');
    } else {
      await userApi.createUser(payload);
      toast.success('Utilisateur créé avec succès.');
    }
    await fetchUsers();
    showModal.value = false;
  } catch (err) {
    toast.error('Échec de l\'opération.');
  } finally {
    saving.value = false;
  }
};

const confirmDelete = async (user: UserInfo) => {
  if (confirm(`Supprimer définitivement l'utilisateur ${user.firstName} ${user.lastName} ?`)) {
    try {
      await userApi.deleteUser(user.id);
      users.value = users.value.filter(u => u.id !== user.id);
      toast.success('Utilisateur supprimé.');
    } catch (err) {
      toast.error('Erreur lors de la suppression.');
    }
  }
};

const getRoleClass = (role: string) => {
  const r = role?.toLowerCase() || '';
  if (r.includes('admin')) return 'role-admin';
  if (r.includes('manager')) return 'role-manager';
  return 'role-user';
};

const formatDate = (dateStr: string | null | undefined) => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('fr-FR', {
    day: '2-digit', month: 'short', year: 'numeric'
  });
};
</script>

<template>
  <div class="admin-view">
    <!-- Header -->
    <div class="view-header">
      <div class="header-text">
        <h1>Gestion des Utilisateurs</h1>
        <p>Administration centralisée des accès et des rôles</p>
      </div>
      <div class="header-actions">
        <button class="btn-primary" @click="openAddModal">
          <i class="fas fa-user-plus"></i> Nouveau
        </button>
      </div>
    </div>

    <!-- Stats -->
    <div class="stats-grid">
      <StatCard label="Collaborateurs" :value="users.length" icon="fas fa-users" color="#3182ce" />
      <StatCard label="Actifs" :value="users.filter(u => u.isActive).length" icon="fas fa-user-check" color="#38a169" />
      <StatCard label="Administrateurs" :value="users.filter(u => u.role?.toLowerCase().includes('admin')).length" icon="fas fa-shield-alt" color="#805ad5" />
      <StatCard label="En attente" :value="0" icon="fas fa-clock" color="#d69e2e" />
    </div>

    <!-- Toolbar -->
    <div class="toolbar">
      <div class="search-section">
        <div class="search-input-wrapper">
          <i class="fas fa-search"></i>
          <input v-model="searchUser" placeholder="Rechercher par nom, prénom ou email..." @input="currentPage = 1" />
        </div>
      </div>
      <div class="filter-section" v-if="isSuperAdmin">
        <select v-model="selectedTenantFilter" class="tenant-filter">
          <option value="">Toutes les entreprises</option>
          <option v-for="t in tenants" :key="t.id" :value="t.id">{{ t.name }}</option>
        </select>
      </div>
    </div>

    <!-- Table Card -->
    <div class="content-card">
      <div class="table-container custom-scrollbar">
        <table class="konta-table">
          <thead>
            <tr>
              <th @click="handleSort('lastName')" class="sortable text-left">
                Utilisateur <i class="fas" :class="sortBy === 'lastName' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th @click="handleSort('role')" class="sortable">
                Rôle <i class="fas" :class="sortBy === 'role' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th>Statut</th>
              <th @click="handleSort('createdAt')" class="sortable">
                Enregistré le <i class="fas" :class="sortBy === 'createdAt' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th v-if="isSuperAdmin">Entreprise</th>
              <th class="text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in paginatedUsers" :key="user.id">
              <td>
                <div class="user-cell">
                  <div class="avatar-box">
                    {{ user.firstName?.charAt(0) }}{{ user.lastName?.charAt(0) }}
                  </div>
                  <div class="user-details">
                    <span class="full-name">{{ user.firstName }} {{ user.lastName }}</span>
                    <span class="email-sub">{{ user.email }}</span>
                  </div>
                </div>
              </td>
              <td>
                <span class="role-pill" :class="getRoleClass(user.role)">{{ user.role }}</span>
              </td>
              <td>
                <div class="status-indicator" :class="{ active: user.isActive }">
                  <span class="dot"></span>
                  {{ user.isActive ? 'Actif' : 'Bloqué' }}
                </div>
              </td>
              <td class="date-cell">{{ formatDate(user.createdAt) }}</td>
              <td v-if="isSuperAdmin" class="tenant-cell">
                {{ tenants.find(t => t.id === user.tenantId)?.name || 'Système' }}
              </td>
              <td class="actions text-right">
                <button class="action-btn edit" @click="openEditModal(user)" title="Modifier">
                  <i class="fas fa-pen"></i>
                </button>
                <button class="action-btn delete" @click="confirmDelete(user)" title="Supprimer">
                  <i class="fas fa-trash-alt"></i>
                </button>
              </td>
            </tr>
            <tr v-if="paginatedUsers.length === 0 && !loading">
              <td colspan="5" class="empty-state">
                <i class="fas fa-users-slash"></i>
                <p>Aucun utilisateur trouvé.</p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="card-footer" v-if="totalPages > 1">
        <div class="pagination">
          <button :disabled="currentPage === 1" @click="currentPage--" class="p-btn">
            <i class="fas fa-chevron-left"></i>
          </button>
          <span class="page-info">{{ currentPage }} / {{ totalPages }}</span>
          <button :disabled="currentPage === totalPages" @click="currentPage++" class="p-btn">
            <i class="fas fa-chevron-right"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- User Modal -->
    <BaseModal :show="showModal" :title="isEditing ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur'" @close="showModal = false">
      <form @submit.prevent="saveUser" class="konta-form">
        <div class="form-row">
          <div class="form-group">
            <label>Prénom</label>
            <input v-model="userForm.firstName" required placeholder="Ex: Jean" />
          </div>
          <div class="form-group">
            <label>Nom</label>
            <input v-model="userForm.lastName" required placeholder="Ex: Dupont" />
          </div>
        </div>

        <div class="form-group">
          <label>Email professionnel</label>
          <input type="email" v-model="userForm.email" required placeholder="jean.dupont@entreprise.com" :disabled="isEditing" />
        </div>

        <div class="form-group" v-if="!isEditing">
          <label>Mot de passe initial</label>
          <input type="password" v-model="userForm.password" required placeholder="Saisir un mot de passe temporaire" minlength="6" />
        </div>

        <div class="form-group" v-if="isSuperAdmin && !isEditing">
          <label>Assigner à une entreprise (Tenant)</label>
          <select v-model="userForm.tenantId">
            <option value="">Utiliser mon tenant</option>
            <option v-for="t in tenants" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <small v-if="userForm.tenantId" style="color: #718096; font-size: 0.75rem;">
            L'utilisateur sera rattaché à {{ tenants.find(t => t.id === userForm.tenantId)?.name }}
          </small>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Rôle</label>
            <select v-model="userForm.role">
              <option value="Admin">Administrateur</option>
              <option value="Manager">Manager</option>
              <option value="User">Collaborateur</option>
            </select>
          </div>
          <div class="form-group">
            <label>Statut</label>
            <select v-model="userForm.isActive">
              <option :value="true">Activé</option>
              <option :value="false">Désactivé</option>
            </select>
          </div>
        </div>

        <div class="modal-actions-inline">
          <button type="button" class="btn-cancel" @click="showModal = false">Annuler</button>
          <button type="submit" class="btn-submit" :disabled="saving">
            <i class="fas fa-save"></i> {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
          </button>
        </div>
      </form>
    </BaseModal>
  </div>
</template>

<style scoped>
.admin-view { display: flex; flex-direction: column; gap: 1.5rem; padding-bottom: 2rem; }
.view-header { display: flex; justify-content: space-between; align-items: center; }
.header-text h1 { font-size: 1.8rem; font-weight: 800; color: #1a202c; margin: 0; }
.header-text p { color: #718096; margin-top: 4px; }

.btn-primary {
  padding: 0.75rem 1.5rem; background: #1a202c; color: white; border: none; border-radius: 12px;
  font-weight: 700; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: all 0.2s;
}
.btn-primary:hover { background: #2d3748; transform: translateY(-1px); }

.stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.25rem; }
.toolbar {
  background: white; padding: 1.25rem 1.5rem; border-radius: 16px; border: 1px solid #e2e8f0;
  display: flex; justify-content: space-between; align-items: center;
}
.search-input-wrapper { position: relative; width: 400px; }
.search-input-wrapper i { position: absolute; left: 14px; top: 50%; transform: translateY(-50%); color: #a0aec0; }
.search-input-wrapper input {
  width: 100%; padding: 0.7rem 1rem 0.7rem 2.6rem; border: 1px solid #e2e8f0; border-radius: 10px;
  background-color: #f7fafc; outline: none; transition: all 0.2s;
}

.tenant-filter {
  padding: 0.7rem 1rem; border: 1px solid #e2e8f0; border-radius: 10px;
  background-color: white; outline: none; min-width: 200px;
}

.content-card {
  background: white; border-radius: 20px; border: 1px solid #e2e8f0;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05); overflow: hidden;
}
.table-container { max-height: 600px; overflow-y: auto; }
.konta-table { width: 100%; border-collapse: separate; border-spacing: 0; }
.konta-table th {
  padding: 1.25rem 1.5rem; font-size: 0.75rem; font-weight: 800; color: #4a5568;
  text-transform: uppercase; background-color: #f8fafc; border-bottom: 1px solid #e2e8f0;
  position: sticky; top: 0; z-index: 10; text-align: left;
}
.konta-table th.sortable { cursor: pointer; }
.konta-table th i { margin-left: 8px; opacity: 0.5; }
.konta-table td { padding: 1rem 1.5rem; border-bottom: 1px solid #edf2f7; vertical-align: middle; }

.user-cell { display: flex; align-items: center; gap: 12px; }
.avatar-box {
  width: 40px; height: 40px; background: #ebf8ff; color: #3182ce;
  border-radius: 12px; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 0.9rem;
}
.full-name { display: block; font-weight: 700; color: #2d3748; font-size: 0.95rem; }
.email-sub { font-size: 0.8rem; color: #718096; }

.role-pill {
  padding: 4px 10px; border-radius: 8px; font-size: 0.75rem; font-weight: 800;
}
.role-admin { background: #fff5f5; color: #c53030; }
.role-manager { background: #ebf8ff; color: #2b6cb0; }
.role-user { background: #f7fafc; color: #4a5568; }

.status-indicator { display: flex; align-items: center; gap: 8px; font-size: 0.8rem; font-weight: 700; color: #a0aec0; }
.status-indicator.active { color: #38a169; }
.status-indicator .dot { width: 8px; height: 8px; border-radius: 50%; background: #cbd5e0; }
.status-indicator.active .dot { background: #38a169; box-shadow: 0 0 0 3px rgba(56, 161, 105, 0.2); }

.action-btn {
  width: 32px; height: 32px; border-radius: 8px; border: 1px solid #e2e8f0;
  background: white; cursor: pointer; transition: all 0.2s; color: #718096; margin-left: 8px;
}
.action-btn:hover { transform: translateY(-1px); }
.action-btn.edit:hover { color: #3182ce; border-color: #3182ce; background: #ebf8ff; }
.action-btn.delete:hover { color: #e53e3e; border-color: #e53e3e; background: #fff5f5; }

.konta-form { display: flex; flex-direction: column; gap: 1.5rem; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.form-group { display: flex; flex-direction: column; gap: 0.5rem; }
.form-group label { font-size: 0.85rem; font-weight: 700; color: #4a5568; }
.form-group input, .form-group select {
  padding: 0.75rem 1rem; border: 1.5px solid #e2e8f0; border-radius: 10px; outline: none; transition: all 0.2s;
}
.form-group input:focus, .form-group select:focus { border-color: #3182ce; }

.modal-actions-inline {
  display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem;
}
.btn-submit {
  padding: 0.75rem 1.5rem; background: #1a202c; color: white; border: none; border-radius: 10px; font-weight: 700; cursor: pointer;
}
.btn-cancel {
  padding: 0.75rem 1.5rem; background: #f7fafc; color: #4a5568; border: 1.5px solid #e2e8f0; border-radius: 10px; font-weight: 700; cursor: pointer;
}

.card-footer { padding: 1.25rem; background: #f8fafc; display: flex; justify-content: center; }
.pagination { display: flex; align-items: center; gap: 1rem; }
.p-btn {
  width: 36px; height: 36px; border-radius: 8px; border: 1px solid #e2e8f0; background: white; cursor: pointer;
}
.p-btn:disabled { opacity: 0.5; }

.empty-state { text-align: center; padding: 4rem 0 !important; color: #cbd5e0; }
.empty-state i { font-size: 3rem; margin-bottom: 1rem; }

@media (max-width: 1024px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 640px) {
  .stats-grid { grid-template-columns: 1fr; }
  .toolbar .search-input-wrapper { width: 100%; }
  .form-row { grid-template-columns: 1fr; }
}
</style>
