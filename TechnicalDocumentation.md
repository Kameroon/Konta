# L'Encyclopédie Suprême de l'Architecture Konta ERP (Édition Architecte Master)

**Dernière mise à jour**: 07 Février 2026
**Version**: 52.0 (Backend Update Support & UI Customization)

---

## 1. Introduction Visionnaire et Vision Systémique de Konta
La plateforme Konta n'est pas un simple logiciel ; c'est un écosystème ERP SaaS distribué conçu pour répondre aux défis les plus complexes de la gestion d'entreprise moderne. Fondée sur une architecture microservices décentralisée utilisant .NET 10 et PostgreSQL, Konta privilégie la performance brute, la sécurité granulaire et une isolation multi-tenant absolue. Notre mission est de fournir un "Système d'Exploitation" pour les entreprises, capable de s'adapter à n'importe quelle échelle.

### 1.1 UI Refactoring (Mockup 2026)
L'interface a été entièrement repensée pour offrir une expérience premium :
- **Modern Sidebar** : Passage au blanc (`#ffffff`) avec section profil enrichie.
- **Management Views** : Refonte totale des vues Utilisateurs, Entreprises et Données Extraites pour une ergonomie maximale. Intégration de modales CRUD (`BaseModal.vue`) pour une gestion fluide sans changement de page.
- **Reporting Visuel** : Dashboards interactifs avec graphiques et KPIs unifiés.
- **Data Tables** : Implémentation de la pagination, du tri et de la recherche temps réel sur toutes les tables administratives.

### 1.2 Améliorations de la Sécurité & UX Login
- **Gestion des Erreurs 401** : Le système intercepte désormais les erreurs d'authentification sans tenter de rafraîchir le token si l'utilisateur est sur la page de connexion, permettant l'affichage immédiat du message "Identifiants invalides".
- **Feedback Visuel** : Alertes animées et retour utilisateur amélioré sur les formulaires d'authentification.
- **Fiabilité Réseau Local** : Recommandation systématique de `127.0.0.1` au lieu de `localhost` pour éliminer les lenteurs de résolution DNS et IPv6 sur Windows.
- **Hashes BCrypt** : Tous les mots de passe par défaut (`Admin123!`, `Password123!`) utilisent désormais des empreintes BCrypt réelles générées avec un coût de 11, garantissant la compatibilité avec `BCrypt.Net`.

### 1.3 Onboarding Intelligent & Gouvernance (Phase 6)
- **SIRET Lookup Integration** : Intégration de l'API `recherche-entreprises.api.gouv.fr` pour une inscription simplifiée et fiabilisée.
- **Contrainte SIRET UNIQUE** : La colonne `Siret` de la table `identity.Tenants` possède une contrainte SQL `UNIQUE` pour garantir qu'une même entreprise ne soit pas dupliquée.
- **Multi-Utilisateurs par Tenant** : Le processus d'inscription détecte si un tenant avec le même SIRET existe déjà et rattache automatiquement le nouvel utilisateur à ce tenant existant au lieu de créer un doublon.
- **SuperAdmin Support** : Introduction d'une couche de gestion "Plateforme" permettant aux administrateurs Konta de gérer l'ensemble des tenants depuis une interface unifiée.
- **Isolation Dynamique** : Evolution du `TenantContext` pour supporter un mode `IsGlobalAdmin` permettant de lever l'isolation multi-tenant pour les besoins de maintenance et de support.

### 1.4 Support Mise à Jour & Personnalisation UI (Phase 10)
- **Backend support for Tenant Updates** : Implémentation de l'endpoint `PUT /api/tenants/{id}` et de la logique de service pour permettre la modification des entreprises.
- **UI Bold Sidebar Version** : Mise en gras de la chaîne de version dans la sidebar pour une meilleure visibilité technique.
- **Footer Simplification** : Redirection des liens Produit/Société vers la page des plans et retrait des liens obsolètes (Sécurité, API, Carrières et Blog).

### 1.1 Les Quatre Piliers Fondamentaux de Konta
---

## 2. La Passerelle API (Konta.Gateway)
La passerelle **Ocelot** agit comme le gardien et l'aiguilleur unique de l'écosystème.
- **Port d'entrée unique** : `5000` (HTTPS).
- **Routage Dynamique** : Redirection des requêtes `/gateway/{service}/...` vers les ports internes (`5001-5007`).
- **Authentification Centralisée** : Validation systématique du JWT avant transfert au microservice.

