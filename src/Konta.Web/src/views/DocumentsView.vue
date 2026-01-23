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
          results.value.unshift(result);
          toast.success(`Facture extraite avec succès.`);
        } else {
          toast.warning(`Aucune donnée structurée n'a pu être extraite de ${job.fileName}.`);
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
      <h1>Documents & OCR</h1>
      <p>Numérisez vos factures et automatisez votre comptabilité avec l'intelligence artificielle.</p>
    </header>

    <div class="grid-container">
      <!-- Zone d'Upload -->
      <section class="upload-section">
        <DocumentUpload @upload="handleUpload" />
        <LoadingOverlay :active="uploading" message="Téléchargement du fichier..." />
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

      <!-- Résultats d'extractions -->
      <section class="results-section">
        <h3>Dernières extractions</h3>
        
        <div v-if="results.length === 0" class="no-data">
          Aucun document extrait pour le moment.
        </div>
        
        <div v-else class="results-list">
          <div v-for="invoice in results" :key="invoice.id" class="invoice-result-card">
            <div class="vendor">{{ invoice.vendorName || 'Vendeur inconnu' }}</div>
            <div class="details">
              <div class="detail-item">
                <label>N° Facture</label>
                <span>{{ invoice.invoiceNumber || 'N/A' }}</span>
              </div>
              <div class="detail-item">
                <label>Date</label>
                <span>{{ invoice.invoiceDate ? new Date(invoice.invoiceDate).toLocaleDateString() : 'N/A' }}</span>
              </div>
              <div class="detail-item amount">
                <label>Total TTC</label>
                <span>{{ formatCurrency(invoice.totalAmountTtc, invoice.currency) }}</span>
              </div>
            </div>
            <div class="card-footer">
              <span class="tag">Facture PDF</span>
              <button class="view-btn">Générer Écriture</button>
            </div>
          </div>
        </div>
      </section>
    </div>
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

/* Results section */
.results-section h3 {
  margin-bottom: 1.5rem;
  color: #1e293b;
}

.results-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.invoice-result-card {
  background: white;
  padding: 1.5rem;
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  transition: box-shadow 0.2s;
}

.invoice-result-card:hover {
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
}

.vendor {
  font-weight: 700;
  font-size: 1.1rem;
  color: #1e293b;
  margin-bottom: 1rem;
}

.details {
  display: flex;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.detail-item label {
  display: block;
  font-size: 0.75rem;
  color: #64748b;
  margin-bottom: 0.2rem;
}

.detail-item span {
  font-weight: 600;
  color: #334155;
}

.detail-item.amount span {
  color: #42b883;
  font-size: 1.1rem;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #f1f5f9;
}

.tag {
  background: #f1f5f9;
  color: #475569;
  font-size: 0.75rem;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}

.view-btn {
  background: #1e293b;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.85rem;
}

.no-data {
  padding: 3rem;
  text-align: center;
  background: #f8fafc;
  border-radius: 12px;
  color: #94a3b8;
  border: 2px dashed #e2e8f0;
}
</style>
