import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { useToast } from 'vue-toastification';

/**
 * Définition des routes de l'application.
 * Utilise le lazy loading pour optimiser les performances.
 */
const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        component: () => import('@/layouts/PublicLayout.vue'),
        children: [
            {
                path: '',
                name: 'Home',
                redirect: '/plans' // Par défaut, on montre les plans
            },
            {
                path: 'plans',
                name: 'Plans',
                component: () => import('@/views/auth/PlansView.vue'),
                meta: { title: 'Nos Forfaits' }
            },
            {
                path: 'auth/login',
                name: 'Login',
                component: () => import('@/views/auth/LoginView.vue'),
                meta: { title: 'Connexion' }
            },
            {
                path: 'auth/register',
                name: 'Register',
                component: () => import('@/views/auth/RegisterView.vue'),
                meta: { title: 'Inscription' }
            }
        ]
    },
    {
        path: '/app',
        component: () => import('@/layouts/MainLayout.vue'),
        meta: { requiresAuth: true },
        children: [
            {
                path: 'dashboard',
                name: 'Dashboard',
                component: () => import('@/views/dashboard/DashboardView.vue'),
                meta: { title: 'Tableau de Bord' }
            },
            {
                path: 'download',
                name: 'Download',
                component: () => import('@/views/DocumentsView.vue'),
                meta: {
                    title: 'Téléchargement',
                    excludeRoles: ['SuperAdmin'] // Non accessible aux SuperAdmins
                }
            },
            {
                path: 'documents',
                name: 'Documents',
                component: () => import('@/views/ArchiveView.vue'),
                meta: {
                    title: 'Mes Documents',
                    excludeRoles: ['SuperAdmin']
                }
            },
            {
                path: 'extracted-data',
                name: 'ExtractedData',
                component: () => import('@/views/ExtractedDataView.vue'),
                meta: {
                    title: 'Données Extraites',
                    roles: ['SuperAdmin']
                }
            },
            {
                path: 'companies',
                name: 'Companies',
                component: () => import('@/views/admin/CompaniesView.vue'),
                meta: {
                    title: 'Entreprises',
                    roles: ['SuperAdmin', 'Admin', 'Manager']
                }
            },
            {
                path: 'profile',
                name: 'Profile',
                component: () => import('@/views/ProfileView.vue'),
                meta: {
                    title: 'Mon Profil',
                    excludeRoles: ['SuperAdmin']
                }
            },
            {
                path: 'admin',
                name: 'Admin',
                component: () => import('@/views/admin/AdminView.vue'),
                meta: {
                    title: 'Utilisateurs',
                    roles: ['SuperAdmin']
                }
            },
            {
                path: 'settings',
                name: 'Settings',
                component: () => import('@/views/admin/SettingsView.vue'),
                meta: {
                    title: 'Paramètres',
                    roles: ['SuperAdmin']
                }
            },
            {
                path: 'access',
                name: 'Access',
                component: () => import('@/views/admin/AccessView.vue'),
                meta: {
                    title: 'Gestion des Accès',
                    roles: ['SuperAdmin']
                }
            }
        ]
    },
    {
        path: '/unauthorized',
        name: 'Unauthorized',
        component: () => import('@/views/errors/UnauthorizedView.vue'),
        meta: { title: 'Accès Refusé' }
    },
    {
        path: '/:pathMatch(.*)*',
        redirect: '/'
    }
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
    scrollBehavior() {
        return { top: 0 };
    }
});

/**
 * Navigation Guard : Sécurisation globale des routes.
 * Gère l'authentification et les permissions par rôle.
 */
router.beforeEach(async (to, from, next) => {
    const authStore = useAuthStore();
    const toast = useToast();

    // Mise à jour du titre de la page
    document.title = `${to.meta.title || 'Konta ERP'} - Konta`;

    // 1. Vérification de l'authentification
    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
        console.warn(`[Router] Accès refusé à ${to.path}. Redirection vers Login.`);
        toast.info('Veuillez vous connecter pour accéder à cette page.');
        return next({ name: 'Login', query: { redirect: to.fullPath } });
    }

    // 2. Vérification des rôles requis (RBAC)
    if (to.meta.roles) {
        const requiredRoles = to.meta.roles as string[];
        const userRole = authStore.user?.role || '';

        // Vérifie si l'utilisateur possède l'un des rôles requis
        const hasRole = requiredRoles.includes(userRole);

        if (!hasRole) {
            console.error(`[Router] Droits insuffisants pour ${to.path}. Rôle requis parmi : ${requiredRoles}`);
            toast.error("Vous n'avez pas les droits nécessaires pour accéder à cet espace.");
            return next({ name: 'Unauthorized' });
        }
    }

    // 2b. Vérification des rôles exclus (certaines pages ne sont PAS accessibles à certains rôles)
    if (to.meta.excludeRoles) {
        const excludedRoles = to.meta.excludeRoles as string[];
        const userRole = authStore.user?.role || '';

        // Si l'utilisateur a un rôle exclu, on le redirige vers Dashboard
        if (excludedRoles.includes(userRole)) {
            console.warn(`[Router] Le rôle ${userRole} n'a pas accès à ${to.path}. Redirection.`);
            return next({ name: 'Dashboard' });
        }
    }

    // 3. Rediriger les utilisateurs déjà connectés vers le Dashboard
    // Quand ils accèdent aux pages publiques (login, register, plans, home)
    const publicPages = ['Login', 'Register', 'Plans', 'Home'];
    if (publicPages.includes(to.name as string) && authStore.isAuthenticated) {
        console.log('[Router] Utilisateur connecté. Redirection vers le Dashboard.');
        return next({ name: 'Dashboard' });
    }

    // Si tout est ok
    next();
});

export default router;
