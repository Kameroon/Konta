<script setup lang="ts">
import BaseModal from './BaseModal.vue';

const props = defineProps<{
  show: boolean;
  targetPlan: string;
  currentPlan?: string;
  loading?: boolean;
}>();

const emit = defineEmits(['close', 'confirm']);
</script>

<template>
  <BaseModal :show="show" title="Confirmation de changement de plan" @close="emit('close')" maxWidth="450px">
    <div class="upgrade-confirm">
      <div class="upgrade-visual">
        <div class="plan-pill current">{{ currentPlan || 'Free' }}</div>
        <div class="arrow">
          <i class="fas fa-arrow-right"></i>
        </div>
        <div class="plan-pill next">{{ targetPlan }}</div>
      </div>

      <div class="message">
        <h4>Prêt pour le changement ?</h4>
        <p>Vous êtes sur le point de passer à l'offre <strong>{{ targetPlan }}</strong>. Vos nouveaux quotas et fonctionnalités seront activés immédiatement.</p>
      </div>

      <div class="alert-info">
        <i class="fas fa-info-circle"></i>
        <span>Le montant sera ajusté lors de votre prochaine facturation mensuelle.</span>
      </div>
    </div>

    <template #footer>
      <button class="btn-cancel" @click="emit('close')" :disabled="loading">Annuler</button>
      <button class="btn-confirm" @click="emit('confirm')" :disabled="loading">
        <span v-if="loading" class="spinner"></span>
        <span v-else>Confirmer l'Upgrade</span>
      </button>
    </template>
  </BaseModal>
</template>

<style scoped>
.upgrade-confirm {
  text-align: center;
}

.upgrade-visual {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.plan-pill {
  padding: 0.6rem 1.2rem;
  border-radius: 99px;
  font-weight: 800;
  font-size: 0.9rem;
  text-transform: uppercase;
}

.plan-pill.current {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.plan-pill.next {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  box-shadow: 0 10px 15px -3px rgba(37, 99, 235, 0.2);
}

.arrow {
  color: #cbd5e0;
  font-size: 1.2rem;
}

.message h4 {
  font-size: 1.25rem;
  color: #1e293b;
  margin-bottom: 0.75rem;
  font-weight: 800;
}

.message p {
  color: #64748b;
  line-height: 1.5;
  font-size: 0.95rem;
}

.alert-info {
  margin-top: 2rem;
  background: #f0f9ff;
  border: 1px solid #e0f2fe;
  padding: 1rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 12px;
  text-align: left;
}

.alert-info i {
  color: #0ea5e9;
  font-size: 1.1rem;
}

.alert-info span {
  font-size: 0.8rem;
  color: #0369a1;
  font-weight: 500;
}

/* Footer Buttons */
.btn-cancel {
  background: transparent;
  border: 1px solid #e2e8f0;
  color: #64748b;
  padding: 0.75rem 1.5rem;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-cancel:hover {
  background: #f8fafc;
  border-color: #cbd5e0;
}

.btn-confirm {
  background: #1e293b;
  color: white;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 10px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 160px;
}

.btn-confirm:hover:not(:disabled) {
  background: #0f172a;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.btn-confirm:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
