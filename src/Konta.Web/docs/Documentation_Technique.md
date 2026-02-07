# Documentation Technique - Konta ERP

Ce document centralise les choix techniques, l'architecture et l'évolution du projet `Konta.Web`. Il sera mis à jour régulièrement à chaque étape du développement.

---

## 🏗️ Architecture Globale
Le projet `Konta.Web` est le frontend du système ERP Konta, conçu pour être performant, modulaire et multi-tenant.

- **Framework** : Vue.js 3 (Composition API)
- **Langage** : TypeScript (Strict Mode)
- **Build Tool** : Vite 5
- **IDE Support** : Visual Studio 2022 & VS Code

---

## 🛠️ Configuration Technique

### Structure du Projet
```text
src/Konta.Web/
├── .esproj               # Configuration Visual Studio 2022
├── package.json          # Dépendances et scripts
├── vite.config.ts        # Configuration du bundler Vite
├── tsconfig.json         # Configuration TypeScript (composite)
├── index.html            # Point d'entrée HTML
└── src/
    ├── api/              # Couche de communication HTTP
    │   ├── http.ts       # Client Axios + Intercepteurs
    │   ├── auth.api.ts   # Appels API Authentification
    │   ├── finance.api.ts # Appels API Financiers & Reporting
    │   └── documents.api.ts # Gestion Documents & OCR
    ├── components/       # Composants réutilisables
    │   └── ui/           # Bibliothèque de composants UI (StatCard, Loaders, Upload)
    ├── router/           # Configuration du routage
    │   └── index.ts      # Définition des routes & Guards (RBAC)
    ├── layouts/          # Gabarits de structure UI
    │   ├── AuthLayout.vue
    │   └── MainLayout.vue
    ├── views/            # Vues applicatives (Pages)
    ├── stores/           # Gestion de l'état global
    │   ├── auth.store.ts   # Authentification & JWT
    │   ├── tenant.store.ts # État SaaS & Abonnement
    │   └── ui.store.ts     # État de l'interface (Sidebar, etc.)
    ├── types/            # Définitions TypeScript
    │   ├── auth.types.ts     # Interfaces Auth
    │   ├── finance.types.ts  # Interfaces Finance & Analytics
    │   ├── document.types.ts # Interfaces Documents & OCR
    │   └── user.types.ts     # Interfaces SaaS & Profil
    └── styles/
        └── main.css      # Styles globaux
```

### Scripts Disponibles
- `npm install` : Installe les dépendances.
- `npm run dev` : Lance le serveur de développement (HMR).
- `npm run build` : Génère le bundle de production dans `dist/`.
- `npm run type-check` : Vérifie les types TypeScript.

---

## ✅ Procédures de Vérification
Pour s'assurer que tout est bien configuré :

1.  **Validation TypeScript** : Exécuter `npm run type-check`. Le résultat doit être un code de sortie `0` (zéro erreur).
2.  **Installation** : Exécuter `npm install`. Toutes les dépendances doivent être résolues sans erreur critique.
3.  **Exécution** : Exécuter `npm run dev`. L'application doit être accessible sur `http://localhost:5173`.
4.  **Intégration Visual Studio** : Le projet `Konta.Web` doit être visible dans l'Explorateur de solutions et compilable.

---

## 📅 Journal des Évolutions

### [2026-01-21] LOT 0 — Bootstrap & Socle Technique
- **Initialisation** : Mise en place de Vue 3 + Vite + TypeScript.
- **Correction Configuration** : Ajustement des extensions `tsconfig` pour compatibilité avec `@vue/tsconfig` v0.5.1.
- **Compatibilité** : Ajout du fichier `.esproj` pour le support natif dans Visual Studio 2022.

### [2026-01-21] LOT 1 — Authentification, HTTP & Sécurité
- **HTTP Layer** : Implémentation d'Axios avec intercepteurs pour la gestion automatique du `Bearer Token`.
- **JWT Refresh** : Logique de rafraîchissement automatique du token (401) intégrée dans le client HTTP.
- **State Management** : Mise en place de **Pinia** pour gérer l'état d'authentification.

### [2026-01-21] LOT 2 — Routing, Layouts & RBAC
- **Routage** : Implémentation de `vue-router` avec support du lazy loading.
- **RBAC** : Gestion des droits d'accès basée sur les rôles utilisateur.
- **Layouts** : Création de `AuthLayout` et `MainLayout` pour structurer l'interface.

### [2026-01-21] LOT 3 — Dashboard Financier
- **KPIs** : Affichage dynamique des indicateurs clés (CA, Dépenses, Marge).
- **Intégration API** : Branchement réel sur le microservice `Konta.Reporting`.
- **UI Components** : Création de `StatCard` et `LoadingOverlay`.

### [2026-01-21] LOT 4 — Documents & OCR
- **Upload** : Système d'upload PDF via `multipart/form-data`.
- **OCR Tracking** : Suivi asynchrone de l'extraction de données via `Konta.Ocr`.
- **Visualisation** : Affichage des résultats structurés extraits par l'IA.

- **Login Réel** : Intégration effective de la connexion avec `Konta.Identity`.

