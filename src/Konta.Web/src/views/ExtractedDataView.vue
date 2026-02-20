<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { documentsApi } from '@/api/documents.api';
import type { ExtractionJob, ExtractedInvoice, ExtractedRib } from '@/types/document.types';
import { DocumentType, JobStatus } from '@/types/document.types';
import StatCard from '@/components/ui/StatCard.vue';
import BaseModal from '@/components/ui/BaseModal.vue';
import ConfirmModal from '@/components/ui/ConfirmModal.vue';
import { useToast } from 'vue-toastification';

const toast = useToast();

/**
 * ExtractedDataView : Consultation des données issues de l'OCR.
 * Refondu avec Pagination, Tri et Téléchargement PDF.
 */

const extractedData = ref<ExtractionJob[]>([]);
const searchData = ref('');
const loading = ref(true);

// Détails modal
const showDetailsModal = ref(false);
const detailLoading = ref(false);
const selectedJob = ref<ExtractionJob | null>(null);
const invoiceData = ref<ExtractedInvoice | null>(null);
const ribData = ref<ExtractedRib | null>(null);

// Pagination et Tri
const currentPage = ref(1);
const itemsPerPage = ref(10);
const sortBy = ref('createdAt');
const sortOrder = ref<'asc' | 'desc'>('desc');

// Confirmation Deletion
const showDeleteConfirm = ref(false);
const itemToDelete = ref<ExtractionJob | null>(null);
const deleting = ref(false);

onMounted(async () => {
  await fetchData();
});

const fetchData = async () => {
  loading.value = true;
  try {
    extractedData.value = await documentsApi.getJobs();
  } catch (err) {
    console.error('Erreur chargement:', err);
    toast.error('Erreur lors du chargement des jobs.');
  } finally {
    loading.value = false;
  }
};

const refreshData = async () => {
  await fetchData();
  currentPage.value = 1;
  toast.success('Données actualisées');
};

