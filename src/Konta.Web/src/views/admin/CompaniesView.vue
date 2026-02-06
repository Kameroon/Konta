<script setup lang="ts">
import { ref, onMounted, computed, reactive } from 'vue';
import { financeApi } from '@/api/finance.api';
import { tenantApi, type TenantResponse } from '@/api/tenant.api';
import { useAuthStore } from '@/stores/auth.store';
import { TierType } from '@/types/finance.types';
import type { Tier } from '@/types/finance.types';
import StatCard from '@/components/ui/StatCard.vue';
import BaseModal from '@/components/ui/BaseModal.vue';
import ConfirmModal from '@/components/ui/ConfirmModal.vue';
import { useToast } from 'vue-toastification';

const toast = useToast();
const authStore = useAuthStore();

/**
 * CompaniesView : Gestion polymorphe des Entreprises (Tenants) et des Tiers.
 */

interface DisplayItem {
  id: string;
  name: string;
  type: string;
  taxId: string;
  address: string;
  tenantId: string;
  isActive: boolean;
  createdAt: string;
  isTenant: boolean;
  industry?: string;
}

const companies = ref<Tier[]>([]);
const tenants = ref<TenantResponse[]>([]);
const searchCompany = ref('');
const selectedTenantFilter = ref('');
const loading = ref(true);
const saving = ref(false);

const isSuperAdmin = computed(() => authStore.user?.role === 'SuperAdmin');

// Ce que nous affichons réellement dans le tableau
const displayItems = computed(() => {
  if (isSuperAdmin.value && !selectedTenantFilter.value) {
    // Mode "Gestion des SaaS Clients" (Tenants)
    return tenants.value.map(t => ({
      id: t.id,
      name: t.name,
      type: t.plan,
      taxId: t.siret || '—',
      address: t.address || '—',
      tenantId: t.id,
      isActive: true, // Les tenants sont actifs par défaut ici
      createdAt: t.createdAt,
      isTenant: true,
      industry: t.industry
    }));
  }

  // Mode "Gestion des Tiers" (Clients/Fournisseurs)
  let source = companies.value;
  if (isSuperAdmin.value && selectedTenantFilter.value) {
    source = source.filter(c => c.tenantId === selectedTenantFilter.value);
  }

  return source.map(c => ({
    id: c.id,
    name: c.name,
    type: c.type === TierType.Client ? 'Client' : 'Fournisseur',
    taxId: c.taxId || '—',
    address: c.address || '—',
    tenantId: c.tenantId,
    isActive: c.isActive,
    createdAt: c.createdAt,
    isTenant: false,
    industry: ''
  }));
});

// Modale & Formulaire
const showModal = ref(false);
const isEditing = ref(false);

// Modern Deletion Confirmation
const showDeleteConfirm = ref(false);
const tierToDelete = ref<Tier | null>(null);
const tenantToDelete = ref<TenantResponse | null>(null);
const isDeleting = ref(false);
const tierForm = reactive<Partial<Tier>>({
  id: '',
  name: '',
  type: TierType.Client,
  email: '',
  taxId: '',
  address: '',
  isActive: true
});

// Tenant Edit Form
const showTenantModal = ref(false);
const tenantForm = reactive<Partial<TenantResponse>>({
  id: '',
  name: '',
  plan: '',
  industry: '',
  address: '',
  siret: ''
});

// Pagination et Tri
const currentPage = ref(1);
const itemsPerPage = ref(10);
const sortBy = ref('name');
const sortOrder = ref<'asc' | 'desc'>('asc');

onMounted(async () => {
  if (isSuperAdmin.value) {
    await fetchTenants();
  }
  await fetchCompanies();
});

const fetchTenants = async () => {
  try {
    tenants.value = await tenantApi.getTenants();
  } catch (err) {
    console.warn('Impossible de charger les tenants', err);
  }
};

const fetchCompanies = async () => {
  loading.value = true;
  try {
     companies.value = await financeApi.getTiers();
      //var fournisseurs = companies.filter(c => c.type === TierType.Supplier).length
      //console.log(companies);
      //console.log(fournisseurs);
  } catch (err) {
    toast.error('Erreur lors du chargement des entreprises.');
  } finally {
    loading.value = false;
  }
};