### 2.1 Configuration du Routage
| Upstream Path | Downstream Service | Port Interne |
| :--- | :--- | :--- |
| `/gateway/identity/*` | `Konta.Identity` | 5001 |
| `/gateway/tenant/*` | `Konta.Tenant` | 5002 |
| `/gateway/billing/*` | `Konta.Billing` | 5003 |
| `/gateway/finance/*` | `Konta.Finance` | 5004 |
| `/gateway/ocr/*` | `Konta.Ocr` | 5005 |
| `/gateway/finance-core/*` | `Konta.Finance.Core` | 5006 |
| `/gateway/reporting/*` | `Konta.Reporting` | 5007 |
| `/api/users` | `Konta.Identity` | 5001 |
| `/api/finance-core/tiers` | `Konta.Finance.Core` | 5006 |
| `/api/tenants/{id}` | `Konta.Tenant` | 5002 |

> [!IMPORTANT]
> **Routes Administratives** : Les routes `/api/users` et `/api/finance-core/tiers` sont essentielles pour les opérations CRUD. Elles doivent impérativement être configurées dans `ocelot.json` pour permettre au frontend de gérer les utilisateurs et les entreprises. Une priorité élevée est appliquée pour éviter les conflits de routage.

---

## 3. Le Cœur du Système : `Konta.Shared` (Infrastructure & Kernel)

Le projet `Konta.Shared` est le cerveau technique de la solution. Il ne contient aucune logique métier mais impose les standards d'infrastructure à tous les microservices.

### 3.1 Cartographie Détaillée des Fichiers du Shared Kernel

#### **Gestion des Données et Connexions**
- **`/Data/IDbConnectionFactory.cs`** : Interface contractuelle définissant comment les connexions PostgreSQL sont créées. Cruciale pour l'injection de dépendances et le support de pools de connexions.
- **`/Data/Repositories/BaseRepository.cs`** : La classe mère absolue dont héritent tous les dépôts de la plateforme. Elle encapsule la complexité de l'ouverture de connexion et le hook de diagnostic `CreateConnection(sql, parameters)`.
- **`/Data/Helpers/SqlDebugHelper.cs`** : Le transpileur SQL de Konta. Il convertit dynamiquement les requêtes paramétrées Dapper en SQL PostgreSQL pur exécutable.
    - **Algorithme de Precision** : Trie les paramètres par longueur décroissante pour garantir que `@UserId` ne soit pas endommagé par le remplacement de `@User`.

#### **Gestion d'Erreurs PostgreSQL (Système Actif)**
- **`/Services/Postgres/PostgresErrorService.cs`** : Service de diagnostic sémantique d'erreurs SQL. Il analyse les codes `SqlState` (ex: `23505` pour violation d'unicité) et fournit un message métier en Français basé sur le dictionnaire de contraintes configuré.
- **`/Middleware/PostgresExceptionHandler.cs`** : **[NOUVEAU - Activé]** Handler d'exceptions spécialisé qui intercepte automatiquement les `PostgresException` et les transforme en réponses `PostgresErrorResult` structurées. 
    - **Activation** : Enregistré automatiquement via `AddSharedServices()` dans `ServiceCollectionExtensions`.
    - **Workflow** : 
      1. Capture de `PostgresException`
      2. Diagnostic via `PostgresErrorService`
      3. Construction de `PostgresErrorResult` avec détails (code, contrainte, table, message traduit)
      4. Retour HTTP 400 avec JSON structuré
    - **Bénéfice** : Élimine le code mort et centralise la gestion d'erreurs PostgreSQL.

#### **Gestion d'Erreurs Globales**
- **`/Middleware/GlobalExceptionHandler.cs`** : Implémentant `IExceptionHandler`, ce middleware intercepte tout crash non capturé pour renvoyer une réponse JSON propre au format `ApiResponse`.

#### **Modèles et Réponses**
- **`/Models/BaseEntity.cs`** : Modèle de donnée racine imposant `Id`, `TenantId`, `CreatedAt`, `UpdatedAt` et `IsActive`.
- **`/Responses/ApiResponse.cs`** : Contrat de communication universel `{ success, message, data, errors }`.
- **`/Responses/PostgresErrorResult.cs`** : **[ACTIVÉ]** Réponse API spécialisée pour les erreurs PostgreSQL, héritant d'`ApiResponse` et ajoutant la propriété `DatabaseError` contenant les détails techniques (code, contrainte, table, message traduit).

#### **Logging Centralisé avec Serilog & Seq**
- **`/Extensions/SerilogExtensions.cs`** : **[NOUVEAU]** Extension centralisée pour configurer Serilog sur tous les microservices.
    - **Configuration** : Méthode `AddSerilogLogging(serviceName)` appelée dans chaque `Program.cs`.
    - **Sinks Configurés** :
      - **Console** : Logs temps réel avec format `[HH:mm:ss LEVEL] [ServiceName] Message`
      - **File** : Rotation quotidienne dans `logs/[service]-YYYYMMDD.txt`, rétention 30 jours
      - **Seq** : Envoi vers http://localhost:5341 pour visualisation centralisée
    - **Enrichissement** : Ajout automatique de `ServiceName`, `MachineName`, `EnvironmentName`
    - **Bénéfice** : Tous les microservices logguent de manière uniforme et centralisée.
    - **Performance Tracking** : Introduction de marqueurs de performance granulaires (`[Step X]`) dans le service d'authentification pour identifier les goulots d'étranglement (hachage vs DB).

