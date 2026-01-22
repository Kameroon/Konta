<script setup lang="ts">
/**
 * AuthLayout : Layout minimaliste pour les écrans de connexion.
 * Centre le contenu et fournit un fond épuré.
 */
</script>

<template>
  <div class="auth-layout">
    <div class="auth-card-container">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </div>
  </div>
</template>

<style scoped>
.auth-layout {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  padding: 1.5rem;
}

.auth-card-container {
  width: 100%;
  max-width: 450px;
}

/* Animations de transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
