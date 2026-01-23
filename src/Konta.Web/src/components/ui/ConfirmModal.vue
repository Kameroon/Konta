<script setup lang="ts">
import BaseModal from './BaseModal.vue';
import { computed } from 'vue';

const props = defineProps<{
  show: boolean;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'danger' | 'warning' | 'info';
  loading?: boolean;
}>();

const emit = defineEmits(['close', 'confirm']);

const variantClass = computed(() => {
  return props.type || 'info';
});

const iconClass = computed(() => {
  switch (props.type) {
    case 'danger': return 'fas fa-exclamation-circle';
    case 'warning': return 'fas fa-exclamation-triangle';
    default: return 'fas fa-question-circle';
  }
});
</script>

<template>
  <BaseModal :show="show" :title="title" @close="emit('close')" maxWidth="420px">
    <div class="confirm-content" :class="variantClass">
      <div class="icon-section">
        <i :class="iconClass"></i>
      </div>
      
      <div class="text-section">
        <p class="message">{{ message }}</p>
      </div>
    </div>

    <template #footer>
      <button class="btn-cancel" @click="emit('close')" :disabled="loading">
        {{ cancelText || 'Annuler' }}
      </button>
      <button class="btn-confirm" :class="variantClass" @click="emit('confirm')" :disabled="loading">
        <span v-if="loading" class="spinner"></span>
        <span v-else>{{ confirmText || 'Confirmer' }}</span>
      </button>
    </template>
  </BaseModal>
</template>

<style scoped>
.confirm-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 1rem 0;
}

.icon-section {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  margin-bottom: 1.5rem;
}

/* Variants */
.confirm-content.danger .icon-section { background: #fee2e2; color: #ef4444; }
.confirm-content.warning .icon-section { background: #fef3c7; color: #f59e0b; }
.confirm-content.info .icon-section { background: #eff6ff; color: #3b82f6; }

.message {
  color: #4b5563;
  line-height: 1.6;
  font-size: 1.05rem;
  margin: 0;
}

/* Buttons */
.btn-cancel {
  background: white;
  border: 1px solid #e2e8f0;
  color: #64748b;
  padding: 0.7rem 1.2rem;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}

.btn-confirm {
  border: none;
  color: white;
  padding: 0.7rem 1.8rem;
  border-radius: 10px;
  font-weight: 700;
  cursor: pointer;
  min-width: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-confirm.danger { background: #ef4444; }
.btn-confirm.danger:hover:not(:disabled) { background: #dc2626; }

.btn-confirm.warning { background: #f59e0b; }
.btn-confirm.warning:hover:not(:disabled) { background: #d97706; }

.btn-confirm.info { background: #1e293b; }
.btn-confirm.info:hover:not(:disabled) { background: #0f172a; }

.btn-confirm:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }
</style>
