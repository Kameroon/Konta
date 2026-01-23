<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { documentsApi } from '@/api/documents.api';
import { JobStatus, type ExtractionJob, type ExtractedInvoice } from '@/types/document.types';
import { useToast } from 'vue-toastification';
import DocumentUpload from '@/components/ui/DocumentUpload.vue';
import LoadingOverlay from '@/components/ui/LoadingOverlay.vue';

/**
 * DocumentsView : Gestion documentaire et OCR.
 * Permet d'envoyer des factures et d'en extraire les données via l'IA.
 */

const authStore = useAuthStore();
const toast = useToast();

const uploading = ref(false);
const activeJobs = ref<Map<string, ExtractionJob>>(new Map());
const results = ref<ExtractedInvoice[]>([]);

/**
 * Gère l'upload d'un nouveau document.
 */
const handleUpload = async (file: File) => {
  if (!authStore.user?.tenantId) return;

  uploading.value = true;
  try {
    const { jobId } = await documentsApi.uploadDocument(file, authStore.user.tenantId);
    toast.success('Document envoyé en extraction.');
    
    // Suivi du job
    trackJob(jobId);
  } catch (err) {
    toast.error("Échec de l'envoi du document.");
  } finally {
    uploading.value = false;
  }
};

/**
 * Tracker de job (Polling maîtrisé).
 */
const trackJob = async (jobId: string) => {
  const poll = async () => {
    try {
      const job = await documentsApi.getJobStatus(jobId);
      activeJobs.value.set(jobId, job);

      if (job.status === JobStatus.Completed) {
        toast.info(`Extraction terminée pour ${job.fileName}. Analyse des résultats...`);
        const result = await documentsApi.getInvoiceResult(jobId);
        
        if (result) {
          console.log('[DocumentsView] Résultat reçu:', result);
          results.value.unshift(result);
          toast.success(`Facture extraite avec succès.`);
        } else {
          console.warn('[DocumentsView] Job terminé mais aucun résultat retourné pour:', jobId);
          toast.warning(`Aucune donnée n'a été extraite de ${job.fileName}.`);
        }
        activeJobs.value.delete(jobId);
      } else if (job.status === JobStatus.Failed) {
        toast.error(`Échec de l'extraction : ${job.fileName}`);
        activeJobs.value.delete(jobId);
      } else {
        // Continuer le polling après 2 secondes
        setTimeout(poll, 2000);
      }
    } catch (err) {
      console.error('[OCR Tracking] Erreur de polling', err);
      activeJobs.value.delete(jobId);
    }
  };

  poll();
};

/**
 * Formate les montants.
 */
const formatCurrency = (amount?: number, currency = 'EUR') => {
  if (amount === undefined || amount === null) return '-';
  return new Intl.NumberFormat('fr-FR', { style: 'currency', currency }).format(amount);
};
</script>

<template>
  <div class="documents-view">
    <header class="section-header">
      <h1>Téléchargement & OCR</h1>
      <p>Envoyez vos factures pour une analyse automatique par l'intelligence artificielle.</p>
    </header>

      <!-- Zone d'Upload -->
      <section class="upload-section">
        <DocumentUpload @upload="handleUpload" />
        <LoadingOverlay :active="uploading" message="Téléchargement du fichier..." />
        
        <!-- Résultats immédiats (Session actuelle) -->
        <div v-if="results.length > 0" class="recent-results fade-in">
          <div class="results-header">
            <h3>Dernières extractions</h3>
            <span class="count">{{ results.length }} document(s)</span>
          </div>
          
          <div class="results-container custom-scrollbar">
            <table class="results-table">
              <thead>
                <tr>
                  <th>Vendeur</th>
                  <th>N° Facture</th>
                  <th>Date</th>
                  <th class="text-right">Total TTC</th>
                  <th class="text-right">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="invoice in results" :key="invoice.id" class="result-row">
                  <td class="vendor-cell">
                    <i class="fas fa-file-invoice-dollar"></i>
                    <strong>{{ invoice.vendorName || 'Inconnu' }}</strong>
                  </td>
                  <td>{{ invoice.invoiceNumber || '—' }}</td>
                  <td>{{ invoice.invoiceDate ? new Date(invoice.invoiceDate).toLocaleDateString() : '—' }}</td>
                  <td class="text-right amount">{{ formatCurrency(invoice.totalAmountTtc, invoice.currency) }}</td>
                  <td class="text-right">
                    <router-link to="/app/archive" class="btn-goto">Détails</router-link>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <!-- Suivi des extractions en cours -->
      <section v-if="activeJobs.size > 0" class="active-jobs">
        <h3>Extractions en cours ({{ activeJobs.size }})</h3>
        <div class="jobs-list">
          <div v-for="[id, job] in activeJobs" :key="id" class="job-card">
            <div class="job-info">
              <span class="file-name">{{ job.fileName }}</span>
              <span class="status-badge" :class="job.status.toLowerCase()">
                {{ job.status === JobStatus.Processing ? 'Traitement...' : 'En attente' }}
              </span>
            </div>
            <div class="progress-bar-small">
              <div class="fill indeterminate"></div>
            </div>
          </div>
        </div>
    </section>
  </div>
