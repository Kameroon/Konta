# Konta.Web - Frontend Vue.js 3

Application frontend pour Konta ERP, développée avec Vue.js 3, TypeScript et Vite.

## 🎯 LOT 0 - Bootstrap & Socle Technique

Ce projet implémente le **LOT 0** de la stratégie de développement, fournissant le socle technique de base pour l'application.

## 🛠️ Stack Technique

- **Framework**: Vue.js 3.4.15
- **Langage**: TypeScript 5.3.3 (mode strict)
- **Build Tool**: Vite 5.0.12
- **API**: Composition API
- **IDE**: Visual Studio 2022 + Visual Studio Code

## 📋 Prérequis

- Node.js >= 18.0.0
- npm >= 9.0.0
- Visual Studio 2022 (optionnel, pour intégration IDE)

## 🚀 Installation

```bash
# Installer les dépendances
npm install
```

## 💻 Développement

### Avec npm

```bash
# Démarrer le serveur de développement
npm run dev

# Vérifier les types TypeScript
npm run type-check

# Construire pour la production
npm run build

# Prévisualiser le build de production
npm run preview
```

### Avec Visual Studio 2022

1. Ouvrir la solution `Konta.sln`
2. Définir `Konta.Web` comme projet de démarrage
3. Appuyer sur F5 ou cliquer sur "Démarrer"

Le projet se lancera automatiquement avec `npm run dev`.

## 📁 Structure du Projet

```
Konta.Web/
├── src/
│   ├── main.ts              # Point d'entrée de l'application
│   ├── App.vue              # Composant racine
│   ├── env.d.ts             # Déclarations TypeScript
│   └── styles/
│       └── main.css         # Styles globaux
├── index.html               # Template HTML
├── package.json             # Dépendances et scripts
├── vite.config.ts           # Configuration Vite
├── tsconfig.json            # Configuration TypeScript
├── tsconfig.node.json       # Configuration TS pour Vite
├── Konta.Web.esproj         # Projet Visual Studio 2022
└── README.md                # Documentation
```

## 🎨 Fonctionnalités

### LOT 0 (Actuel)
- ✅ Configuration Vue.js 3 + TypeScript + Vite
- ✅ Composition API
- ✅ TypeScript strict mode
- ✅ Intégration Visual Studio 2022
- ✅ Styles globaux et design tokens
- ✅ Page d'accueil de démonstration

### Prochains LOTs
- 🔜 **LOT 1**: Authentification & sécurité
- 🔜 **LOT 2**: Routing, layouts & RBAC
- 🔜 **LOT 3**: Dashboard financier
- 🔜 **LOT 4**: Documents & OCR
- 🔜 **LOT 5**: Profil, tenant & finitions

## 🔧 Configuration

### Variables d'Environnement

Créer un fichier `.env.local` à la racine du projet :

```env
VITE_APP_TITLE=Konta ERP
VITE_API_BASE_URL=http://localhost:5000
```

### Alias de Chemins

Le projet utilise l'alias `@` pour référencer le dossier `src/` :

```typescript
import MyComponent from '@/components/MyComponent.vue'
```

## 📦 Build de Production

```bash
npm run build
```

Les fichiers optimisés seront générés dans le dossier `dist/`.

## 🧪 Tests

Les tests seront ajoutés dans les prochains LOTs.

## 📝 Conventions de Code

- **TypeScript strict** : Tous les types doivent être explicites
- **Composition API** : Utiliser `<script setup lang="ts">`
- **Commentaires** : En français, documenter les fonctions complexes
- **Nommage** : camelCase pour les variables, PascalCase pour les composants

## 🔗 Intégration avec les Microservices

L'application consommera les microservices suivants :

- **Konta.Identity** : Authentification et gestion des utilisateurs
- **Konta.Tenant** : Gestion multi-tenant
- **Konta.Finance** : Opérations financières
- **Konta.Finance.Core** : Comptabilité générale
- **Konta.Billing** : Facturation
- **Konta.Ocr** : Extraction de documents
- **Konta.Reporting** : Rapports et analytics
- **Konta.Gateway** : API Gateway

## 📄 Licence

© 2026 Konta ERP - Tous droits réservés

## 👥 Équipe

Développé par l'équipe Konta ERP

---

**Version**: 1.0.0 - LOT 0  
**Dernière mise à jour**: Janvier 2026
