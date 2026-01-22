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
      <div class="icon">📄</div>
      <h3>Déposez votre facture ici</h3>
      <p>ou cliquez pour choisir un fichier PDF</p>
      <div class="file-info">Maximum 10 Mo • Format PDF uniquement</div>
    </div>
  </div>
</template>

<style scoped>
.upload-zone {
  border: 2px dashed #e2e8f0;
  border-radius: 16px;
  padding: 3rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  background: white;
}

.upload-zone:hover {
  border-color: #42b883;
  background-color: #f0fdf4;
  transform: translateY(-2px);
}

.upload-zone.is-dragging {
  border-color: #42b883;
  background-color: #dcfce7;
  transform: scale(1.02);
}

.hidden {
  display: none;
}

.upload-content .icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.upload-content h3 {
  font-size: 1.2rem;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.upload-content p {
  color: #64748b;
  margin-bottom: 1rem;
}

.file-info {
  font-size: 0.8rem;
  color: #94a3b8;
  background: #f8fafc;
  display: inline-block;
  padding: 0.4rem 0.8rem;
  border-radius: 20px;
}
</style>
