<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { documentsApi } from '@/api/documents.api';
import { JobStatus, type ExtractionJob } from '@/types/document.types';
import { useToast } from 'vue-toastification';
import ConfirmModal from '@/components/ui/ConfirmModal.vue';

/**
 * ArchiveView : Historique des documents envoyés par l'utilisateur.
 */

const authStore = useAuthStore();
const toast = useToast();
const jobs = ref<ExtractionJob[]>([]);
const loading = ref(true);

// Deletion UI
const showDeleteConfirm = ref(false);
const jobIdToDelete = ref<string | null>(null);
const deleting = ref(false);

const isSuperAdmin = computed(() => authStore.user?.role === 'SuperAdmin');

onMounted(async () => {
  await fetchJobs();
});

const fetchJobs = async () => {
  loading.value = true;
  try {
    if (isSuperAdmin.value) {
      jobs.value = await documentsApi.getJobs();
    } else {
      jobs.value = await documentsApi.getMyJobs();
    }
  } catch (err) {
    toast.error("Échec de la récupération de l'historique.");
  } finally {
    loading.value = false;
  }
};

const deleteJob = (id: string) => {
  jobIdToDelete.value = id;
  showDeleteConfirm.value = true;
};

const confirmDelete = async () => {
  if (!jobIdToDelete.value) return;
  
  try {
    deleting.value = true;
    await documentsApi.deleteJob(jobIdToDelete.value);
    jobs.value = jobs.value.filter(j => j.id !== jobIdToDelete.value);
    toast.success("Document supprimé.");
    showDeleteConfirm.value = false;
  } catch (err) {
    toast.error("Erreur lors de la suppression.");
  } finally {
    deleting.value = false;
    jobIdToDelete.value = null;
  }
};

const downloadDoc = async (job: ExtractionJob) => {
  try {
    await documentsApi.downloadDocument(job.id, job.fileName);
  } catch (err) {
    toast.error("Erreur lors du téléchargement.");
  }
};

const getStatusLabel = (status: JobStatus) => {
  switch (status) {
    case JobStatus.Completed: return "Terminé";
    case JobStatus.Processing: return "En cours";
    case JobStatus.Failed: return "Échoué";
    default: return "En attente";
  }
};

const getStatusClass = (status: JobStatus) => {
  return `status-${status.toLowerCase()}`;
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('fr-FR', {
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
  });
};
</script>

<template>
  <div class="archive-view">
    <header class="section-header">
      <div class="header-content">
        <h1>{{ isSuperAdmin ? 'Archive Globale' : 'Mes Documents' }}</h1>
        <p>{{ isSuperAdmin ? 'Retrouvez ici tous les fichiers transmis sur la plateforme.' : 'Retrouvez ici tous les fichiers que vous avez transmis pour analyse.' }}</p>
      </div>
      <button class="btn-refresh" @click="fetchJobs" :disabled="loading">
        <i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i> Actualiser
      </button>
    </header>

    <div class="content-card">
      <div v-if="loading && jobs.length === 0" class="loading-state">
        <i class="fas fa-spinner fa-spin"></i> Chargement de votre archive...
      </div>

      <div v-else-if="jobs.length === 0" class="empty-state">
        <div class="empty-icon">
          <i class="fas fa-file-invoice"></i>
        </div>
        <h3>Aucun document trouvé</h3>
        <p>Vous n'avez pas encore téléchargé de documents. Rendez-vous dans la section "Téléchargement" pour commencer.</p>
        <router-link to="/app/download" class="btn-primary">Télécharger un fichier</router-link>
      </div>

      <div v-else class="table-container">
        <table class="konta-table">
          <thead>
            <tr>
              <th>Nom du fichier</th>
              <th>Date d'envoi</th>
              <th>Type détecté</th>
              <th>Statut</th>
              <th class="text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="job in jobs" :key="job.id">
              <td>
                <div class="file-cell">
                  <i class="fas fa-file-pdf pdf-icon"></i>
                  <span class="filename">{{ job.fileName }}</span>
                </div>
              </td>
              <td class="date-cell">{{ formatDate(job.createdAt) }}</td>
              <td>
                <span class="type-badge">{{ job.detectedType }}</span>
              </td>
              <td>
                <span class="status-pill" :class="getStatusClass(job.status)">
                  {{ getStatusLabel(job.status) }}
                </span>
              </td>
              <td class="text-right actions">
                <button class="action-btn" @click="downloadDoc(job)" title="Télécharger le PDF">
                  <i class="fas fa-download"></i>
                </button>
                <button class="action-btn delete" @click="deleteJob(job.id)" title="Supprimer">
                  <i class="fas fa-trash-alt"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Confirmation de suppression moderne -->
    <ConfirmModal
      :show="showDeleteConfirm"
      title="Supprimer le document"
      message="Voulez-vous supprimer ce document de votre historique ? Cette action est irréversible."
      confirmText="Supprimer"
      type="danger"
      :loading="deleting"
      @close="showDeleteConfirm = false"
      @confirm="confirmDelete"
    />
  </div>
