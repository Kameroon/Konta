<script setup lang="ts">
/**
 * StatCard : Composant pour afficher un indicateur clé (KPI).
 * Supporte les tendances, le formatage monétaire et les animations.
 */
interface Props {
  label: string;
  value: number;
  format?: 'currency' | 'percentage' | 'number';
  trend?: 'up' | 'down' | 'stable';
  trendValue?: number;
  icon?: string;
  color?: string;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  format: 'number',
  color: '#42b883',
  loading: false
});

/**
 * Formate la valeur selon le type demandé.
 */
const formatValue = (val: number) => {
  if (props.format === 'currency') {
    return new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(val);
  }
  if (props.format === 'percentage') {
    return `${val.toFixed(2)}%`;
  }
  return val.toLocaleString('fr-FR');
};
</script>

<template>
  <div class="stat-card" :style="{ borderLeftColor: color }">
    <div v-if="loading" class="skeleton-loader">
      <div class="skeleton title"></div>
      <div class="skeleton value"></div>
    </div>
    
    <template v-else>
      <div class="card-header">
        <span class="label">{{ label }}</span>
        <span v-if="icon" class="icon" :style="{ backgroundColor: color + '20', color: color }">
          {{ icon }}
        </span>
      </div>
      
      <div class="card-body">
        <h2 class="value">{{ formatValue(value) }}</h2>
        
        <div v-if="trend" class="trend" :class="trend">
          <span class="trend-icon">
            {{ trend === 'up' ? '↗' : trend === 'down' ? '↘' : '→' }}
          </span>
          <span v-if="trendValue" class="trend-value">{{ trendValue }}%</span>
          <span class="trend-label">vs mois dernier</span>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
  border-left: 4px solid #42b883;
  transition: transform 0.2s, box-shadow 0.2s;
  overflow: hidden;
  position: relative;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.06);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.label {
  font-size: 0.9rem;
  font-weight: 500;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
}

.value {
  font-size: 1.8rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0.5rem 0;
}

/* Trends */
.trend {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  font-weight: 600;
}

.trend.up { color: #10b981; }
.trend.down { color: #ef4444; }
.trend.stable { color: #64748b; }

.trend-label {
  font-weight: 400;
  color: #94a3b8;
  margin-left: 0.2rem;
}

/* Skeleton Loading */
.skeleton {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 4px;
}

.skeleton.title { height: 16px; width: 60%; margin-bottom: 1rem; }
.skeleton.value { height: 32px; width: 80%; }

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
</style>