const sortedAndFilteredCompanies = computed(() => {
  let result = [...displayItems.value];

  if (searchCompany.value) {
    const s = searchCompany.value.toLowerCase();
    result = result.filter(c => 
      c.name.toLowerCase().includes(s) || 
      (c.taxId && c.taxId.toLowerCase().includes(s)) ||
      (c.address && c.address.toLowerCase().includes(s))
    );
  }

  result.sort((a: any, b: any) => {
    const valA = a[sortBy.value] || '';
    const valB = b[sortBy.value] || '';
    if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1;
    return 0;
  });

  return result;
});

const paginatedCompanies = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value;
  return sortedAndFilteredCompanies.value.slice(start, start + itemsPerPage.value);
});

const totalPages = computed(() => Math.ceil(sortedAndFilteredCompanies.value.length / itemsPerPage.value));

const handleSort = (field: string) => {
  if (sortBy.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortBy.value = field;
    sortOrder.value = 'asc';
  }
};

const openAddModal = () => {
  isEditing.value = false;
  Object.assign(tierForm, {
    id: '', name: '', type: TierType.Client, email: '', taxId: '', address: '', isActive: true
  });
  showModal.value = true;
};

const openEditModal = (tier: Tier) => {
  isEditing.value = true;
  Object.assign(tierForm, { ...tier });
  showModal.value = true;
};

// Ouvrir la modale d'édition pour un Tenant
const openTenantEditModal = (tenant: TenantResponse) => {
  Object.assign(tenantForm, { ...tenant });
  showTenantModal.value = true;
};

// Sauvegarder les modifications d'un Tenant
const saveTenant = async () => {
  saving.value = true;
  try {
    if (tenantForm.id) {
      await tenantApi.updateTenant(tenantForm.id, tenantForm);
      toast.success('Entreprise mise à jour.');
      await fetchTenants();
      showTenantModal.value = false;
    }
  } catch (err: any) {
    const errorMsg = err.response?.data?.message || err.message || 'Échec de la mise à jour.';
    toast.error(errorMsg);
  } finally {
    saving.value = false;
  }
};

const saveTier = async () => {
  saving.value = true;
  try {
    const payload = { ...tierForm };
    
    // Nettoyage de l'ID pour éviter l'erreur de format Guid "" sur le backend lors de la création
    if (!isEditing.value || !payload.id) {
      delete payload.id;
    }

    if (isEditing.value && payload.id) {
      await financeApi.updateTier(payload.id, payload);
      toast.success('Informations mises à jour.');
    } else {
      await financeApi.createTier(payload);
      toast.success('Nouvelle entreprise enregistrée.');
    }
    await fetchCompanies();
    showModal.value = false;
  } catch (err) {
    console.error('Erreur lors de l\'enregistrement du tiers:', err);
    toast.error('Échec de l\'opération.');
  } finally {
    saving.value = false;
  }
};

const confirmDelete = (tier: Tier) => {
  tierToDelete.value = tier;
  tenantToDelete.value = null;
  showDeleteConfirm.value = true;
};

// Confirmation de suppression d'un Tenant
const confirmDeleteTenant = (tenant: TenantResponse) => {
  tenantToDelete.value = tenant;
  tierToDelete.value = null;
  showDeleteConfirm.value = true;
};

const handleDelete = async () => {
  try {
    isDeleting.value = true;
    
    if (tierToDelete.value) {
      // Suppression d'un Tier
      await financeApi.deleteTier(tierToDelete.value.id);
      companies.value = companies.value.filter(c => c.id !== tierToDelete.value!.id);
      toast.success('Entreprise supprimée.');
    } else if (tenantToDelete.value) {
      // Suppression d'un Tenant
      await tenantApi.deleteTenant(tenantToDelete.value.id);
      tenants.value = tenants.value.filter(t => t.id !== tenantToDelete.value!.id);
      toast.success('Tenant supprimé.');
    }
    
    showDeleteConfirm.value = false;
  } catch (err: any) {
    const errorMsg = err.response?.data?.message || err.message || 'Erreur lors de la suppression.';
    toast.error(errorMsg);
  } finally {
    isDeleting.value = false;
    tierToDelete.value = null;
    tenantToDelete.value = null;
  }
};

