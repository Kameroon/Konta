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
  <div class="stat-card">
    <div v-if="loading" class="skeleton-loader">
      <div class="skeleton title"></div>
      <div class="skeleton value"></div>
    </div>
    
    <template v-else>
      <div class="card-content">
        <div class="stat-icon-wrapper" :style="{ backgroundColor: color + '15', color: color }">
           <i v-if="icon" :class="icon"></i>
           <span v-else>{{ label.charAt(0) }}</span>
        </div>
        
        <div class="stat-info">
          <span class="value">{{ formatValue(value) }}</span>
          <span class="label">{{ label }}</span>
          
          <div v-if="trend" class="trend" :class="trend">
            <span class="trend-value">{{ trend === 'up' ? '+' : '' }}{{ trendValue }}%</span>
            <span class="trend-label">ce mois</span>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.02);
  border: 1px solid #f1f5f9;
  transition: transform 0.2s, box-shadow 0.2s;
  overflow: hidden;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.04);
}

.card-content {
  display: flex;
  align-items: center;
  gap: 1.2rem;
}

.stat-icon-wrapper {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  flex-shrink: 0;
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.label {
  font-size: 0.85rem;
  font-weight: 500;
  color: #64748b;
  margin-top: 2px;
}

.value {
  font-size: 1.4rem;
  font-weight: 800;
  color: #0f172a;
  line-height: 1;
}

/* Trends */
.trend {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  margin-top: 4px;
  font-size: 0.75rem;
  font-weight: 700;
}

.trend.up { color: #10b981; }
.trend.down { color: #ef4444; }
.trend.stable { color: #64748b; }

.trend-label {
  font-weight: 500;
  color: #94a3b8;
  margin-left: 2px;
}

/* Skeleton Loading */
.skeleton {
  background: linear-gradient(90deg, #f8fafc 25%, #f1f5f9 50%, #f8fafc 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 8px;
}

.skeleton.title { height: 14px; width: 40%; margin-bottom: 0.5rem; }
.skeleton.value { height: 24px; width: 70%; }

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
</style>