### [2026-01-21] LOT 6 — Modèle d'Abonnement (Dynamic Plans)
- **Database** : Création du schéma `billing.SubscriptionPlans` avec gestion fine des quotas (utilisateurs, stockage, support, API).
- **Backend** : Implémentation du repository et de l'endpoint public pour le catalogue d'offres.
- **Frontend** : Intégration dynamique des 5 niveaux de forfaits (Découverte, Basique, Avancée, Premium, Expertise) avec design premium réactif.
- **Processus de Vente** : Liaison avec Stripe Checkout et redirection vers le flux d'inscription.

---

## 👥 Gestion Multi-Tenant & Hiérarchie
L'application utilise une structure multi-tenant stricte avec isolation forcée par la base de données (RLS).

### Niveaux d'Accès
1.  **SuperAdmin (Plateforme)** :
    *   **Périmètre** : Global (tous les tenants).
    *   **TenantId** : `00000000-0000-0000-0000-000000000000` (Système).
    *   **Capacités** : Liste et gestion de toutes les entreprises, supervision globale, configuration des plans.
    *   **Multi-SuperAdmin** : Le système supporte plusieurs SuperAdmins associés au tenant système.
2.  **Admin (Entreprise)** :
    *   **Périmètre** : Local (uniquement son propre tenant).
    *   **Capacités** : Gestion des utilisateurs de son entreprise, souscription aux plans, configuration financière locale.
3.  **User / Accountant (Collaborateur)** :
    *   **Périmètre** : Local avec permissions restreintes définies par l'Admin local.

### Sécurité Row Level (RLS)
L'isolation est garantie au niveau PostgreSQL via la fonction `current_tenant_id()`. Aucune requête ne peut "fuiter" d'un tenant à un autre, même en cas d'erreur dans le code applicatif, car la base de données bloque l'accès aux lignes dont le `TenantId` ne correspond pas à la session active.

---

## 🔒 Sécurité & Standards
- **Routage Sécurisé** : Protection granulaire des routes par flag `requiresAuth` et tableau `roles`.
- **RBAC Client-side** : Validation des permissions avant l'affichage des composants.
- **Authentification JWT** : Utilisation d'Access Tokens et Refresh Tokens (rotation automatique).
- **ApiResponse Wrapper** : Toutes les réponses API sont enveloppées dans une structure standard `{ success, message, data, errors }`.
- **Enums as Strings** : Les énumérations backend sont sérialisées en chaînes de caractères pour une meilleure lisibilité.
- **Gestion d'Erreurs** : Centralisation des erreurs HTTP (401, 403, 500) avec notifications visuelles.
- **TypeScript Strict** : Typage complet des payloads API synchronisé avec les DTOs backend.
- **Fichiers .env** : Utilisation de variables d'environnement pour les URLs des microservices.

---

## 📅 Journal des Évolutions

### [2026-01-23] LOT 7 — Routage, SIRET & Ocelot
- **Gateway Fix** : Restauration et priorisation de la route de recherche SIRET dans Ocelot (Priorité 100).
- **DTO Alignment** : Standardisation de la sérialisation JSON via `JsonPropertyName` pour garantir le camelCase entre C# et Vue.js.
- **Navigation RBAC** : Restriction dynamique du menu "Entreprises" aux seuls profils `SuperAdmin`.

### [2026-01-23] LOT 8 — Sécurité Database & SuperAdmins
- **RLS Integration** : Consolidation du script `database_init.sql` avec les politiques d'isolation PostgreSQL.
- **Support Multi-SuperAdmin** : Extension du script d'initialisation pour inclure plusieurs comptes administrateurs plateforme (`admin` et `support`).
- **Idempotence** : Optimisation des scripts SQL pour permettre des ré-exécutions sans erreurs.
### [2026-02-06] LOT 9 — Fixes UI & Déploiement Azure
- **Déploiement Automatisé** : Mise en place d'un workflow GitHub Actions pour Azure Static Web Apps.
- **Erreurs TypeScript** : Résolution des conflits de types sur `RegisterRequest` et déclaration globale de `__BUILD_TIME__`.
- **UI Tweaks** : Correction du texte blanc invisible sur fond clair et masquage de la colonne adresse.

### [2026-02-07] LOT 10 — Support Mise à Jour & Personnalisation UI
- **Backend Support** : Implémentation de `PUT /api/tenants/{id}` dans `Konta.Tenant` pour permettre la modification des entreprises.
- **UI Customization** : Mise en gras de la version de build dans la sidebar et simplification des liens du footer (redirection stratégique vers `/plans` et suppression des liens Sécurité, API, Carrières et Blog).
### [2026-02-07] LOT 11 — Menu de Navigation Dynamique
- **Gestion Dynamique** : Migration du menu de navigation (sidebar) d'une structure statique vers une gestion dynamique pilotée par la base de données.
- **Admin Interface** : Ajout d'une interface de gestion dans les Paramètres (Settings) permettant aux SuperAdmins d'activer/désactiver des boutons, changer les libellés, les icônes et restreindre l'accès par rôle.
- **Architecture** : Création du microservice Navigation intégré à `Konta.Identity` (Models, Repository, Service, Endpoints) et routage via Ocelot.
- **Frontend** : Implémentation de `navigation.api.ts` et mise à jour de `MainLayout.vue` avec état de chargement et filtrage des droits côté client.