const sortedAndFilteredData = computed(() => {
  let result = [...extractedData.value];

  if (searchData.value) {
    const s = searchData.value.toLowerCase();
    result = result.filter(item => 
      item.fileName.toLowerCase().includes(s) || 
      (item.detectedType && item.detectedType.toLowerCase().includes(s))
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

const totalPages = computed(() => Math.ceil(sortedAndFilteredData.value.length / itemsPerPage.value));
const paginatedData = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value;
  return sortedAndFilteredData.value.slice(start, start + itemsPerPage.value);
});

const handleSort = (field: string) => {
  if (sortBy.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortBy.value = field;
    sortOrder.value = 'desc';
  }
};

const formatDate = (dateStr: string | null | undefined) => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('fr-FR', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
};

const downloadPdf = async (item: ExtractionJob) => {
  try {
    toast.info(`Téléchargement de ${item.fileName}...`);
    await documentsApi.downloadDocument(item.id, item.fileName);
    toast.success('Téléchargement terminé.');
  } catch (err) {
    toast.error('Échec du téléchargement du PDF.');
  }
};

const viewDetails = async (job: ExtractionJob) => {
  if (job.status !== JobStatus.Completed) return;
  
  selectedJob.value = job;
  showDetailsModal.value = true;
  detailLoading.value = true;
  invoiceData.value = null;
  ribData.value = null;

  try {
    if (job.detectedType === DocumentType.Invoice) {
      invoiceData.value = await documentsApi.getInvoiceResult(job.id);
    } else if (job.detectedType === DocumentType.Rib) {
      ribData.value = await documentsApi.getRibResult(job.id);
    }
  } catch (err) {
    console.error('Erreur détails:', err);
    toast.error('Impossible de charger les résultats d\'extraction.');
  } finally {
    detailLoading.value = false;
  }
};

const formatCurrency = (amount?: number) => {
  if (amount === undefined || amount === null) return '—';
  return new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(amount);
};

const confirmDelete = (item: ExtractionJob) => {
  itemToDelete.value = item;
  showDeleteConfirm.value = true;
};

const handleDelete = async () => {
  if (!itemToDelete.value) return;
  
  try {
    deleting.value = true;
    await documentsApi.deleteJob(itemToDelete.value.id);
    extractedData.value = extractedData.value.filter(e => e.id !== itemToDelete.value!.id);
    toast.success('Extraction supprimée.');
    showDeleteConfirm.value = false;
  } catch (err: any) {
    const errorMsg = err.response?.data?.message || err.message || 'Erreur lors de la suppression.';
    toast.error(errorMsg);
  } finally {
    deleting.value = false;
    itemToDelete.value = null;
  }
};
</script>

<template>
  <div class="extracted-data-view">
    <div class="view-header">
      <div class="header-text">
        <h1>Données Extraites</h1>
        <p>Suivi et gestion des flux d'extraction de documents</p>
      </div>
      <div class="header-actions">
        <button class="btn-refresh" @click="refreshData" :disabled="loading">
          <i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i> Actualiser
        </button>
        <button class="btn-export">
          <i class="fas fa-file-excel"></i> Export CSV
        </button>
      </div>
    </div>

    <div class="stats-row">
      <StatCard label="Total Jobs" :value="extractedData.length" icon="fas fa-layer-group" color="#3182ce" />
      <StatCard label="Réussis" :value="extractedData.filter(e => e.status === 'Completed').length" icon="fas fa-check-circle" color="#38a169" />
      <StatCard label="Échecs" :value="extractedData.filter(e => e.status === 'Failed').length" icon="fas fa-times-circle" color="#e53e3e" />
      <StatCard label="En cours" :value="extractedData.filter(e => e.status === 'Pending' || e.status === 'Processing').length" icon="fas fa-spinner" color="#d69e2e" />
    </div>

    <div class="toolbar">
      <div class="search-section">
        <div class="search-input-wrapper">
          <i class="fas fa-search"></i>
          <input v-model="searchData" placeholder="Rechercher par nom ou type..." @input="currentPage = 1" />
        </div>
      </div>
      <div class="pagination-summary">
        Affichage de <b>{{ paginatedData.length }}</b> sur {{ sortedAndFilteredData.length }} jobs
      </div>
    </div>

    <div class="content-card">
      <div v-if="loading && extractedData.length === 0" class="loading-overlay">
        <div class="spinner"></div>
      </div>
      
      <div class="table-container custom-scrollbar">
        <table class="konta-table">
          <thead>
            <tr>
              <th @click="handleSort('fileName')" class="sortable">
                Nom du Fichier <i class="fas" :class="sortBy === 'fileName' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th @click="handleSort('detectedType')" class="sortable">
                Type Document <i class="fas" :class="sortBy === 'detectedType' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th>Statut</th>
              <th @click="handleSort('createdAt')" class="sortable">
                Ajouté le <i class="fas" :class="sortBy === 'createdAt' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th @click="handleSort('processedAt')" class="sortable">
                Traité le <i class="fas" :class="sortBy === 'processedAt' ? (sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down') : 'fa-sort'"></i>
              </th>
              <th>Données</th>
              <th class="text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in paginatedData" :key="item.id">
              <td class="file-cell">
                <i class="fas fa-file-pdf pdf-icon"></i>
                <span class="file-name" :title="item.fileName">{{ item.fileName }}</span>
              </td>
              <td>
                <span class="type-badge" :class="item.detectedType?.toLowerCase()">
                  {{ item.detectedType || 'Unknown' }}
                </span>
              </td>
              <td>
                <div class="status-pill-modern" :class="item.status.toLowerCase()">
                  <span class="pill-dot"></span>
                  {{ item.status }}
                </div>
              </td>
              <td class="date-cell">{{ formatDate(item.createdAt) }}</td>
              <td class="date-cell">{{ formatDate(item.processedAt) }}</td>
              <td>
                <button 
                  v-if="item.status === JobStatus.Completed" 
                  class="btn-view-results" 
                  @click="viewDetails(item)"
                >
                  <i class="fas fa-eye"></i> Voir détails
                </button>
                <span v-else class="text-muted">—</span>
              </td>
              <td class="actions text-right">
                <button class="action-btn download" @click="downloadPdf(item)" title="Télécharger">
                  <i class="fas fa-download"></i>
                </button>
                <button class="action-btn delete" @click="confirmDelete(item)" title="Supprimer">
                  <i class="fas fa-trash-alt"></i>
                </button>
              </td>
            </tr>
            <tr v-if="paginatedData.length === 0 && !loading">
              <td colspan="6" class="empty-state">
                <i class="fas fa-folder-open"></i>
                <p>Aucune donnée trouvée.</p>
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
          <span class="page-info">Page {{ currentPage }} sur {{ totalPages }}</span>
          <button :disabled="currentPage === totalPages" @click="currentPage++" class="p-btn">
            <i class="fas fa-chevron-right"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- Confirmation de suppression moderne -->
    <ConfirmModal
      :show="showDeleteConfirm"
      title="Supprimer l'extraction"
      :message="`Voulez-vous vraiment supprimer l'extraction de '${itemToDelete?.fileName}' ? Cette action est irréversible.`"
      confirmText="Supprimer définitivement"
      type="danger"
      :loading="deleting"
      @close="showDeleteConfirm = false"
      @confirm="handleDelete"
    />

    <!-- Details Modal -->
    <BaseModal 
      :show="showDetailsModal" 
      :title="selectedJob?.detectedType === DocumentType.Invoice ? 'Détails de la Facture' : 'Détails du RIB'" 
      @close="showDetailsModal = false"
      width="600px"
    >
      <div v-if="detailLoading" class="modal-loading">
        <div class="spinner"></div>
        <p>Récupération des données extraites...</p>
      </div>
      <div v-else-if="invoiceData" class="ocr-details">
        <div class="detail-header">
          <i class="fas fa-file-invoice detail-icon"></i>
          <div class="header-info">
            <span class="doc-title">{{ selectedJob?.fileName }}</span>
            <div class="doc-meta">
              <span class="doc-type">Type: Facture / Devis</span>
              <span class="conf-badge" v-if="invoiceData.confidenceScore">
                Score: {{ invoiceData.confidenceScore }}%
              </span>
            </div>
          </div>
        </div>

        <div class="details-grid">
          <div class="detail-item full">
            <label>Numéro de Facture</label>
            <p class="detail-value">{{ invoiceData.invoiceNumber || 'Non détecté' }}</p>
          </div>
          
          <div class="detail-item">
            <label>SIREN Émetteur</label>
            <p class="detail-value">{{ invoiceData.vendorSiret || 'Non détecté' }}</p>
          </div>
          <div class="detail-item">
            <label>SIREN Destinataire</label>
            <p class="detail-value">{{ invoiceData.customerSiret || 'Non détecté' }}</p>
          </div>

          <div class="detail-item full">
            <label>Numéro de TVA</label>
            <p class="detail-value">{{ invoiceData.vendorVatNumber || 'Non détecté' }}</p>
          </div>

          <div class="detail-item">
            <label>Date d'Émission</label>
            <p class="detail-value">{{ invoiceData.invoiceDate ? new Date(invoiceData.invoiceDate).toLocaleDateString('fr-FR') : 'Non détecté' }}</p>
          </div>
          <div class="detail-item">
            <label>Date d'Échéance</label>
            <p class="detail-value text-primary">{{ invoiceData.dueDate ? new Date(invoiceData.dueDate).toLocaleDateString('fr-FR') : 'Non détecté' }}</p>
          </div>

          <div class="detail-item full section-divider">
            <label>Informations Complémentaires</label>
          </div>

          <div class="detail-item">
            <label>Entreprise / Fournisseur</label>
            <p class="detail-value">{{ invoiceData.vendorName || 'Non détecté' }}</p>
          </div>
          <div class="detail-item">
            <label>Devise</label>
            <p class="detail-value">{{ invoiceData.currency || 'EUR' }}</p>
          </div>
        </div>

        <div class="amounts-summary">
          <div class="amount-row">
            <span>Montant HT</span>
            <span class="amount-val">{{ formatCurrency(invoiceData.totalAmountHt) }}</span>
          </div>
          <div class="amount-row">
            <span>TVA</span>
            <span class="amount-val">{{ formatCurrency(invoiceData.vatAmount) }}</span>
          </div>
          <div class="amount-row total">
            <span>TOTAL TTC</span>
            <span class="amount-val">{{ formatCurrency(invoiceData.totalAmountTtc) }}</span>
          </div>
        </div>
      </div>

      <div v-else-if="ribData" class="ocr-details">
        <div class="detail-header">
          <i class="fas fa-university detail-icon"></i>
          <div class="header-info">
            <span class="doc-title">{{ selectedJob?.fileName }}</span>
            <span class="doc-type">Type: RIB / Coordonnées Bancaires</span>
          </div>
        </div>

        <div class="details-grid">
          <div class="detail-item full">
            <label>Titulaire du compte</label>
            <p class="detail-value">{{ ribData.accountHolder || 'Non détecté' }}</p>
          </div>
          <div class="detail-item full">
            <label>IBAN</label>
            <p class="detail-value iban">{{ ribData.iban || 'Non détecté' }}</p>
          </div>
          <div class="detail-item">
            <label>BIC / SWIFT</label>
            <p class="detail-value">{{ ribData.bic || 'Non détecté' }}</p>
          </div>
          <div class="detail-item">
            <label>Banque</label>
            <p class="detail-value">{{ ribData.bankName || 'Non détecté' }}</p>
          </div>
        </div>
      </div>

      <div v-else class="empty-results">
        <i class="fas fa-exclamation-circle"></i>
        <p>Aucune donnée n'a pu être extraite de ce document.</p>
      </div>

      <div class="modal-footer">
        <button class="btn-close-modal" @click="showDetailsModal = false">Fermer</button>
        <button class="btn-download-pdf" @click="selectedJob && downloadPdf(selectedJob)">
          <i class="fas fa-file-pdf"></i> Voir le PDF
        </button>
      </div>
    </BaseModal>
  </div>
</template>

<style scoped>
.extracted-data-view { display: flex; flex-direction: column; gap: 1.5rem; padding-bottom: 2rem; }
.view-header { display: flex; justify-content: space-between; align-items: center; }
.header-text h1 { font-size: 1.8rem; font-weight: 800; color: #1a202c; margin: 0; }
.header-text p { color: #718096; margin-top: 4px; }
.header-actions { display: flex; gap: 10px; }

.btn-refresh, .btn-export {
  padding: 0.75rem 1.25rem; border-radius: 12px; font-weight: 700;
  display: flex; align-items: center; gap: 8px; cursor: pointer; transition: all 0.2s;
}
.btn-refresh { background: white; border: 2px solid #e2e8f0; color: #4a5568; }
.btn-refresh:hover { background: #f7fafc; border-color: #cbd5e0; }
.btn-export { background-color: #1a202c; color: white; border: none; }
.btn-export:hover { background-color: #2d3748; transform: translateY(-1px); }

.stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.25rem; }
.toolbar {
  display: flex; justify-content: space-between; align-items: center;
  background: white; padding: 1rem 1.5rem; border-radius: 16px; border: 1px solid #e2e8f0;
}
.search-input-wrapper { position: relative; width: 350px; }
.search-input-wrapper i { position: absolute; left: 14px; top: 50%; transform: translateY(-50%); color: #a0aec0; }
.search-input-wrapper input {
  width: 100%; padding: 0.7rem 1rem 0.7rem 2.6rem; border: 1px solid #e2e8f0; border-radius: 10px;
  background-color: #f7fafc; outline: none; transition: all 0.2s;
}
.search-input-wrapper input:focus { border-color: #3182ce; background-color: white; box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.15); }
.pagination-summary { font-size: 0.9rem; color: #4a5568; }

.content-card {
  background: white; border-radius: 20px; border: 1px solid #e2e8f0;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05); overflow: hidden; position: relative;
}
.table-container { max-height: 600px; overflow-y: auto; }
.konta-table { width: 100%; border-collapse: separate; border-spacing: 0; }
.konta-table th {
  padding: 1.25rem 1.5rem; font-size: 0.75rem; font-weight: 800; color: #4a5568;
  text-transform: uppercase; letter-spacing: 0.05em; background-color: #f8fafc;
  border-bottom: 1px solid #e2e8f0; position: sticky; top: 0; z-index: 10; text-align: left;
}
.konta-table th.sortable { cursor: pointer; }
.konta-table th.sortable:hover { background-color: #edf2f7; }
.konta-table th i { margin-left: 8px; opacity: 0.5; font-size: 0.8rem; }
.konta-table td { padding: 1.1rem 1.5rem; border-bottom: 1px solid #edf2f7; vertical-align: middle; font-size: 0.9rem; color: #2d3748; }
.konta-table tr:hover td { background-color: #f7fafc; }

.file-cell { display: flex; align-items: center; gap: 12px; max-width: 300px; }
.pdf-icon { color: #e53e3e; font-size: 1.3rem; }
.file-name { font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.type-badge { padding: 4px 10px; border-radius: 8px; font-size: 0.75rem; font-weight: 800; background: #edf2f7; color: #4a5568; }
.type-badge.invoice { background: #ebf8ff; color: #2b6cb0; }

.status-pill-modern {
  display: inline-flex; align-items: center; gap: 8px; padding: 6px 12px; border-radius: 99px; font-size: 0.75rem; font-weight: 800;
}
.pill-dot { width: 6px; height: 6px; border-radius: 50%; }
.status-pill-modern.completed { background: #f0fff4; color: #22543d; }
.status-pill-modern.completed .pill-dot { background: #38a169; }
.status-pill-modern.failed { background: #fff5f5; color: #822727; }
.status-pill-modern.failed .pill-dot { background: #e53e3e; }
.status-pill-modern.processing, .status-pill-modern.pending { background: #fffaf0; color: #744210; }
.status-pill-modern.processing .pill-dot { background: #d69e2e; animation: pulse 1.5s infinite; }

@keyframes pulse { 0% { scale: 1; opacity: 1; } 50% { scale: 1.5; opacity: 0.5; } 100% { scale: 1; opacity: 1; } }

.date-cell { color: #718096; font-size: 0.85rem; font-weight: 500; }
.text-right { text-align: right; }
.actions { display: flex; justify-content: flex-end; gap: 8px; }
.action-btn {
  width: 36px; height: 36px; border-radius: 10px; border: 1px solid #e2e8f0;
  display: flex; align-items: center; justify-content: center; background: white; cursor: pointer; transition: all 0.2s; color: #4a5568;
}
.action-btn:hover { transform: translateY(-2px); box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
.action-btn.download:hover { color: #3182ce; border-color: #3182ce; background: #ebf8ff; }
.action-btn.delete:hover { color: #e53e3e; border-color: #e53e3e; background: #fff5f5; }

.card-footer { padding: 1.25rem 1.5rem; background: #f8fafc; border-top: 1px solid #e2e8f0; display: flex; justify-content: center; }
.pagination { display: flex; align-items: center; gap: 1.5rem; }
.p-btn {
  width: 40px; height: 40px; border-radius: 10px; border: 1px solid #e2e8f0;
  background: white; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.2s;
}
.p-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.page-info { font-weight: 700; color: #4a5568; font-size: 0.9rem; }

.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #f1f1f1; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e0; border-radius: 10px; }
.empty-state { text-align: center; padding: 5rem 0 !important; color: #a0aec0; }
.empty-state i { font-size: 3rem; margin-bottom: 1rem; }
.empty-state p { font-weight: 600; font-size: 1.1rem; }

.loading-overlay {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(255, 255, 255, 0.7); display: flex; justify-content: center; align-items: center; z-index: 20;
}
.spinner { width: 40px; height: 40px; border: 4px solid #f3f3f3; border-top: 4px solid #3182ce; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

@media (max-width: 768px) {
  .stats-row { grid-template-columns: 1fr; }
  .view-header { flex-direction: column; gap: 1rem; align-items: flex-start; }
  .toolbar { flex-direction: column; gap: 1rem; }
  .search-input-wrapper { width: 100%; }
}
.btn-view-results {
  background: #ebf8ff; color: #2b6cb0; border: 1px solid #bee3f8;
  padding: 6px 12px; border-radius: 8px; font-size: 0.8rem; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; gap: 6px; transition: all 0.2s;
}
.btn-view-results:hover { background: #bee3f8; transform: translateY(-1px); }

.modal-loading { text-align: center; padding: 3rem 0; color: #718096; }
.modal-loading .spinner { margin: 0 auto 1rem; }

.ocr-details { display: flex; flex-direction: column; gap: 1.5rem; }
.detail-header { display: flex; align-items: center; gap: 15px; padding-bottom: 1.5rem; border-bottom: 1px solid #edf2f7; }
.detail-icon { font-size: 2rem; color: #3182ce; }
.header-info { display: flex; flex-direction: column; }
.doc-title { font-weight: 800; color: #1a202c; font-size: 1.1rem; }
.doc-type { font-size: 0.8rem; color: #718096; font-weight: 600; }

.details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.detail-item { display: flex; flex-direction: column; gap: 4px; }
.detail-item.full { grid-column: span 2; }
.detail-item label { font-size: 0.75rem; font-weight: 800; color: #a0aec0; text-transform: uppercase; }
.detail-value { font-weight: 700; color: #2d3748; margin: 0; font-size: 1rem; }
.detail-value.iban { font-family: 'Courier New', Courier, monospace; letter-spacing: 1px; }

.amounts-summary {
  background: #f7fafc; padding: 1.5rem; border-radius: 12px; border: 1px solid #e2e8f0;
  display: flex; flex-direction: column; gap: 10px;
}
.amount-row { display: flex; justify-content: space-between; font-weight: 600; color: #4a5568; }
.amount-row.total { 
  margin-top: 5px; padding-top: 10px; border-top: 2px dashed #cbd5e0;
  font-size: 1.25rem; color: #1a202c; font-weight: 900;
}
.amount-val { color: #2d3748; }
.total .amount-val { color: #3182ce; }
.text-primary { color: #3182ce; }
.section-divider { margin-top: 1rem; border-top: 1px solid #edf2f7; padding-top: 1rem; }
.doc-meta { display: flex; align-items: center; gap: 10px; }
.conf-badge {
  background: #f0fff4; color: #22543d; padding: 2px 8px; border-radius: 6px;
  font-size: 0.7rem; font-weight: 800; border: 1px solid #c6f6d5;
}

.modal-footer { display: flex; justify-content: flex-end; gap: 12px; margin-top: 1rem; }
.btn-close-modal { padding: 0.75rem 1.5rem; border-radius: 10px; background: #f7fafc; border: 1px solid #e2e8f0; font-weight: 700; cursor: pointer; }
.btn-download-pdf { padding: 0.75rem 1.5rem; border-radius: 10px; background: #1a202c; color: white; border: none; font-weight: 700; cursor: pointer; display: flex; align-items: center; gap: 8px; }

.text-muted { color: #cbd5e0; }
.empty-results { text-align: center; padding: 3rem 0; color: #a0aec0; }
.empty-results i { font-size: 2.5rem; margin-bottom: 1rem; }

@media (max-width: 640px) {
  .details-grid { grid-template-columns: 1fr; }
  .detail-item.full { grid-column: auto; }
}
</style>
