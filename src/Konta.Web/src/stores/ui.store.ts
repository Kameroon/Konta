import { defineStore } from 'pinia';
import { ref } from 'vue';

/**
 * UI Store : Gère l'état visuel global de l'application.
 * Tous les commentaires et logs sont en français comme demandé.
 */
export const useUiStore = defineStore('ui', () => {
    // --- ÉTAT (State) ---
    const isSidebarOpen = ref(true); // État d'ouverture de la barre latérale
    const isDarkMode = ref(false); // Mode sombre (pour évolution future)
    const globalLoading = ref(false); // État de chargement global du SPA

    // --- ACTIONS ---

    /**
     * Alterne l'état de la barre latérale.
     */
    const toggleSidebar = () => {
        console.log('[UI Store] Alternance de la barre latérale');
        isSidebarOpen.value = !isSidebarOpen.value;
    };

    /**
     * Définit l'état de chargement global.
     */
    const setLoading = (status: boolean) => {
        console.log(`[UI Store] Définition du chargement global : ${status}`);
        globalLoading.value = status;
    };

    /**
     * Alterne le mode sombre.
     */
    const toggleDarkMode = () => {
        console.log('[UI Store] Alternance du mode sombre');
        isDarkMode.value = !isDarkMode.value;
        // Logique de persistance optionnelle ici
    };

    return {
        isSidebarOpen,
        isDarkMode,
        globalLoading,
        toggleSidebar,
        setLoading,
        toggleDarkMode
    };
});
