<script setup lang="ts">
/**
 * App.vue : Point d'entrée racine.
 * Utilise désormais <router-view> pour charger les layouts et vues.
 */
</script>

<template>
  <!-- Le routeur chargera automatiquement le bon layout selon la route -->
  <router-view />
</template>

<style>
/* Styles globaux pour toute l'application */
body {
  margin: 0;
  padding: 0;
  overflow-x: hidden;
}

#app {
  width: 100%;
}
</style>