</template>

<style scoped>
.archive-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-header h1 {
  font-size: 1.8rem;
  font-weight: 800;
  color: #1a202c;
  margin: 0;
}

.section-header p {
  color: #718096;
  margin-top: 4px;
}

.btn-refresh {
  background: white;
  border: 1px solid #e2e8f0;
  padding: 0.6rem 1.2rem;
  border-radius: 10px;
  font-weight: 700;
  color: #4a5568;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s;
}

.btn-refresh:hover {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.content-card {
  background: white;
  border-radius: 20px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  min-height: 400px;
}

.table-container {
  overflow-x: auto;
}

.konta-table {
  width: 100%;
  border-collapse: collapse;
}

.konta-table th {
  text-align: left;
  padding: 1.25rem 1.5rem;
  font-size: 0.75rem;
  font-weight: 800;
  color: #4a5568;
  text-transform: uppercase;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

.konta-table td {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.file-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.pdf-icon {
  color: #e53e3e;
  font-size: 1.2rem;
}

.filename {
  font-weight: 600;
  color: #2d3748;
}

.date-cell {
  color: #718096;
  font-size: 0.9rem;
}

.type-badge {
  background: #ebf8ff;
  color: #2b6cb0;
  padding: 2px 8px;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 700;
}

.status-pill {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 800;
}

.status-completed { background: #c6f6d5; color: #22543d; }
.status-processing { background: #bee3f8; color: #2a4365; }
.status-failed { background: #fed7d7; color: #822727; }
.status-pending { background: #edf2f7; color: #4a5568; }

.action-btn {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #718096;
  cursor: pointer;
  margin-left: 8px;
  transition: all 0.2s;
}

.action-btn:hover {
  background: #ebf8ff;
  color: #3182ce;
  border-color: #3182ce;
}

.action-btn.delete:hover {
  background: #fff5f5;
  color: #e53e3e;
  border-color: #e53e3e;
}

.loading-state, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 5rem 2rem;
  color: #a0aec0;
}

.loading-state i {
  font-size: 2rem;
  margin-bottom: 1rem;
}

.empty-icon {
  width: 80px;
  height: 80px;
  background: #f7fafc;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.2rem;
  margin-bottom: 1.5rem;
  color: #cbd5e0;
}

.empty-state h3 {
  color: #2d3748;
  margin-bottom: 0.5rem;
}

.empty-state p {
  text-align: center;
  max-width: 400px;
  margin-bottom: 2rem;
}

.btn-primary {
  background: #1a202c;
  color: white;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  text-decoration: none;
  font-weight: 700;
  transition: all 0.2s;
}

.btn-primary:hover {
  background: #2d3748;
  transform: translateY(-1px);
}
</style>
