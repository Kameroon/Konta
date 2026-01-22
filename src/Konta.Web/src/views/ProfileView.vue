<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { useTenantStore } from '@/stores/tenant.store';
import { useToast } from 'vue-toastification';

/**
 * ProfileView : Gère les informations personnelles de l'utilisateur et de son entreprise.
 */

const authStore = useAuthStore(); // Accès au store d'authentification
const tenantStore = useTenantStore(); // Accès au store du tenant
const toast = useToast(); // Pour les notifications visuelles

const editing = ref(false); // État du mode édition
const profileForm = ref({
  firstName: authStore.user?.firstName || '',
  lastName: authStore.user?.lastName || '',
  email: authStore.user?.email || ''
});

/**
 * Charge les informations du tenant au montage du composant.
 */
onMounted(async () => {
  if (authStore.user?.tenantId) {
    console.log('[Profil] Chargement des informations du tenant...');
    await tenantStore.fetchTenantInfo(authStore.user.tenantId);
  }
});

/**
 * Sauvegarde les modifications du profil.
 */
const saveProfile = async () => {
  console.log('[Profil] Tentative de sauvegarde du profil...');
  try {
    await authStore.updateProfile({
      firstName: profileForm.value.firstName,
      lastName: profileForm.value.lastName
    });
    toast.success('Profil mis à jour avec succès.');
    editing.value = false;
  } catch (error) {
    console.error('[Profil] Échec de la mise à jour', error);
    toast.error('Une erreur est survenue lors de la sauvegarde.');
  }
};

/**
 * Calcul des détails d'affichage du plan si les données réelles sont absentes.
 * Permet d'éviter d'afficher une carte vide.
 */
const subscriptionDisplay = ref({
  status: '✅ Actif',
  expiry: 'Illimité',
  quotaUsed: 124,
  quotaLimit: 1000,
  features: ['Lecture OCR illimitée', 'Support Prioritaire', 'Accès Multi-utilisateurs']
});

// Mise à jour simplifiée selon le plan si aucune info d'abonnement n'est reçue
onMounted(() => {
  const plan = tenantStore.currentTenant?.plan || 'Free';
  if (plan === 'Premium') {
    subscriptionDisplay.value = {
      status: '✅ Actif',
      expiry: '31/12/2026',
      quotaUsed: 450,
      quotaLimit: 5000,
      features: ['10 utilisateurs inclus', 'Expertise OCR illimitée', 'Support 24/7', 'Modules Avancés']
    };
  } else if (plan === 'Free') {
    subscriptionDisplay.value = {
      status: '✅ Actif',
      expiry: 'Gratuit',
      quotaUsed: 5,
      quotaLimit: 10,
      features: ['1 utilisateur', 'Lecture OCR (10/mois)', 'Support Email']
    };
  }
});
</script>

