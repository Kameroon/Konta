<template>
  <div class="register-container">
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
          <div class="input-with-icon password-field">
            <i class="fas fa-lock"></i>
            <input 
              :type="showPassword ? 'text' : 'password'" 
              id="password" 
              v-model="form.password" 
              placeholder="••••••••" 
              required
              :disabled="loading || !companyFound"
            />
            <button type="button" class="btn-toggle-password" @click="showPassword = !showPassword" tabindex="-1">
              <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
            </button>
          </div>
        </div>

        <div class="form-group">
          <label for="confirmPassword">Confirmer le mot de passe</label>
          <div class="input-with-icon password-field">
            <i class="fas fa-lock"></i>
            <input 
              :type="showConfirmPassword ? 'text' : 'password'" 
              id="confirmPassword" 
              v-model="form.confirmPassword" 
              placeholder="••••••••" 
              required
              :disabled="loading || !companyFound"
            />
            <button type="button" class="btn-toggle-password" @click="showConfirmPassword = !showConfirmPassword" tabindex="-1">
              <i :class="showConfirmPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
            </button>
          </div>
          <small class="form-help" v-if="passwordMismatch">Les mots de passe ne correspondent pas</small>
        </div>

        <div class="form-footer">
          <label class="checkbox-container">
            <input type="checkbox" v-model="termsAccepted" :disabled="loading || !companyFound">
            <span class="checkmark"></span>
            <span class="checkbox-label">J'accepte les <a href="#">Conditions d'Utilisation</a></span>
          </label>
        </div>

        <button type="submit" class="btn-auth" :disabled="loading || !companyFound || !isFormValid">
          <span v-if="!loading">Créer mon espace</span>
          <span v-else class="loader"></span>
        </button>
      </form>

      <div class="auth-extra">
        Déjà un compte ? 
        <router-link :to="{ name: 'Login' }" class="link">Se connecter</router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
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
const showPassword = ref(false);
const showConfirmPassword = ref(false);

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  tenantName: '',
  siret: ''
});

const termsAccepted = ref(false);

// Computed pour la validation du formulaire
const passwordMismatch = computed(() => {
  return form.confirmPassword.length > 0 && form.password !== form.confirmPassword;
});

const isFormValid = computed(() => {
  return form.password === form.confirmPassword && 
         form.password.length >= 6 && 
         termsAccepted.value;
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
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
      password: form.password,
      tenantName: form.tenantName,
      siret: form.siret,
      plan: selectedPlan.value || 'free'
    });
    
    if (success) {
      toast.success('Compte créé avec succès ! Connectez-vous pour accéder à votre espace.');
      router.push({ name: 'Login' });
    }
  } catch (error: any) {
    // Extraire les messages d'erreur du backend
    let errorMessage = 'Erreur lors de l\'inscription';
    
    if (error.response?.data?.errors && Array.isArray(error.response.data.errors)) {
      // Si c'est un tableau d'erreurs, on les affiche toutes
      errorMessage = error.response.data.errors.join(' ');
    } else if (error.response?.data?.message) {
      // Sinon on prend le message principal
      errorMessage = error.response.data.message;
    }
    
    toast.error(errorMessage);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.register-container {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  min-height: calc(100vh - 80px);
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
}

.auth-box {
  width: 100%;
  max-width: 480px;
  background: white;
  padding: 40px;
  border-radius: 24px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.05);
  border: 1px solid #edf2f7;
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
  color: #1a202c;
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

.input-with-button .input-with-icon {
  flex: 1;
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

/* Checkbox styling */
.form-footer {
  margin-top: 8px;
}

.checkbox-container {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  font-size: 0.875rem;
  color: #4a5568;
  user-select: none;
}

.checkbox-container input[type="checkbox"] {
  width: 18px;
  height: 18px;
  margin: 0;
  accent-color: #3182ce;
  cursor: pointer;
  flex-shrink: 0;
}

.checkbox-label {
  line-height: 1.4;
}

.checkbox-label a {
  color: #3182ce;
  text-decoration: none;
  font-weight: 600;
}

.checkbox-label a:hover {
  text-decoration: underline;
}

.checkmark {
  display: none;
}

/* Password toggle button */
.password-field {
  position: relative;
}

.password-field input {
  padding-right: 48px;
}

.btn-toggle-password {
  position: absolute;
  right: 14px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  color: #a0aec0;
  transition: color 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  z-index: 2;
}

.btn-toggle-password:hover {
  color: #3182ce;
}

.btn-toggle-password:focus {
  outline: none;
}

.btn-toggle-password i {
  font-size: 1rem;
}
</style>
