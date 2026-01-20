# Documentation des Routes de l'API Gateway (Konta)

Ce document détaille la configuration du point d'entrée unique de la plateforme Konta (API Gateway) définie dans `ocelot.json`.

## Configuration Globale
- **Base URL** : `https://localhost:5000`
- **Méthodes autorisées** : GET, POST, PUT, DELETE, PATCH
- **Sécurité** : JWT Bearer Token requis sur toutes les routes.

---

## 1. Service d'Identité (Identity)
- **Port aval** : `5001`
- **Chemin amont** : `/gateway/identity/{everything}`
- **Chemin aval** : `/api/identity/{everything}`
- **Description** : Gère l'authentification, la génération de jetons JWT, la gestion des utilisateurs et des rôles.
- **Endpoints clés** :
    - `/api/auth/login` : Authentification utilisateur.
    - `/api/auth/register` : Inscription d'un nouveau tenant/admin.
    - `/api/roles` : Gestion des rôles RBAC.

## 2. Service Tenant
- **Port aval** : `5002`
- **Chemin amont** : `/gateway/tenant/{everything}`
- **Chemin aval** : `/api/tenant/{everything}`
- **Description** : Administration des entreprises clientes (tenants), gestion des abonnements et des paramètres de l'entreprise.

## 3. Service Billing (Facturation SaaS)
- **Port aval** : `5003`
- **Chemin amont** : `/gateway/billing/{everything}`
- **Chemin aval** : `/api/billing/{everything}`
- **Description** : Intégration avec Stripe, gestion des paiements d'abonnements, factures Konta et traitement des webhooks Stripe.

## 4. Service Finance (Comptabilité Générale)
- **Port aval** : `5004`
- **Chemin amont** : `/gateway/finance/{everything}`
- **Chemin aval** : `/api/finance/{everything}`
- **Description** : Cœur de la comptabilité. Gestion du plan comptable (Accounts), des journaux (Journals) et des écritures (JournalEntries).

## 5. Service Finance Core (Opérations)
- **Port aval** : `5006`
- **Chemin amont** : `/gateway/finance-core/{everything}`
- **Chemin aval** : `/api/finance-core/{everything}`
- **Description** : Gestion des tiers (clients/fournisseurs), des factures opérationnelles, de la trésorerie et des budgets.
- **Endpoints clés** :
    - `/api/business-invoices` : Factures d'achat/vente.
    - `/api/budgets` : Suivi de consommation budgétaire.

## 6. Service OCR (IA & Extraction)
- **Port aval** : `5005`
- **Chemin amont** : `/gateway/ocr/{everything}`
- **Chemin aval** : `/api/ocr/{everything}`
- **Description** : Analyse automatique de documents (factures, RIB) via intelligence artificielle.
- **Endpoints clés** :
    - `/api/extraction/upload` : Envoi d'un document pour analyse.

## 7. Service Reporting (Analytics)
- **Port aval** : `5007`
- **Chemin amont** : `/gateway/reporting/{everything}`
- **Chemin aval** : `/api/reporting/{everything}`
- **Description** : Dashboards, agrégats financiers, statistiques de performance et exports de données.
