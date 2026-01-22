<script setup lang="ts">
/**
 * ErrorState : Composant affiché en cas d'échec de récupération des données.
 */
defineProps<{
  error: string | null;
  retry?: () => void;
}>();
</script>

<template>
  <div v-if="error" class="error-state">
    <div class="icon">⚠️</div>
    <h3>Oups ! Une erreur est survenue</h3>
    <p>{{ error }}</p>
    <button v-if="retry" @click="retry" class="retry-btn">
      🔄 Réessayer
    </button>
  </div>
</template>

<style scoped>
.error-state {
  padding: 3rem;
  text-align: center;
  background: #fff5f5;
  border-radius: 12px;
  border: 1px solid #fed7d7;
  color: #c53030;
}

.icon {
  font-size: 2.5rem;
  margin-bottom: 1rem;
}

h3 {
  margin-bottom: 0.5rem;
  font-weight: 700;
}

p {
  margin-bottom: 1.5rem;
  color: #742a2a;
}

.retry-btn {
  padding: 0.6rem 1.2rem;
  background: #c53030;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
}

.retry-btn:hover {
  opacity: 0.9;
}
</style>
