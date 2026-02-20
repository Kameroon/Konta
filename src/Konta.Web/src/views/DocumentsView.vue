<script setup lang="ts">
import { ref, computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { documentsApi } from '@/api/documents.api';
import { JobStatus, type ExtractionJob, type ExtractedInvoice } from '@/types/document.types';
import { useToast } from 'vue-toastification';
import DocumentUpload from '@/components/ui/DocumentUpload.vue';
import LoadingOverlay from '@/components/ui/LoadingOverlay.vue';

const authStore = useAuthStore();
const toast = useToast();

const uploading = ref(false);
const activeJobId = ref<string | null>(null);
const result = ref<ExtractedInvoice | null>(null);
const pdfUrl = ref<string | null>(null);
const currentFile = ref<File | null>(null);

const handleUpload = async (file: File) => {
  if (!authStore.user?.tenantId) return;

  uploading.value = true;
  currentFile.value = file;
  
  // Créer une URL locale pour la prévisualisation immédiate
  pdfUrl.value = URL.createObjectURL(file);

  try {
    const { jobId } = await documentsApi.uploadDocument(file, authStore.user.tenantId);
    activeJobId.value = jobId;
    trackJob(jobId);
  } catch (err) {
    toast.error("Échec de l'envoi du document.");
    resetView();
  } finally {
    uploading.value = false;
  }
};

const trackJob = async (jobId: string) => {
  const poll = async () => {
    try {
      const job = await documentsApi.getJobStatus(jobId);
      
      if (job.status === JobStatus.Completed) {
        const data = await documentsApi.getInvoiceResult(jobId);
        result.value = data;
        toast.success(`Analyse terminée avec succès.`);
      } else if (job.status === JobStatus.Failed) {
        toast.error(`L'analyse a échoué.`);
        resetView();
      } else {
        setTimeout(poll, 1500);
      }
    } catch (err) {
      console.error('Erreur polling:', err);
      resetView();
    }
  };
  poll();
};

const resetView = () => {
  activeJobId.value = null;
  result.value = null;
  pdfUrl.value = null;
  currentFile.value = null;
};

const formatCurrency = (val: number | undefined) => {
  if (val === undefined) return '0,00';
  return new Intl.NumberFormat('fr-FR', { minimumFractionDigits: 2 }).format(val);
};

const promoteInvoice = async () => {
  toast.info("Promotion de la facture en cours...");
  // TODO: Appel vers l'API de promotion (FinanceCore)
  setTimeout(() => {
    toast.success("Facture promue avec succès !");
    resetView();
  }, 1000);
};

</script>

<template>
  <div class="documents-view" :class="{ 'as-result': result || activeJobId }">
    
    <!-- Mode Upload Initial -->
    <div v-if="!activeJobId && !result" class="upload-mode">
      <header class="section-header">
        <h1>Téléchargement & OCR</h1>
        <p>Envoyez vos factures pour une analyse automatique par l'intelligence artificielle.</p>
      </header>

      <div class="upload-container">
        <DocumentUpload @upload="handleUpload" />
        <LoadingOverlay :active="uploading" message="Téléchargement du fichier..." />
      </div>
    </div>

    <!-- Mode Analyse / Résultats (Side by Side) -->
    <div v-else class="result-mode fade-in">
      <div class="result-layout-header">
        <div class="header-left">
          <h1>Validation Side-by-Side <span v-if="result" class="job-id">(ID: {{ result.id.substring(0, 8) }})</span></h1>
        </div>
        <div class="header-right">
          <button @click="resetView" class="btn-cancel-top">
            <i class="fas fa-times"></i> Annuler
          </button>
          <button @click="resetView" class="btn-reject-top">
            <i class="fas fa-undo"></i> Rejeter
          </button>
          <button @click="promoteInvoice" class="btn-confirm-top" :disabled="!result">
            <i class="fas fa-check"></i> Confirmer & Valider
          </button>
        </div>
      </div>

      <div class="result-container">
        <!-- Zone Gauche : Prévisualisation PDF -->
        <div class="pdf-preview">
          <iframe v-if="pdfUrl" :src="pdfUrl" width="100%" height="100%" frameborder="0"></iframe>
          <div v-else class="no-preview">
            <i class="fas fa-file-pdf"></i>
            <p>Chargement du PDF...</p>
          </div>
        </div>

        <!-- Zone Droite : Formulaire de données extraites -->
        <div class="extraction-form custom-scrollbar">
          <div class="form-header-inner">
            <span class="form-title">Données Extraites</span>
            <div class="score-badge" v-if="result">
              Score: {{ result.confidenceScore || 0 }}% 
              <i class="fas fa-check-circle" v-if="(result.confidenceScore || 0) > 80"></i>
            </div>
          </div>

          <div v-if="!result" class="processing-placeholder">
            <div class="spinner"></div>
            <p>Analyse par l'IA en cours...</p>
          </div>

        <div v-else class="fields-container">
          <div class="form-group full">
            <label>Numéro de Facture</label>
            <input v-model="result.invoiceNumber" type="text" placeholder="Non détecté" />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>SIREN Émetteur</label>
              <input v-model="result.vendorSiret" type="text" placeholder="Non détecté" />
            </div>
            <div class="form-group">
              <label>SIREN Destinataire</label>
              <input v-model="result.customerSiret" type="text" placeholder="Non détecté" />
            </div>
          </div>

          <div class="form-group full">
            <label>Numéro de TVA</label>
            <input v-model="result.vendorVatNumber" type="text" placeholder="Non détecté" />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Date d'Émission</label>
              <div class="input-with-icon">
                <input v-model="result.invoiceDate" type="date" />
                <i class="far fa-calendar-alt"></i>
              </div>
            </div>
            <div class="form-group">
              <label>Date d'Échéance</label>
              <div class="input-with-icon">
                <input v-model="result.dueDate" type="date" />
                <i class="far fa-calendar-alt"></i>
              </div>
            </div>
          </div>

          <!-- Bloc financier épuré -->
          <div class="financial-summary">
            <div class="summary-row">
              <span>Montant HT (€)</span>
              <input v-model.number="result.totalAmountHt" type="number" step="0.01" />
            </div>
            <div class="summary-row">
              <span>Montant TVA (€)</span>
              <input v-model.number="result.vatAmount" type="number" step="0.01" />
            </div>
            <div class="summary-row total">
              <span>Total TTC (€)</span>
              <input v-model.number="result.totalAmountTtc" type="number" step="0.01" class="bold" />
            </div>
          </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.documents-view {
  width: 100%;
  max-width: 1280px;
  margin: 0 auto;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  padding: 0 1rem;
}

.documents-view.as-result {
  max-width: 1600px;
  padding: 0;
}

.upload-mode {
  text-align: center;
  padding: 4rem 1rem;
  animation: slideUp 0.5s ease-out;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.section-header h1 {
  font-size: 2.5rem;
  font-weight: 850;
  color: #0f172a;
  letter-spacing: -0.02em;
  margin-bottom: 1rem;
}

.section-header p {
  color: #64748b;
  margin-bottom: 4rem;
  font-size: 1.2rem;
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
}

.upload-container {
  display: flex;
  justify-content: center;
  position: relative;
}

/* Result Mode Layout - Standardized Side-by-Side */
.result-mode {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 120px);
  background: white;
  margin: 0 1rem;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 15px 35px rgba(0,0,0,0.05);
  border: 1px solid #eef2f6;
}

.result-layout-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 2rem;
  background: white;
  border-bottom: 1px solid #f1f5f9;
}

