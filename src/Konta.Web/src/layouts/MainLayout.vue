<script setup lang="ts">
import { onMounted, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { useUiStore } from '@/stores/ui.store';
import { useTenantStore } from '@/stores/tenant.store';
import { useToast } from 'vue-toastification';
import Footer from '@/components/layout/Footer.vue';

const authStore = useAuthStore();
const uiStore = useUiStore();
const tenantStore = useTenantStore();
const router = useRouter();
const toast = useToast();

// Réaction aux changements d'utilisateur (pour le rafraîchissement à chaud si besoin)
watch(() => authStore.user?.tenantId, async (newTenantId: string | undefined) => {
    if (newTenantId && (!tenantStore.currentTenant || tenantStore.currentTenant.id !== newTenantId)) {
        console.log('[MainLayout] Changement de tenant détecté, chargement...');
        await tenantStore.fetchTenantInfo(newTenantId);
    }
}, { immediate: true });

const handleLogout = () => {
    console.log('[MainLayout] Tentative de déconnexion...');
    authStore.logout();
    toast.success('Déconnexion réussie.');
    router.push({ name: 'Login' });
};

const isAdmin = computed(() => {
    return ['Admin', 'SuperAdmin'].includes(authStore.user?.role || '') ?? false;
});

const isSuperAdmin = computed(() => {
    return authStore.user?.role === 'SuperAdmin';
});

// Version dynamique basée sur le timestamp de build
const buildVersion = computed(() => {
    try {
        const date = new Date(__BUILD_TIME__);
        return `v${date.toLocaleDateString('fr-FR', { day: '2-digit', month: '2-digit', year: '2-digit' })} - ${date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })}`;
    } catch {
        return 'v1.0.0';
    }
});
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

      <!-- Section Profil (Nouveauté Mockup) -->
      <div class="user-profile-section" v-if="uiStore.isSidebarOpen">
        <div class="avatar-large">
          {{ authStore.user?.firstName?.charAt(0) }}{{ authStore.user?.lastName?.charAt(0) }}
        </div>
        <div class="profile-details">
          <span class="full-name">{{ authStore.fullName }}</span>
          <span class="company-name" v-if="tenantStore.currentTenant">{{ tenantStore.currentTenant.name }}</span>
          <span class="user-role">{{ authStore.user?.role || 'Utilisateur' }}</span>
        </div>
      </div>
      <div class="user-profile-section-collapsed" v-else>
        <div class="avatar-sm">
          {{ authStore.user?.firstName?.charAt(0) }}{{ authStore.user?.lastName?.charAt(0) }}
        </div>
      </div>
      
      <nav class="nav-menu">
        <!-- Dashboard is common for everyone -->
        <router-link to="/app/dashboard" class="nav-item" active-class="active">
          <i class="fas fa-th-large icon"></i> <span>Tableau de bord</span>
        </router-link>

        <!-- Only for non-SuperAdmins -->
        <template v-if="!isSuperAdmin">
          <router-link to="/app/download" class="nav-item" active-class="active">
            <i class="fas fa-upload icon"></i> <span>Téléchargement</span>
          </router-link>

          <router-link to="/app/documents" class="nav-item" active-class="active">
            <i class="fas fa-file-alt icon"></i> <span>Documents</span>
          </router-link>

          <router-link to="/app/companies" class="nav-item" active-class="active">
            <i class="fas fa-building icon"></i> <span>Partenaires</span>
          </router-link>

          <router-link to="/app/profile" class="nav-item" active-class="active">
            <i class="fas fa-user-circle icon"></i> <span>Profil</span>
          </router-link>
        </template>

        <!-- Only for SuperAdmins -->
        <template v-if="isSuperAdmin">
          <router-link to="/app/extracted-data" class="nav-item" active-class="active">
            <i class="fas fa-database icon"></i> <span>Données extraites</span>
          </router-link>

          <router-link to="/app/companies" class="nav-item" active-class="active">
            <i class="fas fa-building icon"></i> <span>Entreprises</span>
          </router-link>

          <router-link to="/app/admin" class="nav-item" active-class="active">
            <i class="fas fa-users icon"></i> <span>Utilisateurs</span>
          </router-link>
        </template>

        <!-- Settings is common (or only SuperAdmin? User said SuperAdmin sees it. Others? User didn't mention it for Others, but typically it's needed) -->
        <router-link v-if="isSuperAdmin" to="/app/settings" class="nav-item" active-class="active">
          <i class="fas fa-cog icon"></i> <span>Paramètres</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <button @click="handleLogout" class="logout-btn">
          <i class="fas fa-sign-out-alt icon"></i> <span v-if="uiStore.isSidebarOpen">Déconnexion</span>
        </button>
        <div class="version-info" v-if="uiStore.isSidebarOpen">
          <strong>{{ buildVersion }}</strong>
        </div>
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
            <div class="avatar-top" :style="{ backgroundColor: '#42b883' }">
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

/* Sidebar Styling - Basée sur Mockup Blanc */
.sidebar {
  width: 280px;
  background-color: #ffffff;
  color: #1e293b;
  display: flex;
  flex-direction: column;
  position: fixed;
  height: 100vh;
  z-index: 100;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  padding: 0;
  border-right: 1px solid #e2e8f0;
}

.main-layout.sidebar-collapsed .sidebar {
  width: 80px;
}

.sidebar-header {
  padding: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 70px;
  margin-bottom: 1rem;
}

.collapse-btn {
  background: #f1f5f9;
  border: none;
  color: #64748b;
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
  background: #e2e8f0;
  color: #1e293b;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.logo-icon {
  background-color: #000;
  color: #fff;
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
  font-weight: 600;
  color: #000;
  letter-spacing: -0.5px;
}

/* User Profile Section */
.user-profile-section {
  padding: 0 1.5rem 1.5rem 1.5rem;
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 1rem;
}

.avatar-large {
  width: 48px;
  height: 48px;
  background-color: #f1f5f9;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: #1e293b;
  font-size: 1rem;
  border: 2px solid #fff;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.user-profile-section-collapsed {
  display: flex;
  justify-content: center;
  padding: 1rem 0;
}

.avatar-sm {
  width: 36px;
  height: 36px;
  background-color: #f1f5f9;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: #1e293b;
  font-size: 0.8rem;
}

.profile-details {
  display: flex;
  flex-direction: column;
}

.full-name {
  font-weight: 700;
  color: #1e293b;
  font-size: 0.95rem;
}

.company-name {
  font-size: 0.75rem;
  color: #64748b;
}

.user-role {
  font-size: 0.7rem;
  color: #42b883;
  font-weight: 700;
  text-transform: uppercase;
  margin-top: 2px;
}

.nav-menu {
  flex: 1;
  padding: 0 0.8rem;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 0.8rem 1rem;
  color: #64748b;
  text-decoration: none;
  border-radius: 8px;
  margin-bottom: 0.3rem;
  transition: all 0.2s;
  font-weight: 500;
  font-size: 0.95rem;
}

.nav-item:hover {
  background-color: #f8fafc;
  color: #1e293b;
}

.nav-item.active {
  background-color: #eff6ff;
  color: #3b82f6;
  font-weight: 600;
}

.icon {
  width: 20px;
  display: flex;
  justify-content: center;
  margin-right: 1rem;
  font-size: 1.1rem;
}

.main-layout.sidebar-collapsed .icon {
  margin-right: 0;
}

.sidebar-footer {
  padding: 1rem 0.8rem;
  border-top: 1px solid #f1f5f9;
}

.version-info {
  text-align: center;
  font-size: 0.7rem;
  color: #94a3b8;
  margin-top: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px dashed #e2e8f0;
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
  border-bottom: 1px solid #f1f5f9;
  position: sticky;
  top: 0;
  z-index: 90;
}

.top-bar-left .page-title {
  font-size: 1.3rem;
  font-weight: 800;
  color: #0f172a;
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
  border-right: 1px solid #f1f5f9;
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

.avatar-top {
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

.logout-btn {
  width: 100%;
  padding: 0.8rem;
  background: transparent;
  border: none;
  color: #64748b;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  transition: all 0.2s;
  font-weight: 500;
}

.logout-btn:hover {
  background: #fef2f2;
  color: #ef4444;
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
  min-height: calc(100vh - 70px);
}

.page-content {
  flex: 1;
  padding: 2rem 2rem 0.5rem;
  display: flex;
  flex-direction: column;
}

/* Responsive Hide Text */
@media (max-width: 1024px) {
  .logo-text, .nav-item span, .user-name, .tenant-name {
    display: none;
  }
}
</style>
