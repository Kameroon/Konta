<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { useUiStore } from '@/stores/ui.store';
import { useTenantStore } from '@/stores/tenant.store';
import { useToast } from 'vue-toastification';

import Footer from '@/components/layout/Footer.vue';

/**
 * MainLayout : Structure principale de l'ERP après connexion.
 * Gère la navigation latérale et l'état SaaS.
 */

const authStore = useAuthStore();
const uiStore = useUiStore();
const tenantStore = useTenantStore();
const router = useRouter();
const toast = useToast();

/**
 * Initialisation du layout : Chargement des données SaaS.
 */
onMounted(async () => {
  if (authStore.user?.tenantId && !tenantStore.currentTenant) {
    console.log('[MainLayout] Initialisation des données du tenant...');
    await tenantStore.fetchTenantInfo(authStore.user.tenantId);
  }
});

/**
 * Déconnexion sécurisée de l'application.
 */
const handleLogout = () => {
  console.log('[MainLayout] Tentative de déconnexion...');
  authStore.logout();
  toast.success('Déconnexion réussie.');
  router.push({ name: 'Login' });
};
</script>

<template>
  <div class="main-layout" :class="{ 'sidebar-collapsed': !uiStore.isSidebarOpen }">
    <!-- Barre latérale (Sidebar) -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="logo">
          <span class="logo-icon">K</span>
          <span class="logo-text">Konta<strong>ERP</strong></span>
        </div>
        <button class="collapse-btn" @click="uiStore.toggleSidebar">
          {{ uiStore.isSidebarOpen ? '❮' : '❯' }}
        </button>
      </div>
      
      <nav class="nav-menu">
        <router-link to="/app/dashboard" class="nav-item" active-class="active">
          <i class="icon">📊</i> <span>Tableau de bord</span>
        </router-link>

        <router-link to="/app/documents" class="nav-item" active-class="active">
          <i class="icon">📄</i> <span>Documents & OCR</span>
        </router-link>
        
        <!-- Visible seulement pour les Admins -->
        <router-link v-if="authStore.user?.roles.includes('Admin')" to="/app/admin" class="nav-item" active-class="active">
          <i class="icon">⚙️</i> <span>Administration</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <router-link to="/app/profile" class="nav-item profile-link" active-class="active">
          <i class="icon">👤</i> <span>Mon Profil</span>
        </router-link>
        <button @click="handleLogout" class="logout-btn">
          <i class="icon">🚪</i> <span>Déconnexion</span>
        </button>
      </div>
    </aside>

    <!-- Zone principale -->
    <main class="content-wrapper">
      <header class="top-bar">
        <div class="top-bar-left">
          <span class="page-title">{{ $route.meta.title || 'Application' }}</span>
        </div>
        
        <div class="top-bar-right">
          <div class="tenant-info" v-if="tenantStore.currentTenant">
            <span class="tenant-badge">{{ tenantStore.currentTenant.plan }}</span>
            <span class="tenant-name">{{ tenantStore.currentTenant.name }}</span>
          </div>
          
          <div class="user-info">
            <span class="user-name">{{ authStore.fullName }}</span>
            <div class="avatar" :style="{ backgroundColor: '#42b883' }">
              {{ authStore.user?.firstName?.charAt(0) }}{{ authStore.user?.lastName?.charAt(0) }}
            </div>
          </div>
        </div>
      </header>

      <div class="layout-body">
        <section class="page-content">
          <router-view v-slot="{ Component }">
            <transition name="page-fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </section>

        <Footer />
      </div>
    </main>
  </div>
</template>

<style scoped>
.main-layout {
  display: flex;
  min-height: 100vh;
  background-color: #f8fafc;
}

/* Sidebar Styling */
.sidebar {
  width: 280px;
  background-color: #1e293b;
  color: white;
  display: flex;
  flex-direction: column;
  position: fixed;
  height: 100vh;
  z-index: 100;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  padding: 0;
}

.main-layout.sidebar-collapsed .sidebar {
  width: 80px;
}

.main-layout.sidebar-collapsed .sidebar .logo-text,
.main-layout.sidebar-collapsed .sidebar .nav-item span,
.main-layout.sidebar-collapsed .sidebar .profile-link span {
  display: none;
}

.sidebar-header {
  padding: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(255,255,255,0.1);
  height: 70px;
}

.collapse-btn {
  background: rgba(255,255,255,0.05);
  border: none;
  color: white;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
}

.collapse-btn:hover {
  background: rgba(255,255,255,0.1);
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.logo-icon {
  background-color: #42b883;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.1rem;
}

.logo-text {
  font-size: 1.2rem;
  letter-spacing: -0.5px;
}

.nav-menu {
  flex: 1;
  padding: 1.5rem 0.8rem;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 0.8rem 1rem;
  color: #94a3b8;
  text-decoration: none;
  border-radius: 8px;
  margin-bottom: 0.5rem;
  transition: all 0.2s;
  font-weight: 500;
}

.nav-item:hover, .nav-item.active {
  background-color: rgba(255,255,255,0.05);
  color: white;
}

.nav-item.active {
  background-color: #42b883;
  color: white;
}

.icon {
  width: 20px;
  display: flex;
  justify-content: center;
  margin-right: 1rem;
  font-style: normal;
}

.main-layout.sidebar-collapsed .icon {
  margin-right: 0;
}

.sidebar-footer {
  padding: 1rem 0.8rem;
  border-top: 1px solid rgba(255,255,255,0.1);
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* Top Bar Styling */
.content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  margin-left: 280px;
  transition: margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.main-layout.sidebar-collapsed .content-wrapper {
  margin-left: 80px;
}

.top-bar {
  height: 70px;
  background: white;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 2.5rem;
  border-bottom: 1px solid #e2e8f0;
  position: sticky;
  top: 0;
  z-index: 90;
}

.top-bar-left .page-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
}

.top-bar-right {
  display: flex;
  align-items: center;
  gap: 2rem;
}

.tenant-info {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  padding-right: 2rem;
  border-right: 1px solid #e2e8f0;
}

.tenant-badge {
  background: #f1f5f9;
  color: #64748b;
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  text-transform: uppercase;
}

.tenant-name {
  font-weight: 600;
  color: #475569;
  font-size: 0.9rem;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-name {
  font-weight: 600;
  color: #1e293b;
  font-size: 0.9rem;
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: white;
  font-size: 0.9rem;
}

/* Logout Button */
.logout-btn {
  width: 100%;
  padding: 0.8rem;
  background: transparent;
  border: none;
  color: #ef4444;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  transition: background 0.2s;
  font-weight: 500;
}

.logout-btn:hover {
  background: rgba(239, 68, 68, 0.1);
}

/* Transitions */
.page-fade-enter-active,
.page-fade-leave-active {
  transition: all 0.2s ease;
}

.page-fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.page-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.layout-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: calc(100vh - 70px); /* Hauteur moins la top-bar */
}

.page-content {
  flex: 1;
  padding: 2.5rem; /* Marges standardisées */
  display: flex;
  flex-direction: column;
}
</style>
