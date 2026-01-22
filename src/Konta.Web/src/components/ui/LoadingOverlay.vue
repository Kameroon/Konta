<script setup lang="ts">
/**
 * LoadingOverlay : Composant de chargement avec spinner et message.
 */
defineProps<{
  active: boolean;
  message?: string;
}>();
</script>

<template>
  <transition name="fade">
    <div v-if="active" class="loading-overlay">
      <div class="loader-content">
        <div class="spinner"></div>
        <p v-if="message">{{ message }}</p>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(2px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
  border-radius: 12px;
}

.loader-content {
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #42b883;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

p {
  color: #1e293b;
  font-weight: 500;
  font-size: 0.9rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
