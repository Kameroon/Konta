import './styles/main.css'
import 'vue-toastification/dist/index.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import Toast, { type PluginOptions, POSITION } from 'vue-toastification'

import App from './App.vue'
import router from './router' // Import du routeur

/**
 * Initialisation de l'application Vue 3 avec Router et Store.
 */
const app = createApp(App)

// --- Configuration de Pinia ---
const pinia = createPinia()
app.use(pinia)

// --- Configuration du Routeur ---
app.use(router)

// --- Configuration de Toast ---
const toastOptions: PluginOptions = {
    position: POSITION.TOP_RIGHT,
    timeout: 5000,
    closeOnClick: true,
    pauseOnFocusLoss: true,
    pauseOnHover: true,
    draggable: true,
    draggablePercent: 0.6,
    showCloseButtonOnHover: false,
    hideProgressBar: false,
    closeButton: "button",
    icon: true,
    rtl: false
}
app.use(Toast, toastOptions)

app.mount('#app')

console.log('[Konta Web] Application initialisée avec Router, Pinia et Toasts.');
