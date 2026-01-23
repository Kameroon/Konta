<template>
  <div class="auth-box fade-in">
    <div class="auth-header">
      <h2>Créer votre compte</h2>
      <p v-if="selectedPlan">Vous avez choisi le plan <strong>{{ selectedPlan }}</strong></p>
      <p v-else>Rejoignez la révolution de la gestion financière</p>
    </div>

    <form @submit.prevent="handleRegister" class="auth-form">
      <div class="form-row">
        <div class="form-group">
          <label for="firstName">Prénom</label>
          <input 
            type="text" 
            id="firstName" 
            v-model="form.firstName" 
            placeholder="Jean" 
            required
            :disabled="loading"
          />
        </div>
        <div class="form-group">
          <label for="lastName">Nom</label>
          <input 
            type="text" 
            id="lastName" 
            v-model="form.lastName" 
            placeholder="Dupont" 
            required
            :disabled="loading"
          />
        </div>
      </div>

      <div class="form-group">
        <label for="siret">Numéro SIRET</label>
        <div class="input-with-button">
          <div class="input-with-icon">
            <i class="fas fa-fingerprint"></i>
            <input 
              type="text" 
              id="siret" 
              v-model="form.siret" 
              placeholder="123 456 789 00012" 
              required
              :disabled="loading"
              maxlength="14"
            />
          </div>
          <button type="button" class="btn-lookup" @click="handleLookup" :disabled="loading || form.siret.length < 9">
            <i class="fas fa-search" v-if="!lookingUp"></i>
            <span v-else class="loader-small"></span>
          </button>
        </div>
        <small class="form-help" v-if="lookupError">{{ lookupError }}</small>
        <small class="form-success" v-if="companyFound">Information récupérée !</small>
      </div>

      <div class="form-group">
        <label for="companyName">Nom de l'entreprise</label>
        <div class="input-with-icon">
          <i class="fas fa-building"></i>
          <input 
            type="text" 
            id="companyName" 
            v-model="form.tenantName" 
            placeholder="Konta Corp" 
            required
            :disabled="loading || companyFound"
          />
        </div>
      </div>

      <div class="form-group">
        <label for="email">Adresse email professionelle</label>
        <div class="input-with-icon">
          <i class="fas fa-envelope"></i>
          <input 
            type="email" 
            id="email" 
            v-model="form.email" 
            placeholder="admin@entreprise.com" 
            required
            :disabled="loading || !companyFound"
          />
        </div>
      </div>

      <div class="form-group">
        <label for="password">Mot de passe</label>
        <div class="input-with-icon">
          <i class="fas fa-lock"></i>
          <input 
            type="password" 
            id="password" 
            v-model="form.password" 
            placeholder="••••••••" 
            required
            :disabled="loading || !companyFound"
          />
        </div>
      </div>

      <div class="form-footer">
        <label class="checkbox-container">
          <input type="checkbox" required :disabled="loading || !companyFound">
          <span class="checkmark"></span>
          J'accepte les <a href="#">Conditions d'Utilisation</a>
        </label>
      </div>

      <button type="submit" class="btn-auth" :disabled="loading || !companyFound">
        <span v-if="!loading">Créer mon espace</span>
        <span v-else class="loader"></span>
      </button>
    </form>

    <div class="auth-extra">
      Déjà un compte ? 
      <router-link :to="{ name: 'Login' }" class="link">Se connecter</router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { authApi } from '@/api/auth.api';
import { useToast } from 'vue-toastification';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const toast = useToast();

const selectedPlan = ref<string | null>(null);
const loading = ref(false);
const lookingUp = ref(false);
const lookupError = ref('');
const companyFound = ref(false);

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  tenantName: '',
  siret: ''
});

onMounted(() => {
  selectedPlan.value = (route.query.plan as string) || null;
});

const handleLookup = async () => {
  if (form.siret.length < 9) return;
  
  lookingUp.value = true;
  lookupError.value = '';
  companyFound.value = false;
  
  try {
    const data = await authApi.lookupCompany(form.siret);
    if (data) {
      form.tenantName = data.name;
      companyFound.value = true;
      toast.info(`Entreprise identifiée : ${data.name}`);
    }
  } catch (err: any) {
    lookupError.value = "Impossible de trouver l'entreprise. Vérifiez le SIRET.";
  } finally {
    lookingUp.value = false;
  }
};

const handleRegister = async () => {
  loading.value = true;
  try {
    const success = await authStore.register({
      ...form,
      plan: selectedPlan.value || 'free'
    });
    
    if (success) {
      toast.success('Compte créé avec succès ! Bienvenue chez Konta.');
      router.push({ name: 'Dashboard' });
    }
  } catch (error: any) {
    toast.error(error.response?.data?.message || 'Erreur lors de l\'inscription');
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.auth-box {
  width: 100%;
  max-width: 480px;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(20px);
  padding: 40px;
  border-radius: 24px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.5);
}

.auth-header {
  text-align: center;
  margin-bottom: 30px;
}

.auth-header h2 {
  font-size: 2rem;
  font-weight: 800;
  color: #1a202c;
  margin-bottom: 10px;
}

.auth-header p {
  color: #718096;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-row {
  display: flex;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #4a5568;
}

.input-with-icon {
  position: relative;
}

.input-with-icon i {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  color: #a0aec0;
}

input {
  width: 100%;
  padding: 12px 16px;
  padding-left: 44px;
  border: 2px solid #edf2f7;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.2s;
  background: white;
}

.form-row input {
  padding-left: 16px;
}

input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.15);
}

.input-with-button {
  display: flex;
  gap: 8px;
}

.btn-lookup {
  padding: 0 16px;
  background: white;
  border: 2px solid #edf2f7;
  border-radius: 12px;
  color: #3182ce;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-lookup:hover:not(:disabled) {
  background: #f7fafc;
  border-color: #3182ce;
}

.btn-lookup:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.form-help {
  color: #e53e3e;
  font-size: 0.75rem;
}

.form-success {
  color: #38a169;
  font-size: 0.75rem;
}

.loader-small {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(49, 130, 206, 0.2);
  border-radius: 50%;
  border-top-color: #3182ce;
  animation: spin 1s linear infinite;
  display: inline-block;
}

.btn-auth {
  margin-top: 10px;
  padding: 14px;
  background: #3182ce;
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  justify-content: center;
  align-items: center;
}

.btn-auth:hover {
  background: #2b6cb0;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(49, 130, 206, 0.3);
}

.btn-auth:disabled {
  background: #a0aec0;
  cursor: not-allowed;
  transform: none;
}

.auth-extra {
  margin-top: 24px;
  text-align: center;
  font-size: 0.875rem;
  color: #718096;
}

.link {
  color: #3182ce;
  font-weight: 600;
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}

.loader {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Base fade-in animation */
.fade-in {
  animation: fadeIn 0.5s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