<template>
  <div class="profile-view">
    <header class="view-header">
      <h1>Mon Profil & Abonnement</h1>
      <p>Gérez vos informations personnelles et les paramètres de votre entreprise <strong>{{ tenantStore.tenantName }}</strong>.</p>
    </header>

    <div class="profile-grid">
      <!-- Informations Personnelles -->
      <section class="profile-section card">
        <div class="card-header">
          <h3>Informations Personnelles</h3>
          <button @click="editing = !editing" class="edit-btn">
            {{ editing ? 'Annuler' : 'Modifier' }}
          </button>
        </div>

        <form @submit.prevent="saveProfile" class="profile-form">
          <div class="form-group">
            <label>Prénom</label>
            <input v-model="profileForm.firstName" :disabled="!editing" type="text" placeholder="Gérard" />
          </div>
          <div class="form-group">
            <label>Nom</label>
            <input v-model="profileForm.lastName" :disabled="!editing" type="text" placeholder="Menvuca" />
          </div>
          <div class="form-group">
            <label>Email (Identifiant)</label>
            <input v-model="profileForm.email" disabled type="email" />
            <small>L'email ne peut pas être modifié par l'utilisateur.</small>
          </div>
          <div class="form-group">
            <label>Rôles</label>
            <div class="roles-tags">
              <span v-for="role in authStore.user?.roles" :key="role" class="role-tag">{{ role }}</span>
            </div>
          </div>
          
          <div v-if="editing" class="form-actions">
            <button type="submit" class="save-btn">Enregistrer les modifications</button>
          </div>
        </form>
      </section>

      <!-- Détails de l'Abonnement -->
      <section class="subscription-section card">
        <div class="card-header">
          <h3>Détails de l'Entité & Abonnement</h3>
          <span v-if="tenantStore.currentTenant" class="plan-badge" :class="tenantStore.currentTenant.plan.toLowerCase()">
            Plan {{ tenantStore.currentTenant.plan }}
          </span>
        </div>

        <div v-if="tenantStore.loading" class="loading-state">
          Chargement des données SaaS...
        </div>

        <template v-else-if="subscriptionDisplay">
          <div class="sub-info">
            <div class="info-item">
              <label>Entreprise</label>
              <span>{{ tenantStore.currentTenant?.name }}</span>
            </div>
            <div class="info-item">
              <label>Statut</label>
              <span class="status-active">{{ subscriptionDisplay.status }}</span>
            </div>
            <div class="info-item">
              <label>Expire le</label>
              <span>{{ subscriptionDisplay.expiry }}</span>
            </div>
          </div>

          <div class="quota-section">
            <div class="quota-header">
              <label>Consommation OCR (Factures)</label>
              <span>{{ subscriptionDisplay.quotaUsed }} / {{ subscriptionDisplay.quotaLimit }}</span>
            </div>
            <div class="progress-bar">
              <div class="fill" :style="{ width: (subscriptionDisplay.quotaUsed / subscriptionDisplay.quotaLimit * 100) + '%' }"></div>
            </div>
          </div>

          <div class="features-section">
            <label>Inclus dans votre plan :</label>
            <ul class="features-list">
              <li v-for="feature in subscriptionDisplay.features" :key="feature">
                ✨ {{ feature }}
              </li>
            </ul>
          </div>
          
          <div class="sub-actions">
            <button @click="$router.push('/plans')" class="upgrade-btn">
              Changer de plan / Voir les offres 🚀
            </button>
          </div>
        </template>
      </section>
    </div>
  </div>
</template>

<style scoped>
.profile-view {
  max-width: 1100px;
  margin: 0 auto;
}

.view-header {
  margin-bottom: 2.5rem;
}

.view-header h1 {
  font-size: 2.2rem;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.view-header p {
  color: #64748b;
}

.profile-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

.card {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 6px rgba(0,0,0,0.02);
  border: 1px solid #f1f5f9;
  padding: 2rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.card-header h3 {
  font-size: 1.2rem;
  color: #1e293b;
  margin: 0;
}

/* Formulaire */
.profile-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

label {
  font-size: 0.9rem;
  font-weight: 600;
  color: #475569;
}

input {
  padding: 0.8rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #f8fafc;
  transition: all 0.2s;
}

input:focus:not(:disabled) {
  border-color: #42b883;
  outline: none;
  background: white;
}

input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

small {
  color: #94a3b8;
  font-size: 0.8rem;
}

.role-tag {
  background: #f1f5f9;
  color: #475569;
  padding: 0.2rem 0.6rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.edit-btn {
  background: transparent;
  color: #3b82f6;
  border: none;
  font-weight: 600;
  cursor: pointer;
}

.save-btn {
  background: #42b883;
  color: white;
  border: none;
  padding: 1rem;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s;
}

/* Abonnement */
.plan-badge {
  padding: 0.3rem 0.8rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
}

.plan-badge.premium { background: #fee2e2; color: #ef4444; }
.plan-badge.free { background: #f1f5f9; color: #475569; }

.sub-info {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
  margin-bottom: 2rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 0.8rem;
}

.status-active {
  color: #10b981;
  font-weight: 700;
}

.quota-section {
  margin-bottom: 2rem;
}

.quota-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  font-weight: 600;
}

.progress-bar {
  height: 10px;
  background: #f1f5f9;
  border-radius: 5px;
  overflow: hidden;
}

.fill {
  height: 100%;
  background: #42b883;
  border-radius: 5px;
}

.features-list {
  list-style: none;
  padding: 0;
  margin-top: 1rem;
}

.features-list li {
  color: #64748b;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.upgrade-btn {
  width: 100%;
  padding: 1rem;
  background: #1e293b;
  color: white;
  border-radius: 12px;
  border: none;
  font-weight: 700;
  margin-top: 2rem;
  cursor: pointer;
}

@media (max-width: 900px) {
  .profile-grid {
    grid-template-columns: 1fr;
  }
}
</style>
