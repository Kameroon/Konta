# Documentation des Routes de l'API Gateway (Konta)

Ce document dÃ©taille la configuration du point d'entrÃ©e unique de la plateforme Konta (API Gateway) dÃ©finie dans `ocelot.json`.

## Configuration Globale
- **Base URL** : `https://127.0.0.1:5000`
- **MÃ©thodes autorisÃ©es** : GET, POST, PUT, DELETE, PATCH
- **SÃ©curitÃ©** : JWT Bearer Token requis sur toutes les routes.

---

## 1. Service d'IdentitÃ© (Identity)
- **Port aval** : `5001`
- **Chemin amont** : `/gateway/identity/{everything}`
- **Chemin aval** : `/api/identity/{everything}`
- **Description** : GÃ¨re l'authentification, la gÃ©nÃ©ration de jetons JWT, la gestion des utilisateurs et des rÃ´les.
- **Endpoints clÃ©s** :
    - `/api/auth/login` : Authentification utilisateur.
    - `/api/auth/register` : Inscription d'un nouveau tenant/admin (support SIRET).
    - `/api/tenants/lookup/{siret}` : Recherche publique d'entreprise par SIRET (API Gouv).
    - `/api/roles` : Gestion des rÃ´les RBAC.
    - `/api/users` : Liste des utilisateurs (Vue globale pour SuperAdmins).

## 2. Service Tenant
- **Port aval** : `5002`
- **Chemin amont** : `/gateway/tenant/{everything}`
- **Chemin aval** : `/api/tenant/{everything}`
- **Description** : Administration des entreprises clientes (tenants), gestion des abonnements et des paramÃ¨tres de l'entreprise.

## 3. Service Billing (Facturation SaaS)
- **Port aval** : `5003`
- **Chemin amont** : `/gateway/billing/{everything}`
- **Chemin aval** : `/api/billing/{everything}`
- **Description** : IntÃ©gration avec Stripe, gestion des paiements d'abonnements, factures Konta et traitement des webhooks Stripe.

## 4. Service Finance (ComptabilitÃ© GÃ©nÃ©rale)
- **Port aval** : `5004`
- **Chemin amont** : `/gateway/finance/{everything}`
- **Chemin aval** : `/api/finance/{everything}`
- **Description** : CÅ“ur de la comptabilitÃ©. Gestion du plan comptable (Accounts), des journaux (Journals) et des Ã©critures (JournalEntries).

## 5. Service Finance Core (OpÃ©rations)
- **Port aval** : `5006`
- **Chemin amont** : `/gateway/finance-core/{everything}`
- **Chemin aval** : `/api/finance-core/{everything}`
- **Description** : Gestion des tiers (clients/fournisseurs), des factures opÃ©rationnelles, de la trÃ©sorerie et des budgets.
- **Endpoints clÃ©s** :
    - `/api/business-invoices` : Factures d'achat/vente.
    - `/api/budgets` : Suivi de consommation budgÃ©taire.
    - `/api/finance-core/tiers` : Gestion des clients et fournisseurs.

## 6. Service OCR (IA & Extraction)
- **Port aval** : `5005`
- **Chemin amont** : `/gateway/ocr/{everything}`
- **Chemin aval** : `/api/ocr/{everything}`
- **Description** : Analyse automatique de documents (factures, RIB) via intelligence artificielle.
- **Endpoints clÃ©s** :
    - `/api/ocr/upload` : Envoi d'un document pour analyse.
    - `/api/ocr/jobs` : Historique global des jobs du tenant.
    - `/api/ocr/jobs/mine` : Historique des jobs de l'utilisateur connecté.

## 7. Service Reporting (Analytics)
- **Port aval** : `5007`
- **Chemin amont** : `/gateway/reporting/{everything}`
- **Chemin aval** : `/api/reporting/{everything}`
- **Description** : Dashboards, agrÃ©gats financiers, statistiques de performance et exports de donnÃ©es.
