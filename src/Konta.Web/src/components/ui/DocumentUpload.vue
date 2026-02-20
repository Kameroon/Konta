<script setup lang="ts">
import { ref } from 'vue';

/**
 * DocumentUpload : Zone d'upload interactive pour les documents PDF.
 */
const emit = defineEmits<{
  (e: 'upload', file: File): void
}>();

const isDragging = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);

const onDragOver = (e: DragEvent) => {
  e.preventDefault();
  isDragging.value = true;
};

const onDragLeave = () => {
  isDragging.value = false;
};

const onDrop = (e: DragEvent) => {
  e.preventDefault();
  isDragging.value = false;
  const files = e.dataTransfer?.files;
  if (files && files.length > 0) {
    handleFile(files[0]);
  }
};

const onFileChange = (e: Event) => {
  const target = e.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    handleFile(target.files[0]);
  }
};

const handleFile = (file: File) => {
  if (file.type !== 'application/pdf') {
    alert('Seuls les fichiers PDF sont acceptés.');
    return;
  }
  emit('upload', file);
};

const triggerFileInput = () => {
  fileInput.value?.click();
};
</script>

<template>
  <div 
    class="upload-zone"
    :class="{ 'is-dragging': isDragging }"
    @dragover="onDragOver"
    @dragleave="onDragLeave"
    @drop="onDrop"
    @click="triggerFileInput"
  >
    <input 
      type="file" 
      ref="fileInput" 
      class="hidden" 
      accept="application/pdf" 
      @change="onFileChange" 
    />
    
    <div class="upload-content">
      <div class="icon-wrapper">
        <svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="16" y1="13" x2="8" y2="13"></line>
          <line x1="16" y1="17" x2="8" y2="17"></line>
          <polyline points="10 9 9 9 8 9"></polyline>
        </svg>
      </div>
      <h3>Déposez votre facture ici</h3>
      <p>ou cliquez pour choisir un fichier PDF</p>
      <div class="file-info">Maximum 10 Mo • Format PDF uniquement</div>
    </div>
  </div>
</template>

<style scoped>
.upload-zone {
  border: 1px dashed #cbd5e1;
  border-radius: 24px;
  padding: 5rem 3rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  background: white;
  max-width: 600px;
  margin: 0 auto;
}

.upload-zone:hover {
  border-color: #7c3aed;
  background-color: #f8fafc;
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05);
}

.upload-zone.is-dragging {
  border-color: #7c3aed;
  background-color: #f5f3ff;
  transform: scale(1.01);
}

.hidden {
  display: none;
}

.icon-wrapper {
  margin-bottom: 2rem;
  display: flex;
  justify-content: center;
}

.file-icon {
  width: 64px;
  height: 64px;
  color: #c4b5fd; /* Soft purple from POC */
  background: #f5f3ff;
  padding: 12px;
  border-radius: 12px;
}

.upload-content h3 {
  font-size: 1.4rem;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 0.75rem;
}

.upload-content p {
  color: #64748b;
  margin-bottom: 1.5rem;
  font-size: 1.1rem;
}

.file-info {
  font-size: 0.85rem;
  color: #94a3b8;
  background: #f1f5f9;
  display: inline-block;
  padding: 0.5rem 1.25rem;
  border-radius: 99px;
  font-weight: 500;
}
</style>
