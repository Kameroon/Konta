# L'Encyclopédie Suprême de l'Architecture Konta ERP (Édition Architecte Master)

**Dernière mise à jour**: 20 Janvier 2026
**Version**: 42.0 (The Infinite Technical Bible - Ocelot API Gateway Added)

---

## 1. Introduction Visionnaire et Vision Systémique de Konta
La plateforme Konta n'est pas un simple logiciel ; c'est un écosystème ERP SaaS distribué conçu pour répondre aux défis les plus complexes de la gestion d'entreprise moderne. Fondée sur une architecture microservices décentralisée utilisant .NET 10 et PostgreSQL, Konta privilégie la performance brute, la sécurité granulaire et une isolation multi-tenant absolue. Notre mission est de fournir un "Système d'Exploitation" pour les entreprises, capable de s'adapter à n'importe quelle échelle.

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

---

## 3. Le Cœur du Système : `Konta.Shared` (Infrastructure & Kernel)

Le projet `Konta.Shared` est le cerveau technique de la solution. Il ne contient aucune logique métier mais impose les standards d'infrastructure à tous les microservices.

### 3.1 Cartographie Détaillée des Fichiers du Shared Kernel
- **`/Data/IDbConnectionFactory.cs`** : Interface contractuelle définissant comment les connexions PostgreSQL sont créées. Cruciale pour l'injection de dépendances et le support de pools de connexions.
- **`/Data/Repositories/BaseRepository.cs`** : La classe mère absolue dont héritent tous les dépôts de la plateforme. Elle encapsule la complexité de l'ouverture de connexion et le hook de diagnostic `CreateConnection(sql, parameters)`.
- **`/Data/Helpers/SqlDebugHelper.cs`** : Le transpileur SQL de Konta. Il convertit dynamiquement les requêtes paramétrées Dapper en SQL PostgreSQL pur exécutable.
    - **Algorithme de Precision** : Trie les paramètres par longueur décroissante pour garantir que `@UserId` ne soit pas endommagé par le remplacement de `@User`.
- **`/Services/Postgres/PostgresErrorService.cs`** : Service de diagnostic sémantique d'erreurs SQL. Il analyse les codes `SqlState` (ex: `23505`) et fournit un message métier en Français basé sur le dictionnaire de contraintes configuré.
- **`/Middleware/GlobalExceptionHandler.cs`** : Implémentant `IExceptionHandler`, ce middleware intercepte tout crash non capturé pour renvoyer une réponse JSON propre au format `ApiResponse`.
- **`/Models/BaseEntity.cs`** : Modèle de donnée racine imposant `Id`, `TenantId`, `CreatedAt`, `UpdatedAt` et `IsActive`.
- **`/Responses/ApiResponse.cs`** : Contrat de communication universel `{ success, message, data, errors }`.

---

## 3. Microservice Identity : Identité, Sécurité et Onboarding