.header-left h1 {
  font-size: 1.4rem;
  font-weight: 850;
  color: #0f172a;
  margin: 0;
}

.job-id {
  font-size: 0.9rem;
  color: #94a3b8;
  font-weight: 500;
  margin-left: 0.5rem;
}

.header-right {
  display: flex;
  gap: 0.75rem;
}

.btn-cancel-top, .btn-reject-top, .btn-confirm-top {
  padding: 0.6rem 1.25rem;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s;
}

.btn-cancel-top {
  background: transparent;
  border: none;
  color: #64748b;
}

.btn-reject-top {
  background: white;
  border: 1.5px solid #fee2e2;
  color: #ef4444;
}

.btn-reject-top:hover { background: #fef2f2; }

.btn-confirm-top {
  background: #10b981;
  border: none;
  color: white;
}

.btn-confirm-top:hover {
  background: #059669;
  transform: translateY(-1px);
}

.btn-confirm-top:disabled {
  background: #cbd5e1;
  transform: none;
}

.result-container {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.pdf-preview {
  flex: 1.2;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pdf-preview iframe {
  width: 100%;
  height: 100%;
}

.no-preview {
  color: #94a3b8;
  text-align: center;
}

.no-preview i { font-size: 3rem; margin-bottom: 1rem; }

.extraction-form {
  flex: 0.8;
  background: white;
  padding: 2rem;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  border-left: 1px solid #f1f5f9;
}

.form-header-inner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #f8fafc;
}

.form-title {
  font-size: 0.9rem;
  font-weight: 800;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.score-badge {
  font-size: 0.85rem;
  font-weight: 700;
  color: #64748b;
  display: flex;
  align-items: center;
  gap: 6px;
}

.text-success { color: #10b981; }

/* Processing / Loading State Styling */
.processing-placeholder {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #64748b;
  gap: 2rem;
  background: radial-gradient(circle at center, #ffffff 0%, #fcfcfd 100%);
}

.spinner {
  width: 64px;
  height: 64px;
  border: 5px solid #f1f5f9;
  border-top: 5px solid #7c3aed;
  border-radius: 50%;
  animation: spin 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
  box-shadow: 0 4px 6px -1px rgba(124, 58, 237, 0.1);
}

@keyframes spin { to { transform: rotate(360deg); } }

.processing-placeholder p {
  font-size: 1.1rem;
  font-weight: 600;
  letter-spacing: -0.01em;
}

/* Form Elements - Polished Inputs */
.fields-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  animation: fadeIn 0.6s ease-out;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.form-group.full { grid-column: span 2; }

.form-group label {
  font-size: 0.8rem;
  font-weight: 750;
  color: #475569;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.form-group input {
  padding: 0.85rem 1.1rem;
  border: 1.5px solid #f1f5f9;
  border-radius: 12px;
  background: #fcfcfd;
  color: #1e293b;
  font-weight: 600;
  font-size: 0.95rem;
  transition: all 0.2s ease;
}

.form-group input:hover {
  border-color: #e2e8f0;
  background: white;
}

.form-group input:focus {
  border-color: #7c3aed;
  background: white;
  box-shadow: 0 0 0 4px rgba(124, 58, 237, 0.08);
  outline: none;
}

.input-with-icon {
  position: relative;
}

.input-with-icon input { width: 100%; padding-right: 2.5rem; }

.input-with-icon i {
  position: absolute;
  right: 1.1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  pointer-events: none;
  font-size: 1rem;
}

/* Financial Summary Block - Very Konta Style */
.financial-summary {
  background: #f8fafc;
  padding: 2rem;
  border-radius: 20px;
  border: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 1.5rem;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #64748b;
  font-weight: 600;
}

.summary-row input {
  width: 140px;
  padding: 0.5rem 1rem;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  text-align: right;
  font-weight: 800;
  color: #0f172a;
  background: white !important;
  font-size: 1rem;
}

.summary-row.total {
  margin-top: 0.5rem;
  padding-top: 1.5rem;
  border-top: 2px dashed #e2e8f0;
  color: #0f172a;
  font-weight: 900;
  font-size: 1.1rem;
}

.summary-row.total input {
  background: #0f172a !important;
  border-color: #0f172a;
  color: white !important;
  font-size: 1.25rem;
  width: 160px;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.15);
}

/* Actions Section */
.actions {
  display: flex;
  gap: 1rem;
  margin-top: 2.5rem;
  padding-bottom: 1rem;
}

.btn-cancel {
  flex: 1;
  padding: 1rem;
  border: 1.5px solid #e2e8f0;
  background: white;
  border-radius: 14px;
  font-weight: 700;
  color: #64748b;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  transition: all 0.2s;
}

.btn-cancel:hover {
  background: #f8fafc;
  color: #1e293b;
  border-color: #cbd5e1;
}

.btn-promote {
  flex: 2;
  padding: 1rem;
  background: #0f172a;
  color: white;
  border: none;
  border-radius: 14px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);
}

.btn-promote:hover { 
  background: #1e293b; 
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
}

.btn-promote:active { transform: translateY(0); }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

/* Scrollbar Refinement */
.custom-scrollbar::-webkit-scrollbar { width: 8px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { 
  background: #e2e8f0; 
  border-radius: 10px; 
  border: 2px solid white;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }

/* Global Container adjustment for results mode */
@media screen and (min-height: 900px) {
  .result-mode { height: calc(100vh - 180px); }
}
</style>
