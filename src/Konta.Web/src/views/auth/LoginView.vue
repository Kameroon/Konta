<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { useRouter, useRoute } from 'vue-router';
import { useToast } from 'vue-toastification';

/**
 * LoginView : Page de connexion avec intégration API réelle.
 */

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const email = ref('admin@konta.com');
const password = ref('password');
const isSubmitting = ref(false);

/**
 * Gère la soumission du formulaire de connexion.
 */
const handleLogin = async () => {
  if (!email.value || !password.value) {
    toast.warning('Veuillez remplir tous les champs.');
    return;
  }

  isSubmitting.value = true;
  console.log('[LoginView] Tentative de connexion...');

  try {
    await authStore.login({
      email: email.value,
      password: password.value
    });

    toast.success('Bienvenue sur Konta !');
    
    // Logique de redirection basée sur le rôle
    if (authStore.user?.roles.includes('Admin')) {
      router.push({ name: 'Admin' });
    } else {
      // Redirection vers la page demandée initialement ou vers le dashboard utilisateur
      const redirectPath = route.query.redirect as string || '/app/dashboard';
      router.push(redirectPath);
    }
  } catch (err: any) {
    console.error('[LoginView] Erreur:', err);
    toast.error(err.message || 'Identifiants invalides ou erreur serveur.');
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="login-view">
    <div class="login-card">
      <div class="logo-area">
        <span class="logo-icon">K</span>
        <span class="logo-text">Konta<strong>ERP</strong></span>
      </div>
      
      <h2>Bienvenue</h2>
      <p class="subtitle">Connectez-vous pour gérer votre entreprise</p>

      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>Adresse E-mail</label>
          <input 
            v-model="email" 
            type="email" 
            placeholder="jean.dupont@konta.com" 
            required
            :disabled="isSubmitting"
          />
        </div>

        <div class="form-group">
          <label>Mot de passe</label>
          <input 
            v-model="password" 
            type="password" 
            placeholder="••••••••" 
            required
            :disabled="isSubmitting"
          />
        </div>

        <button type="submit" class="login-btn" :disabled="isSubmitting">
          <span v-if="!isSubmitting">Se connecter</span>
          <span v-else class="loader"></span>
        </button>
      </form>
      
      <div class="login-footer">
        <router-link :to="{ name: 'Plans' }" class="forgot-pass">Pas encore de compte ? S'inscrire</router-link>
        <br>
        <a href="#" class="forgot-pass">Mot de passe oublié ?</a>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-view {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
}

.login-card {
  background: white;
  padding: 3rem;
  border-radius: 24px;
  box-shadow: 0 20px 50px rgba(0,0,0,0.05);
  width: 100%;
  max-width: 450px;
  text-align: center;
}

.logo-area {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.8rem;
  margin-bottom: 2rem;
}

.logo-icon {
  background-color: #42b883;
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 800;
  font-size: 1.4rem;
}

.logo-text {
  font-size: 1.5rem;
  color: #1e293b;
}

h2 {
  margin: 0;
  color: #1e293b;
  font-size: 2rem;
  font-weight: 700;
}

.subtitle {
  color: #64748b;
  margin-bottom: 2.5rem;
  font-size: 1rem;
}

.form-group {
  margin-bottom: 1.5rem;
  text-align: left;
}

label {
  display: block;
  margin-bottom: 0.6rem;
  color: #475569;
  font-weight: 600;
  font-size: 0.9rem;
}

input {
  width: 100%;
  padding: 0.9rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.2s;
  background-color: #f8fafc;
}

input:focus {
  outline: none;
  border-color: #42b883;
  background-color: white;
  box-shadow: 0 0 0 4px rgba(66, 184, 131, 0.1);
}

.login-btn {
  width: 100%;
  padding: 1rem;
  background-color: #1e293b;
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s;
  margin-top: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.login-btn:hover:not(:disabled) {
  background-color: #0f172a;
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}

.login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loader {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255,255,255,0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.login-footer {
  margin-top: 2rem;
}

.forgot-pass {
  color: #64748b;
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
}

.forgot-pass:hover {
  color: #42b883;
  text-decoration: underline;
}
</style>