</template>

<style scoped>
.documents-view {
  max-width: 1200px;
  margin: 0 auto;
}

.section-header h1 {
  font-size: 2.2rem;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.section-header p {
  color: #64748b;
  margin-bottom: 2.5rem;
}

.grid-container {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}

/* Jobs tracking */
.active-jobs h3 {
  margin-bottom: 1rem;
  color: #1e293b;
  font-size: 1.1rem;
}

.jobs-list {
  display: grid;
  gap: 1rem;
}

.job-card {
  background: white;
  padding: 1.2rem;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0,0,0,0.02);
  border-left: 4px solid #3b82f6;
}

.job-info {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.8rem;
}

.status-badge {
  font-size: 0.75rem;
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
  font-weight: 600;
}

.status-badge.pending { background: #e2e8f0; color: #475569; }
.status-badge.processing { background: #dbeafe; color: #1e40af; }

.progress-bar-small {
  height: 6px;
  background: #f1f5f9;
  border-radius: 3px;
  overflow: hidden;
}

.fill.indeterminate {
  width: 30%;
  height: 100%;
  background: #3b82f6;
  animation: indeterminate 1.5s infinite linear;
}

@keyframes indeterminate {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(400%); }
}

/* Recent results display */
.recent-results {
  margin-top: 2rem;
  background: white;
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.results-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  background: #f8fafc;
}

.results-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
}

.results-header .count {
  font-size: 0.8rem;
  background: #3b82f6;
  color: white;
  padding: 0.2rem 0.6rem;
  border-radius: 20px;
  font-weight: 600;
}

.results-container {
  max-height: 400px;
  overflow-y: auto;
}

.results-table {
  width: 100%;
  border-collapse: collapse;
}

.results-table th {
  text-align: left;
  padding: 0.75rem 1.5rem;
  font-size: 0.75rem;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.results-table td {
  padding: 1rem 1.5rem;
  font-size: 0.9rem;
  border-bottom: 1px solid #f8fafc;
}

.result-row:hover td {
  background: #f0f9ff;
}

.vendor-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.vendor-cell i {
  color: #3b82f6;
}

.text-right {
  text-align: right;
}

.amount {
  font-weight: 700;
  color: #10b981;
}

.btn-goto {
  display: inline-block;
  padding: 0.4rem 0.8rem;
  background: #1e293b;
  color: white;
  text-decoration: none;
  font-size: 0.8rem;
  font-weight: 600;
  border-radius: 6px;
  transition: all 0.2s;
}

.btn-goto:hover {
  background: #3b82f6;
  transform: translateY(-1px);
}

.no-data {
  padding: 3rem;
  text-align: center;
  background: #f8fafc;
  border-radius: 12px;
  color: #94a3b8;
  border: 2px dashed #e2e8f0;
}

.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #f1f1f1; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e0; border-radius: 10px; }

.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
