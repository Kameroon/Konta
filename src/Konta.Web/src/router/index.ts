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
                component: () => import('@/views/DocumentsView.vue'), // Shortcut to existing view for now
                meta: { title: 'Téléchargement' }
            },
            {
                path: 'documents',
                name: 'Documents',
                component: () => import('@/views/DocumentsView.vue'),
                meta: { title: 'Documents' }
            },
            {
                path: 'extracted-data',
                name: 'ExtractedData',
                component: () => import('@/views/ExtractedDataView.vue'),
                meta: { title: 'Données Extraites' }
            },
            {
                path: 'companies',
                name: 'Companies',
                component: () => import('@/views/admin/CompaniesView.vue'),
                meta: {
                    title: 'Entreprises',
                    roles: ['SuperAdmin']
                }
            },
            {
                path: 'profile',
                name: 'Profile',
                component: () => import('@/views/ProfileView.vue'),
                meta: { title: 'Mon Profil' }
            },
            {
                path: 'admin',
                name: 'Admin',
                component: () => import('@/views/admin/AdminView.vue'),
                meta: {
                    title: 'Utilisateurs',
                    roles: ['Admin', 'SuperAdmin']
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

    // 2. Vérification des rôles (RBAC)
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

    // 3. Prévenir l'accès aux pages d'auth si déjà connecté
    if (to.name === 'Login' && authStore.isAuthenticated) {
        console.log('[Router] Utilisateur déjà connecté. Redirection vers le Dashboard.');
        return next({ name: 'Dashboard' });
    }

    // Si tout est ok
    next();
});

export default router;