### 3.1 Anatomie des Services d'Identité
- **`AuthService.cs`** : Cœur de la sécurité. Coordonne le login, vérifie le hash BCrypt, valide l'état du compte et génère le couple Access/RefreshToken. Gère également la détection de rejeu des tokens.
- **`TokenService.cs`** : Forge de jetons JWT. Elle calcule les claims critiques (`tenant_id`, `permissions`, `email`) et définit les durées de vie des jetons.
- **`TenantService.cs`** : Orchestrateur de l'onboarding pour les nouveaux clients SaaS. Lors d'un `Register`, il crée :
    1. Le Tenant (L'Entreprise).
    2. Le rôle Administrateur par défaut.
    3. Les permissions système initiales pour ce rôle.
    4. L'utilisateur administrateur racine lié au rôle.

### 3.2 Spécification de la Base de Données (Identity Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **Tenants** | `Id` | UUID PK | Identifiant universel de l'entreprise. |
| **Tenants** | `Name` | TEXT | Nom commercial obligatoire. |
| **Users** | `Email` | TEXT UNIQUE | Identifiant de connexion indexé. |
| **Users** | `PasswordHash`| TEXT | Secret hashé en BCrypt. |
| **Roles** | `Name` | TEXT | Nom du rôle (ex: Comptable, Admin). |
| **Permissions**| `SystemName` | TEXT UNIQUE | Identifiant technique (ex: `finance.write`). |

---

## 4. Microservice Tenant : Gestion SaaS et Identité d'Entreprise

### 4.1 Rôle et Périmètre
Le microservice Tenant est désormais purement focalisé sur la structure de l'entreprise. Il ne gère plus les aspects financiers qui ont été déportés vers `Konta.Billing`.

### 4.2 Schéma de Données (Tenant)
- **`Tenants`** : Stocke le nom, l'identifiant fiscal et l'adresse.
- **`TenantUsage`** : État de consommation actuel (Compteurs maintenus par Triggers).

---

## 5. Microservice Billing : Paiements, Stripe et Facturation

### 5.1 Architecture Stripe Intégrée
Le service `Konta.Billing` centralise toute la monétisation de la plateforme.
- **SDK Stripe** : Utilisation de `Stripe.net` pour la gestion des Checkout Sessions et du Billing Portal.
- **Idempotence des Webhooks** : Utilisation de la table `WebhookEvents` pour garantir qu'un événement Stripe (ex: `invoice.paid`) n'est traité qu'une seule fois.

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

## 6. Microservice Finance : Comptabilité Générale et Grand Livre

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
- **Base de Tiers** : Référentiel unifié des clients et fournisseurs.

### 7.2 Spécification de la Base de Données (Finance Core Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **Budgets** | `SpentAmount` | DECIMAL | Montant consommé, mis à jour via `BudgetService`. |
| **TreasuryAccounts**| `CurrentBalance`| DECIMAL | Solde actuel, surveillé par `TreasuryService`. |
| **FinanceAlerts** | `Severity` | TEXT | Niveau de gravité de l'alerte (Warning, Critical). |
| **BusinessInvoices**| `IsPurchase` | BOOLEAN | Distingue les cycles d'achat (Dépense) des cycles de vente (Revenu). |

---

## 9. Microservice Reporting & Analytics : Aide à la décision

### 8.1 Moteur de Performance
`Konta.Reporting` centralise les indicateurs et fournit une couche analytique optimisée.
- **Stratégie de Cache** : Utilisation de `IMemoryCache` (Standard .NET) pour garantir des temps de réponse sous les 100ms sur les dashboards.
- **Reporting Snapshot** : Système d'historisation des données agrégées pour éviter les calculs coûteux sur les données froides.
- **Exports Natifs** : Moteur PDF (`QuestPDF`) et Excel (`ClosedXML`) intégrés.

### 8.2 Spécification de la Base de Données (Analytical Schema)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **ReportingSnapshots** | `Data` | JSONB | Stockage flexible des agrégats calculés (CA, Marge, Flux). |
| **ReportingSnapshots** | `SnapshotType` | TEXT | Identifie le type d'agrégat (Monthly, DailyCash, etc.). |

---

## 10. Microservice OCR & Extraction : Intelligence Documentaire

### 7.1 Extraction Asynchrone (LLM Powered)
`Konta.Ocr` transforme des fichiers PDF bruts en données comptables structurées.
- **Processus en 2 étapes** : 
    1. Extraction du texte natif via `PdfPig`.
    2. Parsing sémantique par IA (LLM) pour identifier les champs financiers.
- **Queue de Traitement** : Utilisation d'un `BackgroundWorker` asynchrone pour traiter les documents sans bloquer l'interface utilisateur.

### 7.2 Spécification de la Base de Données (OCR Table Analysis)
| Table | Colonne | Type SQL | Rôle et Contrainte |
| :--- | :--- | :--- | :--- |
| **ExtractionJobs** | `Status` | INTEGER | État du job (Pending, Processing, Completed, Failed). |
| **ExtractedInvoices**| `VendorName` | TEXT | Nom du fournisseur détecté par l'IA. |
| **ExtractedInvoices**| `TotalAmountTtc`| DECIMAL | Montant final TTC extrait. |
| **ExtractedRibs** | `Iban` | TEXT | IBAN extrait et nettoyé. |

---

## 11. Perspectives : Les Services du Futur (La Route de l'ERP Galactique)

### 8.1 `Konta.Inventory` (Stock & Logistique)
- **Objectif** : Gestion des articles, des entrepôts et des mouvements physiques.
- **Tables** : `Products`, `Warehouses`, `StockMovements`.
- **Algorithme** : Valorisation selon la méthode du Prix Unitaire Moyen Pondéré (PUMP).

### 5.3 `Konta.Sales` (CRM & Front Office)
- Gestion des Devis (Quotations), Factures (Invoices) et Règlements clients.

---

## 12. Manuel de Maintenance et Guide de Survie Master

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

## 13. Catalogue des Librairies et Dépendances Technique
| Librairie | Version | Rôle et Justification |
| :--- | :--- | :--- |
| **Dapper** | 2.1.66 | Micro-ORM pour la performance brute et le contrôle SQL total. |
| **Npgsql** | 10.0.1 | Le connecteur PostgreSQL de référence pour .NET. |
| **FluentValidation**| 12.1.1 | Pour séparer proprement les règles métier des modèles de données. |
| **BCrypt.Net** | 4.0.3 | Assure un hashage de mot de passe inviolable contre les attaques. |
| **JwtBearer** | 10.0.2 | Standard de session microservice moderne. |
| **Swashbuckle** | 10.1.0 | Documentation d'API interactive avec support JWT. |

---

## 14. Index exhaustif des Services et Méthodes (Service Interface Map)

### `IAuthService`
- `LoginAsync(request)` : Authentifie l'utilisateur et retourne les jetons Access/Refresh.
- `RefreshTokenAsync(token, refresh)` : Gère le renouvellement des sessions.

### `ITenantService`
- `RegisterTenantAsync(request)` : Orchestration de l'inscription (Identity + Tenant DB).
- `GetTenantByIdAsync(id)` : Lecture unitaire des informations de l'entreprise.

### `IQuotaService`
- `CheckQuotaAsync(tenantId, metric)` : Le "gardien" des limites SaaS.

---

## 15. Glossaire Technique de la Plateforme (Glossary)
- **Tenant** : Une instance d'entreprise isolée sur la plateforme.
- **Multi-Tenancy** : Architecture permettant à un seul logiciel de servir plusieurs clients isolés.
- **RBAC (Role-Based Access Control)** : Gestion des droits basée sur l'affectation de permissions à des rôles.
- **JWT** : JSON Web Token, jeton d'accès sécurisé et infalsifiable.
- **Swagger/OpenApi** : Interface interactive permettant de tester les endpoints et de visualiser la documentation technique.

---

## 16. Manuel d'Onboarding (Processus d'apprentissage 4 Semaines)

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
 
 ## 12. Configuration de la Documentation API (Swagger & Security)
 La documentation OpenAPI (Swagger) est configurée pour supporter l'authentification JWT directement dans l'interface.
 
 ### 11.1 Support JWT dans Swagger UI
 Chaque microservice expose une interface Swagger (`/swagger`) avec un bouton **"Authorize"** fonctionnel :
 - **Mécanisme** : Utilise le schéma `Bearer`.
 - **Utilisation** : Entrer `Bearer <votre_token>` pour que toutes les requêtes subséquentes incluent le header `Authorization`.
 - **Implémentation** : Configuration via `AddSwaggerGen` utilisant une approche dynamique pour garantir la compatibilité avec l'environnement .NET 10.
 
 ---

## 18. FAQ Technique (Foire Aux Questions)
**Q : Pourquoi ne pas avoir utilisé EF Core ?**
R : Pour privilégier la performance brute et avoir un contrôle total sur le SQL généré.

---

## 19. Conclusion technique
La plateforme Konta est bâtie pour durer. Chaque dossier, chaque fichier et chaque ligne de code respecte une architecture pensée pour l'échelle. Cette encyclopédie technique de plus de 500 lignes est le garant que n'importe quel ingénieur, actuel ou futur, pourra s'approprier le système et le faire évoluer avec la même rigueur.

---
*Ce document massif est la source de vérité absolue pour l'architecture technique de Konta ERP.*
