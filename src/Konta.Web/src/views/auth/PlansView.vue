<template>
  <div class="plans-page">
    <!-- Hero Section -->
    <div class="hero-section">
      <div class="container">
        <h1>Des Forfaits Pensés pour <span>Votre Croissance</span></h1>
        <p class="hero-subtitle">
          De l'auto-entrepreneur à la grande entreprise, Konta s'adapte à votre échelle.
        </p>
      </div>
    </div>

    <!-- Pricing Grid -->
    <div class="container">
      <div v-if="loading" class="loading-container">
        <div class="premium-spinner"></div>
        <p>Analyse des meilleures offres...</p>
      </div>

      <div v-else-if="error" class="error-container">
        <i class="fas fa-exclamation-triangle"></i>
        <h3>Oups ! Une erreur est survenue</h3>
        <p>{{ error }}</p>
        <button @click="fetchPlans" class="btn-primary">Réessayer</button>
      </div>

      <div v-else class="pricing-grid">
        <div 
          v-for="plan in plans" 
          :key="plan.id"
          :class="['plan-card', plan.code, { 'most-popular': plan.code === 'premium' }]"
        >
          <div v-if="plan.code === 'premium'" class="popular-tag">Plus Populaire</div>
          
          <div class="plan-header">
            <div class="icon-wrapper">
              <i :class="getPlanIcon(plan.code)"></i>
            </div>
            <h3>{{ plan.name }}</h3>
            <p class="plan-tagline">{{ getPlanTagline(plan.code) }}</p>
          </div>

          <div class="plan-price">
            <template v-if="plan.price > 0">
              <span class="currency">€</span>
              <span class="amount">{{ formatPrice(plan.price) }}</span>
              <span class="period">/m</span>
            </template>
            <template v-else-if="plan.code === 'discovery'">
              <span class="amount">Gratuit</span>
            </template>
            <template v-else>
              <span class="amount quote">Devis</span>
            </template>
          </div>

          <ul class="plan-features">
            <li v-for="(feature, index) in parseFeatures(plan.features)" :key="index">
              <i class="fas fa-check"></i>
              <span>{{ feature }}</span>
            </li>
            <li v-if="plan.hasPrioritySupport">
              <i class="fas fa-star"></i>
              <span>Support 24/7</span>
            </li>
            <li v-if="plan.hasApiAccess">
              <i class="fas fa-code"></i>
              <span>Accès API</span>
            </li>
          </ul>

          <div class="plan-footer">
            <button 
              @click="selectPlan(plan.code)" 
              :class="['btn-action', { 'btn-premium': plan.code === 'premium' || plan.code === 'advanced' }]"
            >
              {{ plan.code === 'expertise' ? 'Parler à un Expert' : 'Choisir' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Trust Section -->
    <div class="trust-section">
      <div class="container">
        <p>Ils nous font confiance</p>
        <div class="logos-scroll">
          <i class="fab fa-apple"></i>
          <i class="fab fa-google"></i>
          <i class="fab fa-stripe"></i>
          <i class="fab fa-airbnb"></i>
          <i class="fab fa-slack"></i>
        </div>
      </div>
    </div>

    <!-- Modal de confirmation moderne -->
    <ConfirmUpgradeModal 
      :show="showConfirmModal"
      :targetPlan="selectedPlanName"
      :currentPlan="tenantStore.currentTenant?.plan"
      :loading="upgrading"
      @close="showConfirmModal = false"
      @confirm="handleConfirmUpgrade"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { useTenantStore } from '@/stores/tenant.store';
import { billingApi, type SubscriptionPlan } from '@/api/billing.api';
import { tenantApi } from '@/api/tenant.api';
import { useToast } from 'vue-toastification';
import ConfirmUpgradeModal from '@/components/ui/ConfirmUpgradeModal.vue';

const router = useRouter();
const authStore = useAuthStore();
const tenantStore = useTenantStore();
const toast = useToast();

const plans = ref<SubscriptionPlan[]>([]);
const loading = ref(true);
const upgrading = ref(false);
const error = ref<string | null>(null);

// Modal state
const showConfirmModal = ref(false);
const selectedPlanCode = ref('');
const selectedPlanName = ref('');

const fetchPlans = async () => {
  loading.value = true;
  error.value = null;
  try {
    const response = await billingApi.getPlans();
    if (response.success && response.data) {
      plans.value = response.data;
    } else {
      error.value = response.message || "Erreur de chargement.";
    }
  } catch (err: any) {
    error.value = "Connexion impossible au serveur de facturation.";
  } finally {
    loading.value = false;
  }
};

onMounted(fetchPlans);

const selectPlan = async (planCode: string) => {
  if (planCode === 'expertise') {
    window.location.href = 'mailto:contact@konta.com?subject=Demande Plan Expertise';
    return;
  }

  // Si l'utilisateur est déjà connecté, on traite la mise à jour immédiate (Upgrade)
  if (authStore.isAuthenticated && authStore.user?.tenantId) {
    selectedPlanCode.value = planCode;
    // Format name for display
    let name = planCode.charAt(0).toUpperCase() + planCode.slice(1);
    if (name === 'Discovery') name = 'Free';
    selectedPlanName.value = name;
    
    showConfirmModal.value = true;
    return;
  }

  // Sinon, redirection classique vers l'inscription pour les nouveaux clients
  router.push({ name: 'Register', query: { plan: planCode } });
};

const handleConfirmUpgrade = async () => {
  if (!authStore.user?.tenantId) return;

  try {
    upgrading.value = true;
    const success = await tenantApi.updateTenantPlan(authStore.user.tenantId, selectedPlanName.value);
    
    if (success) {
        toast.success(`Votre abonnement a été mis à jour vers : ${selectedPlanName.value}`);
        await tenantStore.fetchTenantInfo(authStore.user.tenantId);
        showConfirmModal.value = false;
        router.push('/app/profile');
    } else {
        toast.error("Impossible de mettre à jour le plan.");
    }
  } catch (err) {
    console.error('[Plans] Erreur upgrade:', err);
    toast.error("Une erreur technique est survenue.");
  } finally {
    upgrading.value = false;
  }
};

const getPlanIcon = (code: string) => {
  switch (code) {
    case 'discovery': return 'fas fa-leaf';
    case 'basic': return 'fas fa-rocket';
    case 'advanced': return 'fas fa-bolt';
    case 'premium': return 'fas fa-crown';
    case 'expertise': return 'fas fa-gem';
    default: return 'fas fa-box';
  }
};

const getPlanTagline = (code: string) => {
  switch (code) {
    case 'discovery': return 'Pour tester';
    case 'basic': return 'Petites entreprises';
    case 'advanced': return 'Gestion poussée';
    case 'premium': return 'PME Illimitées';
    case 'expertise': return 'Grands comptes';
    default: return 'Gestion intelligente';
  }
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('fr-FR').format(price);
};

const parseFeatures = (features: any): string[] => {
  if (!features) return [];
  if (Array.isArray(features)) return features;
  try {
    return JSON.parse(features);
  } catch {
    return [];
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');

.plans-page {
  font-family: 'Plus Jakarta Sans', sans-serif;
  background: #fcfdfe;
  color: #1a202c;
  overflow-x: hidden;
}

.container {
  max-width: 1515px;
  margin: 0 auto;
  padding: 0 15px;
}

/* Hero Section - Plus compact */
.hero-section {
  padding: 60px 0 40px;
  text-align: center;
  background: radial-gradient(circle at 50% -20%, #eff6ff 0%, #fcfdfe 100%);
}

h1 {
  font-size: clamp(2rem, 3.5vw, 3rem);
  font-weight: 800;
  line-height: 1.1;
  letter-spacing: -0.04em;
  margin-bottom: 15px;
  color: #0f172a;
}

h1 span {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.hero-subtitle {
  font-size: 1.1rem;
  color: #64748b;
  max-width: 600px;
  margin: 0 auto;
  line-height: 1.4;
}

/* Pricing Grid - Optimisé pour 5 colonnes horizontales */
.pricing-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 15px;
  margin-bottom: 60px;
}

.plan-card {
  background: white;
  border-radius: 20px;
  padding: 24px 20px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  position: relative;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.02);
}

.plan-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.05);
  border-color: #3b82f6;
}

.plan-card.most-popular {
  border: 2px solid #3b82f6;
  background: linear-gradient(to bottom, #ffffff, #f8fbff);
}

.popular-tag {
  position: absolute;
  top: -12px;
  left: 50%;
  transform: translateX(-50%);
  background: #3b82f6;
  color: white;
  padding: 4px 16px;
  border-radius: 99px;
  font-weight: 800;
  font-size: 0.65rem;
  text-transform: uppercase;
  z-index: 10;
}

.plan-header {
  margin-bottom: 15px;
}

.icon-wrapper {
  width: 44px;
  height: 44px;
  background: #f1f5f9;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  color: #3b82f6;
  margin-bottom: 12px;
}

.discovery .icon-wrapper { background: #f0fdf4; color: #16a34a; }
.premium .icon-wrapper { background: #eff6ff; color: #2563eb; }
.expertise .icon-wrapper { background: #faf5ff; color: #7c3aed; }

h3 {
  font-size: 1.25rem;
  font-weight: 800;
  color: #0f172a;
  margin-bottom: 4px;
}

.plan-tagline {
  color: #64748b;
  font-size: 0.8rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.plan-price {
  margin-bottom: 20px;
  display: flex;
  align-items: baseline;
  gap: 2px;
}

.plan-price .amount {
  font-size: 2rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.05em;
}

.plan-price .currency {
  font-size: 1rem;
  font-weight: 700;
  color: #64748b;
}

.plan-price .period {
  font-size: 0.8rem;
  color: #94a3b8;
  font-weight: 500;
}

.plan-price .amount.quote {
  font-size: 1.5rem;
  color: #475569;
}

.plan-features {
  list-style: none;
  padding: 0;
  margin: 0 0 24px 0;
  flex: 1;
}

.plan-features li {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
  color: #475569;
  font-weight: 500;
  font-size: 0.75rem;
}

.plan-features li i {
  color: #3b82f6;
  font-size: 0.7rem;
}

.btn-action {
  width: 100%;
  padding: 12px;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #0f172a;
  font-weight: 700;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-action:hover {
  background: #f8fafc;
  border-color: #cbd5e0;
}

.btn-premium {
  background: #0f172a;
  color: white;
  border: none;
}

.btn-premium:hover {
  background: #1e293b;
  transform: translateY(-2px);
  box-shadow: 0 8px 15px rgba(0, 0, 0, 0.1);
}

/* Trust Section */
.trust-section {
  padding-bottom: 60px;
  text-align: center;
}

.trust-section p {
  color: #94a3b8;
  text-transform: uppercase;
  font-weight: 700;
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  margin-bottom: 24px;
}

.logos-scroll {
  display: flex;
  justify-content: center;
  gap: 40px;
  font-size: 1.75rem;
  color: #cbd5e0;
}

/* Mobile Responsivity */
@media (max-width: 1100px) {
  .pricing-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 768px) {
  .hero-section {
    padding: 40px 0 20px;
  }
  
  .pricing-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }
  
  .plan-card {
    padding: 24px;
    max-width: none;
  }
}

/* Spinner & Error */
.loading-container, .error-container {
  text-align: center;
  padding: 60px 0;
}

.premium-spinner {
  width: 44px;
  height: 44px;
  border: 4px solid #f1f5f9;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
