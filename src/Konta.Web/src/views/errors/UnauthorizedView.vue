<script setup lang="ts">
/**
 * UnauthorizedView : Affiché lors d'une erreur 403 (RBAC).
 */
</script>

<template>
  <div class="unauthorized-view">
    <div class="error-container">
      <div class="icon">🚫</div>
      <h1>Accès Refusé</h1>
      <p>Vous n'avez pas les permissions nécessaires pour accéder à cette page.</p>
      <router-link to="/" class="back-btn">Retour à l'accueil</router-link>
    </div>
  </div>
</template>

<style scoped>
.unauthorized-view {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  background-color: #f8fafc;
}

.error-container .icon {
  font-size: 5rem;
  margin-bottom: 1rem;
}

h1 {
  font-size: 2.5rem;
  color: #1e293b;
}

p {
  color: #64748b;
  margin-bottom: 2rem;
}

.back-btn {
  display: inline-block;
  padding: 0.8rem 1.5rem;
  background-color: #1e293b;
  color: white;
  text-decoration: none;
  border-radius: 8px;
  transition: opacity 0.2s;
}

.back-btn:hover {
  opacity: 0.9;
}
</style>
