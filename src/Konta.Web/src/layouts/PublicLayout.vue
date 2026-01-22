<script setup lang="ts">
import Footer from '@/components/layout/Footer.vue';
import { useAuthStore } from '@/stores/auth.store';

const authStore = useAuthStore();
</script>

<template>
  <div class="public-layout">
    <header class="public-header">
      <div class="header-content">
        <router-link to="/plans" class="logo">
          <span class="logo-icon">K</span>
          <span class="logo-text">Konta<strong>ERP</strong></span>
        </router-link>

        <nav class="public-nav">
          <router-link to="/plans" class="nav-link">Tarifs</router-link>
          <a href="#" class="nav-link">Fonctionnalités</a>
          <a href="#" class="nav-link">Contact</a>
        </nav>

        <div class="auth-actions">
          <template v-if="!authStore.isAuthenticated">
            <router-link :to="{ name: 'Login' }" class="btn-login">Connexion</router-link>
            <router-link :to="{ name: 'Register' }" class="btn-register">Essai Gratuit</router-link>
          </template>
          <template v-else>
            <router-link to="/" class="btn-dashboard">Console</router-link>
          </template>
        </div>
      </div>
    </header>

    <main class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <Footer />
  </div>
</template>

<style scoped>
.public-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: white;
}

.public-header {
  height: 80px;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  align-items: center;
}

.header-content {
  max-width: 1400px;
  width: 100%;
  margin: 0 auto;
  padding: 0 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
}

.logo-icon {
  background: #3b82f6;
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.2rem;
}

.logo-text {
  font-size: 22px;
  color: #1e293b;
  letter-spacing: -0.5px;
}

.public-nav {
  display: flex;
  gap: 32px;
}

.nav-link {
  color: #64748b;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.95rem;
  transition: color 0.3s;
}

.nav-link:hover {
  color: #3b82f6;
}

.auth-actions {
  display: flex;
  gap: 16px;
  align-items: center;
}

.btn-login {
  color: #1e293b;
  text-decoration: none;
  font-weight: 600;
  padding: 10px 20px;
  border-radius: 12px;
  transition: background 0.3s;
}

.btn-login:hover {
  background: #f1f5f9;
}

.btn-register, .btn-dashboard {
  background: #3b82f6;
  color: white;
  text-decoration: none;
  font-weight: 700;
  padding: 12px 24px;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
  transition: transform 0.3s, box-shadow 0.3s;
}

.btn-register:hover, .btn-dashboard:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.4);
}

.main-content {
  flex: 1;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

@media (max-width: 768px) {
  .public-nav {
    display: none;
  }
  
  .header-content {
    padding: 0 20px;
  }
}
</style>