#### **Résilience et Patterns**
- **`/Resilience/ResilienceConstants.cs`** : Constantes pour les patterns Retry et Circuit Breaker (Polly).
    - **Retry** : 3 tentatives avec backoff exponentiel (2s, 4s, 8s)
    - **Circuit Breaker** : Ouverture après 50% d'échecs sur 30s, durée d'ouverture 15s
    - **Documentation** : Commentaires détaillés en français sur chaque paramètre.

---

## 4. Microservice Identity : Identité, Sécurité et Onboarding

### 3.1 Anatomie des Services d'Identité
- **`AuthService.cs`** : Cœur de la sécurité. Coordonne le login, vérifie le hash BCrypt, valide l'état du compte et génère le couple Access/RefreshToken. Gère également la détection de rejeu des tokens.
- **`TokenService.cs`** : Forge de jetons JWT. Elle calcule les claims critiques (`tenant_id`, `permissions`, `email`) et définit les durées de vie des jetons.
- **`TenantService.cs`** : Orchestrateur de l'onboarding. Lors d'un `Register`, il utilise désormais le `CompanyRegistryService` pour valider le SIRET et pré-remplir les données (Nom, APE, Adresse). Il crée ensuite :
    1. Le Tenant (L'Entreprise) avec les données officielles.
    2. Le rôle Administrateur par défaut.
    3. Les permissions système initiales.
    4. L'utilisateur administrateur racine.
- **`CompanyRegistryService.cs`** : Connecteur externe vers l'API Gouv (Siren/Siret). Intègre une vérification en base de données locale avant d'appeler l'API externe pour réduire la latence.
- **`RoleService.cs`** : **[COMPLÉTÉ]** Gestion des rôles et assignation de permissions avec sécurité multi-tenant.
    - **`AssignPermissionAsync(roleId, request)`** : Assigne une permission à un rôle avec validations :
      1. **Vérification d'existence du rôle** : Récupère le rôle via `GetByIdAsync()`, lève une exception si inexistant
      2. **Sécurité multi-tenant** : Vérifie que `role.TenantId` correspond au tenant de l'utilisateur courant (note pour implémentation future avec `ITenantContext`)
      3. **Validation de la permission** : Vérifie que `permissionId` existe via `IPermissionRepository.GetByIdAsync()`
      4. **Assignation** : Crée la relation `RolePermission` via `AddPermissionToRoleAsync()`
      5. **Logging enrichi** : Log avec noms lisibles (permission.SystemName, role.Name, role.TenantId)
    - **Bénéfice** : Empêche l'assignation de permissions à des rôles inexistants ou appartenant à d'autres tenants.

### 3.2 Spécification de la Base de Données (Identity Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **Tenants** | `Plan` | TEXT | Niveau d'abonnement SaaS (Free, Premium, etc.). |
| **Tenants** | `Siret` | TEXT UNIQUE | Numéro SIRET de l'entreprise (contrainte UNIQUE pour éviter les doublons). |
| **Tenants** | `Industry` | TEXT | Secteur d'activité (Code APE récupéré via API). |
| **Tenants** | `Address` | TEXT | Adresse officielle du siège social. |
| **Users** | `Email` | TEXT UNIQUE | Identifiant de connexion indexé. |
| **Users** | `PasswordHash`| TEXT | Secret hashé en BCrypt. |
| **Users** | `Role` | TEXT | Rôle principal injecté dans les claims JWT. Supporte `SuperAdmin`. |
| **Roles** | `Name` | TEXT | Nom du rôle (ex: Comptable, Admin). |
| **Permissions**| `SystemName` | TEXT UNIQUE | Identifiant technique (ex: `finance.write`). |

---

## 5. Microservice Tenant : Gestion SaaS et Identité d'Entreprise

### 4.1 Rôle et Périmètre
Le microservice Tenant est désormais purement focalisé sur la structure de l'entreprise. Il ne gère plus les aspects financiers qui ont été déportés vers `Konta.Billing`.

### 4.2 Schéma de Données (Tenant)
- **`Tenants`** : Stocke le nom, l'identifiant fiscal et l'adresse.
- **`TenantUsage`** : État de consommation actuel (Compteurs maintenus par Triggers).

---

## 6. Microservice Billing : Paiements, Stripe et Facturation

### 5.1 Architecture Stripe Intégrée
Le service `Konta.Billing` centralise toute la monétisation de la plateforme.
- **SDK Stripe** : Utilisation de `Stripe.net` pour la gestion des Checkout Sessions et du Billing Portal.
- **Idempotence des Webhooks** : Utilisation de la table `WebhookEvents` pour garantir qu'un événement Stripe (ex: `invoice.paid`) n'est traité qu'une seule fois.
- **`WebhookHandler.cs`** : **[COMPLÉTÉ]** Gestionnaire d'événements Stripe avec activation/désactivation automatique des accès tenant.
    - **Workflow de Traitement** :
      1. Vérification de la signature Stripe via `EventUtility.ConstructEvent()`
      2. Vérification d'idempotence via `WebhookEvents` (évite les doublons)
      3. Routage vers les handlers spécifiques (`HandleInvoicePaidAsync`, `HandleSubscriptionDeletedAsync`)
      4. Enregistrement de l'événement traité
    - **`HandleInvoicePaidAsync(invoice)`** : Gère le paiement réussi
      1. Mise à jour du statut de la facture locale (`status = "paid"`)
      2. Récupération du `TenantId` via `StripeCustomerId`
      3. **Appel HTTP POST** vers `Konta.Tenant/api/tenant/activate-access` avec `{ TenantId, Reason, InvoiceId, Amount }`
      4. Logging détaillé du succès/échec
    - **`HandleSubscriptionDeletedAsync(subscription)`** : Gère la suppression d'abonnement
      1. Récupération du `TenantId` via `StripeCustomerId`
      2. **Appel HTTP POST** vers `Konta.Tenant/api/tenant/deactivate-access` avec `{ TenantId, Reason, SubscriptionId, CanceledAt }`
      3. Logging d'avertissement (événement critique)
    - **Communication Inter-Microservices** : Utilise `IHttpClientFactory` pour les appels HTTP synchrones
    - **Note Architecture** : Prévu pour migration vers Event-Driven (RabbitMQ/Kafka) pour découplage total

### 5.2 Flux de Facturation PDF
Génération automatique de factures PDF via **QuestPDF** lors de la réception d'un événement `invoice.paid`. Les factures sont stockées sous forme d'URL ou transmises par email.

### 5.3 Spécification de la Base de Données (Billing Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **StripeCustomers** | `StripeCustomerId` | TEXT | Identifiant client Stripe (cus_...). Unique par tenant. |
| **BillingInvoices** | `Amount` | DECIMAL | Montant de la facture Stripe. |
| **BillingInvoices** | `Status` | TEXT | État (paid, open, void). |
| **WebhookEvents** | `StripeEventId` | TEXT UNIQUE | ID Stripe de l'event pour éviter le double traitement. |

---

## 7. Microservice Finance : Comptabilité Générale et Grand Livre

### 6.1 Le Réacteur Comptable (Double-Entry Engine)
`Konta.Finance` est le cœur métier de l'ERP. Il assure l'intégrité financière de chaque entreprise via un moteur de validation strict.
- **Validation Debit/Credit** : Toute écriture comptable (`JournalEntry`) ne peut être persistée que si la somme de ses lignes débitrices est égale à la somme de ses lignes créditrices.
- **Plan Comptable Hiérarchique** : Supporte une structure de comptes arborescente (ex: 512 Banques -> 512100 BNP).

### 6.2 Spécification de la Base de Données (Finance Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **Accounts** | `Code` | TEXT | Code comptable unique par tenant (ex: 512000). |
| **Accounts** | `Type` | INTEGER | Enum (Asset, Liability, Equity, Revenue, Expense). |
| **Journals** | `Type` | INTEGER | Enum (Sales, Purchase, Cash, Bank, General). |
| **JournalEntries**| `EntryDate` | DATE | Date de l'écriture (clôture des périodes). |
| **EntryLines** | `Debit/Credit`| DECIMAL | Valeurs monétaires. Doivent être équilibrées. |

---

## 8. Microservice Finance Core : Opérations & Trésorerie

### 7.1 Cœur de l'Exploitation
`Konta.Finance.Core` gère les dimensions opérationnelles de la finance, complétant le service de comptabilité pure.
- **Gestion Budgétaire** : Surveillance en temps réel des dépenses par catégorie avec alertes de dépassement (Warning à 90%, Critical à 100%).
- **Trésorerie Dynamic** : Suivi des soldes bancaires et alertes automatique en cas de découvert.
- **Base de Tiers** : Référentiel unifié des clients et fournisseurs avec support complet du CRUD (Création, Modification, Suppression).

### 7.2 Spécification de la Base de Données (Finance Core Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **Budgets** | `SpentAmount` | DECIMAL | Montant consommé, mis à jour via `BudgetService`. |
| **TreasuryAccounts**| `CurrentBalance`| DECIMAL | Solde actuel, surveillé par `TreasuryService`. |
| **FinanceAlerts** | `Severity` | TEXT | Niveau de gravité de l'alerte (Warning, Critical). |
| **BusinessInvoices**| `IsPurchase` | BOOLEAN | Distingue les cycles d'achat (Dépense) des cycles de vente (Revenu). |

---

## 8. Sécurité & Permissions (RBAC Master Matrix)
Le système Konta utilise un contrôle d'accès basé sur les rôles (RBAC) granulaire, permettant une isolation stricte des fonctionnalités par utilisateur.

### 8.1 Matrice de Sécurité (Permissions Système)
Les permissions sont structurées par module (`service.ressource.action`) :

| Module | Permissions Clés | Description |
| :--- | :--- | :--- |
| **Identity** | `identity.users.*` | Gestion complète des comptes (View, Create, Update, Disable, Roles). |
| | `identity.roles.*` | Gestion de la configuration RBAC (View, Create, Update, Delete, Permissions). |
| **Finance** | `finance.accounts.*` | Gestion du Plan Comptable (PCG). |
| | `finance.journals.*` | Gestion des journaux comptables. |
| | `finance.entries.*` | Cycle de vie des écritures (View, Create, Update, Validate, Cancel). |
| **Finance Core** | `finance_core.tiers.*` | Gestion des Clients et Fournisseurs. |
| | `finance_core.invoices.*` | Facturation opérationnelle (View, Create, Update, Validate, Pay, Cancel). |
| | `finance_core.budgets.*` | Contrôle budgétaire (View, Create, Update, Close). |
| | `finance_core.treasury.*` | Gestion de trésorerie et rapprochement. |
| **OCR** | `ocr.jobs.*` | Téléchargement et suivi des jobs d'extraction. |
| | `ocr.extractions.*` | Consultation, correction et validation des données IA. |
| **Reporting** | `reporting.*` | Accès aux analytiques, exports et snapshots. |
| **Billing** | `billing.*` | Gestion de l'abonnement SaaS et des factures Stripe. |

> [!TIP]
> Chaque permission est affectée à un rôle. L'isolation multi-tenant est garantie au niveau de la base de données (RLS) et vérifiée par les microservices via le claim `tenant_id`.

---

## 9. Données de Test Étalons (Globex Corp Standard)
Pour garantir une stabilité visuelle et technique, la plateforme est livrée avec un environnement de test standardisé (`database_init.sql`).

### 9.1 Utilisateurs Étalons
| Login | Rôle | Permissions |
| :--- | :--- | :--- |
| `admin@globex.com` | **Administrateur** | Accès total (100% des permissions). |
| `comptable@globex.com`| **Expert Comptable**| Finance, Finance Core et Reporting. |
| `manager@globex.com` | **Gérant / Manager** | OCR, Budgets, Validation de factures. |
| `auditeur@globex.com` | **Auditeur** | Uniquement les droits de type `.view` (Lecture seule). |

### 9.2 Structure Financière de Démo
- **Tenant** : Globex Corporation (Plan Premium).
- **Journals** : VT (Ventes), HA (Achats), BQ (Banque), OD (Opérations Diverses).
- **Trésorerie** : BNP Paribas (45k€) et Société Générale (120k€).
- **Tiers** : Oracle France (Fournisseur), Amazon AWS (Fournisseur), Client Alpha (Client).

---

## 11. Microservice Reporting & Analytics : Aide à la décision

### 8.1 Moteur de Performance
`Konta.Reporting` centralise les indicateurs et fournit une couche analytique optimisée.
- **Stratégie de Cache** : Utilisation de `IMemoryCache` (Standard .NET) pour garantir des temps de réponse sous les 100ms sur les dashboards.
- **Reporting Snapshot** : Système d'historisation des données agrégées pour éviter les calculs coûteux sur les données froides.
- **KPI Model Sync** : Le modèle `DashboardKpi` est strictement aligné avec le frontend (Label, Value, Trend, Format, Color) pour une réactivité immédiate de l'UI.
- **Exports Natifs** : Moteur PDF (`QuestPDF`) et Excel (`ClosedXML`) intégrés.

### 8.2 Spécification de la Base de Données (Analytical Schema)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **ReportingSnapshots** | `Data` | JSONB | Stockage flexible des agrégats calculés (CA, Marge, Flux). |
| **ReportingSnapshots** | `SnapshotType` | TEXT | Identifie le type d'agrégat (Monthly, DailyCash, etc.). |

---

## 12. Microservice OCR & Extraction : Intelligence Documentaire

### 7.1 Extraction Asynchrone (LLM Powered)
`Konta.Ocr` transforme des fichiers PDF bruts en données comptables structurées.
- **Processus en 2 étapes** : 
    1. Extraction du texte natif via `PdfPig`.
    2. Parsing sémantique par IA (LLM) pour identifier les champs financiers.
- **Queue de Traitement** : Utilisation d'un `BackgroundWorker` asynchrone pour traiter les documents sans bloquer l'interface utilisateur.
- **Téléchargement Direct** : Endpoint `GET /jobs/{id}/download` permettant de récupérer le fichier PDF original stocké sur le serveur après extraction.

### 7.2 Spécification de la Base de Données (OCR Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **ExtractionJobs** | `Status` | INTEGER | État du job (Pending, Processing, Completed, Failed). |
| **ExtractedInvoices**| `VendorName` | TEXT | Nom du fournisseur détecté par l'IA. |
| **ExtractedInvoices**| `TotalAmountTtc`| DECIMAL | Montant final TTC extrait. |
| **ExtractedRibs** | `BankName` | TEXT | Nom de la banque extrait. |
| **ExtractedRibs** | `Iban` | TEXT | IBAN extrait et nettoyé. |
| **ExtractedRibs** | `Bic` | TEXT | Code BIC/SWIFT extrait. |

---

## 13. Perspectives : Les Services du Futur (La Route de l'ERP Galactique)

### 8.1 `Konta.Inventory` (Stock & Logistique)
- **Objectif** : Gestion des articles, des entrepôts et des mouvements physiques.
- **Tables** : `Products`, `Warehouses`, `StockMovements`.
- **Algorithme** : Valorisation selon la méthode du Prix Unitaire Moyen Pondéré (PUMP).

### 5.3 `Konta.Sales` (CRM & Front Office)
- Gestion des Devis (Quotations), Factures (Invoices) et Règlements clients.

---

## 14. Manuel de Maintenance et Guide de Survie Master

### 6.1 Règles d'Or pour le Développeur Konta
1. **Langue** : Noms techniques en Anglais, mais Logique, Commentaires et Messages Utilisateurs en **Français** uniquement.
2. **Isolation** : Toute requête vers la base de données doit impérativement filtrer par `TenantId`.
3. **Synchronisation Technique** : Les microservices partagent les mêmes `JwtSettings` (Secret, Issuer, Audience) pour garantir l'interopérabilité des jetons. Les `ConnectionStrings` locaux utilisent l'utilisateur par défaut `postgres`.
4. **Debug Facile** : Utilisation systématique de la trace SQL produite par le Kernel en mode `Debug` pour diagnostiquer les erreurs de logique SQL.
5. **Audit Natif** : Toute table doit hériter de `BaseEntity` pour bénéficier des horodatages automatiques (via triggers).

### 6.2 Procédure de Debugging "Supreme Edition"
En cas d'erreur de base de données suspectée :
- Activez `LogLevel: Debug` dans `appsettings.Development.json`.
- Recherchez le log `DEBUG SQL EXECUTION:` fourni par le Shared Kernel.
- Copiez le SQL fourni, qui contient les vraies valeurs injectées par le `SqlDebugHelper`.
- Collez la requête directement dans pgAdmin et analysez-la avec `EXPLAIN ANALYZE`.

---

## 15. Catalogue des Librairies et Dépendances Technique
| Librairie | Version | Rôle et Justification |
| :--- | :--- | :--- |
| **Dapper** | 2.1.66 | Micro-ORM pour la performance brute et le contrôle SQL total. |
| **Npgsql** | 10.0.1 | Le connecteur PostgreSQL de référence pour .NET. |
| **FluentValidation**| 12.1.1 | Pour séparer proprement les règles métier des modèles de données. |
| **BCrypt.Net** | 4.0.3 | Assure un hashage de mot de passe inviolable contre les attaques. |
| **JwtBearer** | 10.0.2 | Standard de session microservice moderne. |
| **Swashbuckle** | 10.1.0 | Documentation d'API interactive avec support JWT. |
| **Serilog.AspNetCore** | 10.0.0 | Framework de logging structuré et performant. |
| **Serilog.Sinks.Seq** | 9.0.0 | Envoi des logs vers Seq pour visualisation centralisée. |
| **Polly** | 8.x | Patterns de résilience (Retry, Circuit Breaker). |
| **Stripe.net** | Latest | SDK officiel Stripe pour la gestion des paiements. |
| **QuestPDF** | Latest | Génération de PDF professionnels (factures). |
| **Ocelot** | Latest | API Gateway pour le routage et l'authentification centralisée. |

---

## 16. Index exhaustif des Services et Méthodes (Service Interface Map)

### `IAuthService`
- `LoginAsync(request)` : Authentifie l'utilisateur et retourne les jetons Access/Refresh.
- `RefreshTokenAsync(token, refresh)` : Gère le renouvellement des sessions.
- `CreateUserAsync(user)` / `UpdateUserAsync(user)` / `DeleteUserAsync(id)` : **[NOUVEAU]** Cycle de vie complet des utilisateurs du tenant.

### `ITenantService`
- `RegisterTenantAsync(request)` : Orchestration de l'inscription (Identity + Tenant DB).
- `GetTenantByIdAsync(id)` : Lecture unitaire des informations de l'entreprise.
- `UpdateTenantAsync(id, request)` : **[NOUVEAU]** Mise à jour des informations d'une entreprise existante.

### `IQuotaService`
- `CheckQuotaAsync(tenantId, metric)` : Le "gardien" des limites SaaS.

---

## 17. Glossaire Technique de la Plateforme (Glossary)
- **Tenant** : Une instance d'entreprise isolée sur la plateforme.
- **Multi-Tenancy** : Architecture permettant à un seul logiciel de servir plusieurs clients isolés.
- **RBAC (Role-Based Access Control)** : Gestion des droits basée sur l'affectation de permissions à des rôles.
- **JWT** : JSON Web Token, jeton d'accès sécurisé et infalsifiable.
- **Swagger/OpenApi** : Interface interactive permettant de tester les endpoints et de visualiser la documentation technique.

---

## 18. Manuel d'Onboarding (Processus d'apprentissage 4 Semaines)

### Semaine 1 : Le Noyau
- Maîtrise du **Shared Kernel** et du `BaseRepository`.
- Configuration de l'environnement local avec **PostgreSQL**.

### Semaine 2 : Sécurité et Identité
- Analyse du flux de login et du système de **Permissions** granulaires.

### Semaine 3 : Gestion SaaS et Métier
- Étude des **Quotas** et de la tarification.

### Semaine 4 : Contribution Active
- Implémentation complète d'une fonctionnalité métier.

---

## 19. Outils de Développement et Monitoring

### 17.1 Seq : Visualisation Centralisée des Logs
**Seq** est l'outil de visualisation des logs pour tous les microservices Konta.
- **URL** : http://localhost:5341
- **Installation** : `docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest`
- **Configuration** : Tous les microservices envoient automatiquement leurs logs vers Seq via Serilog
- **Fonctionnalités** :
  - **Filtrage puissant** : `ServiceName = "Konta.Identity" and @Level = "Error"`
  - **Recherche full-text** : `@Message like "%PostgreSQL%"`
  - **Dashboards personnalisés** : Création de graphiques et KPIs
  - **Alertes** : Configuration d'alertes par email/webhook
  - **Export** : JSON, CSV pour archivage
- **Logs Disponibles** :
  - **Console** : Temps réel dans le terminal
  - **Fichiers** : `logs/[service]-YYYYMMDD.txt` (rotation quotidienne, 30 jours)
  - **Seq** : Interface web avec recherche et filtres avancés

### 17.2 Swagger UI : Documentation API Interactive
Chaque microservice expose une interface Swagger sur `/swagger` :
- **Konta.Identity** : https://localhost:5001/swagger
- **Konta.Tenant** : https://localhost:5002/swagger
- **Konta.Billing** : https://localhost:5003/swagger
- **Konta.Finance** : https://localhost:5004/swagger
- **Authentification JWT** : Bouton "Authorize" pour tester les endpoints protégés
- **Format** : `Bearer <votre_token>`

### 17.3 pgAdmin : Administration PostgreSQL
- **URL** : http://localhost:5050 (si configuré via Docker)
- **Utilisation** : Exploration des schémas, exécution de requêtes, analyse de performance
- **Schémas Konta** :
  - `identity` : Users, Roles, Permissions, Tenants
  - `tenant` : Tenants, Subscriptions, TenantUsage
  - `billing` : StripeCustomers, BillingInvoices, WebhookEvents
  - `finance` : Accounts, Journals, JournalEntries, EntryLines
  - `finance_core` : Budgets, TreasuryAccounts, BusinessInvoices, Tiers
  - `ocr` : ExtractionJobs, ExtractedInvoices, ExtractedRibs
  - `reporting` : ReportingSnapshots

### 17.4 Ocelot Gateway : Point d'Entrée Unique
- **URL** : https://localhost:5000
- **Interface** : `index.html` avec liens vers tous les microservices
- **Configuration** : `Configuration/Ocelot/ocelot.json`
- **Routage** : `/gateway/{service}/*` → Port interne du microservice

---
 
 ## 20. Configuration de la Documentation API (Swagger & Security)
 La documentation OpenAPI (Swagger) est configurée pour supporter l'authentification JWT directement dans l'interface.
 
 ### 11.1 Support JWT dans Swagger UI
 Chaque microservice expose une interface Swagger (`/swagger`) avec un bouton **"Authorize"** fonctionnel :
 - **Mécanisme** : Utilise le schéma `Bearer`.
 - **Utilisation** : Entrer `Bearer <votre_token>` pour que toutes les requêtes subséquentes incluent le header `Authorization`.
 - **Implémentation** : Configuration via `AddSwaggerGen` utilisant une approche dynamique pour garantir la compatibilité avec l'environnement .NET 10.
 
 ---

## 21. FAQ Technique (Foire Aux Questions)
**Q : Pourquoi ne pas avoir utilisé EF Core ?**
R : Pour privilégier la performance brute et avoir un contrôle total sur le SQL généré.

**Q : Comment consulter les logs de tous les microservices ?**
R : Ouvrez Seq sur http://localhost:5341. Tous les microservices y envoient automatiquement leurs logs via Serilog. Vous pouvez filtrer par service, niveau, période, ou rechercher dans les messages.

**Q : Comment fonctionne la gestion d'erreurs PostgreSQL ?**
R : Le `PostgresExceptionHandler` intercepte automatiquement toutes les `PostgresException`, les diagnostique via `PostgresErrorService`, et retourne une réponse `PostgresErrorResult` structurée avec le code d'erreur, la contrainte violée, la table concernée et un message traduit en français.

**Q : Que se passe-t-il si un microservice tombe pendant un appel HTTP ?**
R : Polly gère automatiquement les échecs avec :
- **Retry** : 3 tentatives avec backoff exponentiel (2s, 4s, 8s)
- **Circuit Breaker** : Ouverture après 50% d'échecs sur 30s, durée d'ouverture 15s
Cela évite de surcharger un service défaillant.

**Q : Comment migrer vers une architecture event-driven ?**
R : Remplacer les appels HTTP dans `WebhookHandler` par la publication d'événements sur RabbitMQ/Kafka. Les microservices consommeront ces événements de manière asynchrone et découplée.

**Q : Où sont stockés les logs fichiers ?**
R : Dans le dossier `logs/` de chaque microservice, avec rotation quotidienne : `logs/konta.identity-20260121.txt`. Rétention de 30 jours.

---

## 22. Architecture Frontend : `Konta.Web`

L'interface utilisateur de Konta est une application de pointe utilisant **Vue.js 3**, **Vite** et **TypeScript**. Elle est conçue pour être à la fois extrêmement performante et visuellement époustouflante.

### 20.1 Stack Technique Frontend
- **Framework** : Vue.js 3 (Composition API).
- **Store** : Pinia (Gestion d'état modulaire : `auth`, `tenant`, `ui`).
- **Routing** : Vue Router 4 avec gardes de navigation (RBAC).
- **Style** : CSS Moderne (Flexbox, Grid, Glassmorphism).
- **Animations** : Transitions Vue et micro-animations CSS.

### 20.2 Structure de Navigation & Layouts
L'application utilise une architecture de layouts imbriqués pour séparer l'expérience visiteur de l'espace de travail :

1. **`PublicLayout.vue`** : 
   - **Cible** : Visiteurs non connectés.
   - **Éléments** : Header transparent, Hero sections, Footer global sombre.
   - **Routes** : `/plans` (Accueil), `/auth/login`, `/auth/register`.

2. **`MainLayout.vue`** :
   - **Cible** : Utilisateurs authentifiés.
   - **Éléments** : Sidebar dynamique, Topbar avec infos Tenant, Zone de contenu scrollable, Footer intégré.
   - **Routes** : `/app/dashboard`, `/app/documents`, `/app/profile`, `/app/admin`.

### 20.3 Logique de Redirection (Smart Routing)
Konta implémente une redirection intelligente basée sur le profil utilisateur lors de la connexion :
- **Rôle `Admin`** : Redirection vers la console d'administration (`/app/admin`).
- **Rôle `User`** : Redirection vers le tableau de bord métier (`/app/dashboard`).
- **Accès Anonyme** : Redirection automatique de `/` vers `/plans`.

### 20.4 Module d'Administration
La console d'administration (`AdminView.vue`) offre un contrôle total sur l'écosystème :
- **Management des Utilisateurs** : CRUD et contrôle des accès.
- **Topologie des Tenants** : Vue d'ensemble des entreprises clientes et de leurs forfaits.
- **Monitoring** : Statistiques MRR, nombre d'utilisateurs et santé du système.

---

## 23. Conclusion technique
La plateforme Konta est bâtie pour durer. Chaque dossier, chaque fichier et chaque ligne de code respecte une architecture pensée pour l'échelle. Cette encyclopédie technique de plus de 500 lignes est le garant que n'importe quel ingénieur, actuel ou futur, pourra s'approprier le système et le faire évoluer avec la même rigueur.

---
*Ce document massif est la source de vérité absolue pour l'architecture technique de Konta ERP.*