const formatDate = (dateStr: string | null | undefined) => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('fr-FR', {
    day: '2-digit', month: 'short', year: 'numeric'
  });
};
</script>

<template>
  <div class="companies-view">
    <div class="view-header">
      <div class="header-text">
        <h1>Gestion des Entreprises</h1>
        <p>Répertoire centralisé des clients et fournisseurs</p>
      </div>
      <div class="header-actions">
        <button class="btn-primary" @click="openAddModal">
          <i class="fas fa-plus"></i> Nouvelle Fiche
        </button>
      </div>
    </div>

    <div class="stats-grid">
      <StatCard :label="isSuperAdmin && !selectedTenantFilter ? 'Total Entreprises' : 'Total Tiers'" :value="sortedAndFilteredCompanies.length" icon="fas fa-building" color="#3182ce" />
      <StatCard :label="isSuperAdmin && !selectedTenantFilter ? 'Plans Expertise' : 'Clients'" :value="sortedAndFilteredCompanies.filter(c => isSuperAdmin && !selectedTenantFilter ? c.type.toLowerCase().includes('expert') : c.type === 'Client').length" icon="fas fa-user-tie" color="#38a169" />
      <StatCard :label="isSuperAdmin && !selectedTenantFilter ? 'Plans Avancés' : 'Fournisseurs'" :value="sortedAndFilteredCompanies.filter(c => isSuperAdmin && !selectedTenantFilter ? c.type.toLowerCase().includes('avan') : c.type === 'Fournisseur').length" icon="fas fa-truck" color="#805ad5" />
      <StatCard label="Actifs" :value="sortedAndFilteredCompanies.filter(c => c.isActive).length" icon="fas fa-check-circle" color="#3182ce" />
    </div>

    <div class="toolbar">
      <div class="search-section">
        <div class="search-input-wrapper">
          <i class="fas fa-search"></i>
          <input v-model="searchCompany" placeholder="Rechercher par nom, SIRET ou adresse..." @input="currentPage = 1" />
        </div>
      </div>
      <div class="filter-section" v-if="isSuperAdmin">
        <select v-model="selectedTenantFilter" class="tenant-filter" @change="currentPage = 1">
          <option value="">Toutes les entreprises (Tenants)</option>
          <option v-for="t in tenants" :key="t.id" :value="t.id">{{ t.name }}</option>
        </select>
      </div>
      <div class="pagination-summary" v-if="!loading">
        <b>{{ paginatedCompanies.length }}</b> sur {{ sortedAndFilteredCompanies.length }} tiers
      </div>
    </div>

    <div class="content-card">
      <div class="table-container custom-scrollbar">
        <table class="konta-table">
          <thead>
            <tr>
              <th @click="handleSort('name')" class="sortable text-left">
                Entreprise <i class="fas" :class="sortBy === 'name' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th>Type</th>
              <th>SIRET</th>
              <th>Adresse</th>
              <th v-if="isSuperAdmin">Tenant</th>
              <th>Statut</th>
              <th @click="handleSort('createdAt')" class="sortable">
                Ajoutée le <i class="fas" :class="sortBy === 'createdAt' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th class="text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="comp in paginatedCompanies" :key="comp.id">
              <td>
                <div class="company-cell">
                  <div class="logo-box">{{ comp.name?.charAt(0) || '?' }}</div>
                  <div class="company-details">
                    <span class="company-name">{{ comp.name }}</span>
                    <span class="company-address" v-if="comp.isTenant && comp.industry">{{ comp.industry }}</span>
                  </div>
                </div>
              </td>
              <td>
                <span class="type-pill" :class="{ 'client': comp.type === 'Client' || comp.isTenant, 'supplier': comp.type === 'Fournisseur' }">
                  {{ comp.type }}
                </span>
              </td>
              <td class="tax-cell">{{ comp.taxId || '—' }}</td>
              <td class="address-cell">{{ comp.address || '—' }}</td>
              <td v-if="isSuperAdmin && !comp.isTenant" class="tenant-cell">
                <span class="t-badge">{{ tenants.find(t => t.id === comp.tenantId)?.name || '...' }}</span>
              </td>
              <td v-else-if="isSuperAdmin && comp.isTenant" class="tenant-cell">
                <span class="t-badge system">Plateforme</span>
              </td>
              <td>
                <div class="status-indicator" :class="{ active: comp.isActive }">
                  <span class="dot"></span>
                  {{ comp.isActive ? 'Actif' : 'Inactif' }}
                </div>
              </td>
              <td class="date-cell">{{ formatDate(comp.createdAt) }}</td>
              <td class="actions text-right">
                <button v-if="!comp.isTenant" class="action-btn edit" @click="openEditModal(comp as any)" title="Editer">
                  <i class="fas fa-pen"></i>
                </button>
                <button v-if="!comp.isTenant" class="action-btn delete" @click="confirmDelete(comp as any)" title="Supprimer">
                  <i class="fas fa-trash-alt"></i>
                </button>
                <button v-if="comp.isTenant" class="action-btn edit" @click="openTenantEditModal(tenants.find(t => t.id === comp.id)!)" title="Editer">
                  <i class="fas fa-pen"></i>
                </button>
                <button v-if="comp.isTenant" class="action-btn delete" @click="confirmDeleteTenant(tenants.find(t => t.id === comp.id)!)" title="Supprimer">
                  <i class="fas fa-trash-alt"></i>
                </button>
              </td>
            </tr>
            <tr v-if="paginatedCompanies.length === 0 && !loading">
              <td colspan="6" class="empty-state">
                <i class="fas fa-building"></i>
                <p>Aucun tiers répertorié.</p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

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

    <!-- Modale de confirmation moderne -->
    <ConfirmModal
      :show="showDeleteConfirm"
      :title="tenantToDelete ? 'Supprimer le Tenant' : 'Supprimer la fiche'"
      :message="tenantToDelete 
        ? `Supprimer définitivement le tenant '${tenantToDelete?.name}' ? Cette action est irréversible et supprimera toutes les données associées.`
        : `Supprimer définitivement l'entreprise '${tierToDelete?.name}' ? Cette action effacera toutes les données liées à ce tiers.`"
      confirmText="Supprimer"
      type="danger"
      :loading="isDeleting"
      @close="showDeleteConfirm = false"
      @confirm="handleDelete"
    />

    <!-- Tier Modal -->
    <BaseModal :show="showModal" :title="isEditing ? 'Editer la fiche tiers' : 'Nouveau tiers'" @close="showModal = false">
      <form @submit.prevent="saveTier" class="konta-form">
        <div class="form-group">
          <label>Dénomination sociale</label>
          <input v-model="tierForm.name" required placeholder="Ex: Konta Inc." />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Type de relation</label>
            <select v-model="tierForm.type">
              <option :value="TierType.Client">Client</option>
              <option :value="TierType.Supplier">Fournisseur</option>
            </select>
          </div>
          <div class="form-group">
            <label>SIRET / Numéro de TVA</label>
            <input v-model="tierForm.taxId" placeholder="Ex: FR123456789" />
          </div>
        </div>

        <div class="form-group">
          <label>Email de contact (Facturation)</label>
          <input type="email" v-model="tierForm.email" placeholder="contact@entreprise.com" />
        </div>

        <div class="form-group">
          <label>Adresse du siège</label>
          <input v-model="tierForm.address" placeholder="Ex: 123 rue de la Paix, Paris" />
        </div>

        <div class="form-group">
          <label>Statut</label>
          <select v-model="tierForm.isActive">
            <option :value="true">Actif</option>
            <option :value="false">Inactif (Bloqué)</option>
          </select>
        </div>

        <div class="modal-actions-inline">
          <button type="button" class="btn-cancel" @click="showModal = false">Annuler</button>
          <button type="submit" class="btn-submit" :disabled="saving">
            <i class="fas fa-save"></i> {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
          </button>
        </div>
      </form>
    </BaseModal>

    <!-- Tenant Edit Modal -->
    <BaseModal :show="showTenantModal" title="Modifier l'entreprise" @close="showTenantModal = false">
      <form @submit.prevent="saveTenant" class="konta-form">
        <div class="form-group">
          <label>Nom de l'entreprise</label>
          <input v-model="tenantForm.name" required placeholder="Ex: Konta Platform" />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Plan d'abonnement</label>
            <select v-model="tenantForm.plan">
              <option value="discovery">Découverte</option>
              <option value="basic">Basique</option>
              <option value="advanced">Avancé</option>
              <option value="premium">Premium</option>
              <option value="expertise">Expertise</option>
            </select>
          </div>
          <div class="form-group">
            <label>SIRET</label>
            <input v-model="tenantForm.siret" placeholder="Ex: 78467169500079" />
          </div>
        </div>

        <div class="form-group">
          <label>Secteur d'activité</label>
          <input v-model="tenantForm.industry" placeholder="Ex: Technologie, Santé, Finance..." />
        </div>

        <div class="form-group">
          <label>Adresse du siège</label>
          <input v-model="tenantForm.address" placeholder="Ex: 123 rue de la Paix, Paris" />
        </div>

        <div class="modal-actions-inline">
          <button type="button" class="btn-cancel" @click="showTenantModal = false">Annuler</button>
          <button type="submit" class="btn-submit" :disabled="saving">
            <i class="fas fa-save"></i> {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
          </button>
        </div>
      </form>
    </BaseModal>
  </div>
</template>

<style scoped>
.companies-view { display: flex; flex-direction: column; gap: 1.5rem; padding-bottom: 2rem; }
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
  display: flex; justify-content: space-between; align-items: center;
  background: white; padding: 1.25rem 1.5rem; border-radius: 16px; border: 1px solid #e2e8f0;
}
.search-input-wrapper { position: relative; width: 400px; }
.search-input-wrapper i { position: absolute; left: 14px; top: 50%; transform: translateY(-50%); color: #a0aec0; }
.search-input-wrapper input {
  width: 100%; padding: 0.7rem 1rem 0.7rem 2.6rem; border: 1px solid #e2e8f0; border-radius: 10px;
  background-color: #f7fafc; outline: none; transition: all 0.2s;
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
.konta-table td { padding: 1.2rem 1.5rem; border-bottom: 1px solid #edf2f7; vertical-align: middle; }

.company-cell { display: flex; align-items: center; gap: 12px; }
.logo-box {
  width: 44px; height: 44px; background: #ebf8ff; color: #3182ce;
  border-radius: 12px; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 1rem; border: 1px solid #bee3f8;
}
.company-name { display: block; font-weight: 700; color: #1a202c; font-size: 1rem; }
.company-address { font-size: 0.8rem; color: #718096; display: block; margin-top: 2px; }

.type-pill {
  padding: 4px 10px; border-radius: 8px; font-size: 0.75rem; font-weight: 800;
}
.type-pill.client { background: #1a202c; color: white; }
.type-pill.supplier { background: #f7fafc; color: #4a5568; border: 1px solid #e2e8f0; }

.status-indicator { display: flex; align-items: center; gap: 8px; font-size: 0.8rem; font-weight: 700; color: #a0aec0; }
.status-indicator.active { color: #38a169; }
.status-indicator .dot { width: 8px; height: 8px; border-radius: 50%; background: #cbd5e0; }
.status-indicator.active .dot { background: #38a169; box-shadow: 0 0 0 3px rgba(56, 161, 105, 0.2); }

.action-btn {
  width: 34px; height: 34px; border-radius: 8px; border: 1px solid #e2e8f0;
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
  .toolbar { flex-direction: column; gap: 1rem; align-items: stretch; }
  .search-input-wrapper { width: 100%; }
}

.tenant-filter {
  padding: 0.7rem 1rem; border: 1px solid #e2e8f0; border-radius: 10px;
  background-color: white; outline: none; min-width: 250px; cursor: pointer;
  font-weight: 600; color: #4a5568;
}

.t-badge {
  background: #f7fafc; color: #4a5568; padding: 4px 8px; border-radius: 6px;
  font-size: 0.75rem; font-weight: 700; border: 1px solid #e2e8f0;
}
</style>
