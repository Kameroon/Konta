-- ==========================================================
-- SCRIPT D'INITIALISATION DE LA BASE DE DONNÉES KONTA ERP
-- Description: Schéma consolidé pour tous les microservices
-- Date: 30 Janvier 2026
-- Version: 51.0 (SIRET Uniqueness & Multi-User Registration)
-- Notes:
--   - La colonne Siret possède une contrainte UNIQUE
--   - Le processus d'inscription permet plusieurs utilisateurs par tenant
-- ==========================================================

-- ----------------------------------------------------------
-- 1. SCHEMA IDENTITY & AUTHENTICATION
-- ----------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS identity;

CREATE TABLE IF NOT EXISTS identity.Tenants (
    Id UUID PRIMARY KEY,
    Name TEXT NOT NULL,
    Identifier TEXT,
    Industry TEXT,
    Address TEXT,
    Siret TEXT UNIQUE,
    Plan TEXT NOT NULL DEFAULT 'Free',
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE identity.Tenants IS 'Stocke les entreprises (tenants) utilisant la plateforme';
COMMENT ON COLUMN identity.Tenants.Id IS 'Identifiant unique de l''entreprise (UUID)';
COMMENT ON COLUMN identity.Tenants.Name IS 'Nom commercial officiel de l''entreprise';
COMMENT ON COLUMN identity.Tenants.Siret IS 'Numéro SIRET unique de l''entreprise (contrainte UNIQUE pour éviter les doublons)';
COMMENT ON COLUMN identity.Tenants.Plan IS 'Niveau d''abonnement SaaS (Free, Premium, Enterprise)';
COMMENT ON COLUMN identity.Tenants.CreatedAt IS 'Horodatage de création du tenant';
COMMENT ON COLUMN identity.Tenants.UpdatedAt IS 'Horodatage de la dernière modification du tenant';
COMMENT ON COLUMN identity.Tenants.IsActive IS 'Drapeau d''activation pour suppression logique';

CREATE TABLE IF NOT EXISTS identity.Users (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL REFERENCES identity.Tenants(Id),
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'User',
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE identity.Users IS 'Référentiel des utilisateurs authentifiés';
COMMENT ON COLUMN identity.Users.Id IS 'Identifiant unique de l''utilisateur';
COMMENT ON COLUMN identity.Users.TenantId IS 'Lien vers le tenant parent (isolation multi-tenant)';
COMMENT ON COLUMN identity.Users.Email IS 'Adresse email servant d''identifiant unique de connexion';
COMMENT ON COLUMN identity.Users.PasswordHash IS 'Empreinte du mot de passe sécurisée via algorithme BCrypt';
COMMENT ON COLUMN identity.Users.FirstName IS 'Prénom de l''utilisateur';
COMMENT ON COLUMN identity.Users.LastName IS 'Nom de famille de l''utilisateur';
COMMENT ON COLUMN identity.Users.Role IS 'Rôle fonctionnel (Admin, User, Accountant, Auditor)';
COMMENT ON COLUMN identity.Users.CreatedAt IS 'Date d''inscription ou création de l''utilisateur';
COMMENT ON COLUMN identity.Users.UpdatedAt IS 'Date de dernière mise à jour du profil';
COMMENT ON COLUMN identity.Users.IsActive IS 'État du compte utilisateur (Actif/Inactif)';

CREATE TABLE IF NOT EXISTS identity.Roles (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL REFERENCES identity.Tenants(Id),
    Name TEXT NOT NULL,
    Description TEXT,
    IsDefault BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE identity.Roles IS 'Définition des rôles de sécurité personnalisés par tenant';
COMMENT ON COLUMN identity.Roles.Id IS 'Identifiant unique du rôle';
COMMENT ON COLUMN identity.Roles.TenantId IS 'Tenant propriétaire du rôle';
COMMENT ON COLUMN identity.Roles.Name IS 'Nom du rôle (ex: Responsable Achat)';
COMMENT ON COLUMN identity.Roles.Description IS 'Description détaillée des droits du rôle';
COMMENT ON COLUMN identity.Roles.IsDefault IS 'Indique si ce rôle est affecté automatiquement aux nouveaux utilisateurs';
COMMENT ON COLUMN identity.Roles.CreatedAt IS 'Date de création du rôle';
COMMENT ON COLUMN identity.Roles.UpdatedAt IS 'Date de modification du rôle';
COMMENT ON COLUMN identity.Roles.IsActive IS 'Disponibilité du rôle';

CREATE TABLE IF NOT EXISTS identity.Permissions (
    Id UUID PRIMARY KEY,
    SystemName TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Description TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE identity.Permissions IS 'Liste exhaustive des permissions système granulaires';
COMMENT ON COLUMN identity.Permissions.Id IS 'Identifiant unique de la permission';
COMMENT ON COLUMN identity.Permissions.SystemName IS 'Code technique unique (ex: finance.entry.create)';
COMMENT ON COLUMN identity.Permissions.Name IS 'Libellé convivial de la permission';
COMMENT ON COLUMN identity.Permissions.Description IS 'Explication de ce que permet cette permission';
COMMENT ON COLUMN identity.Permissions.CreatedAt IS 'Date d''ajout de la permission au système';
COMMENT ON COLUMN identity.Permissions.UpdatedAt IS 'Dernière mise à jour technique';
COMMENT ON COLUMN identity.Permissions.IsActive IS 'Disponibilité globale de la permission';

CREATE TABLE IF NOT EXISTS identity.RolePermissions (
    RoleId UUID NOT NULL REFERENCES identity.Roles(Id),
    PermissionId UUID NOT NULL REFERENCES identity.Permissions(Id),
    PRIMARY KEY (RoleId, PermissionId)
);

COMMENT ON TABLE identity.RolePermissions IS 'Association n-n entre les rôles et les permissions';
COMMENT ON COLUMN identity.RolePermissions.RoleId IS 'Référence au rôle';
COMMENT ON COLUMN identity.RolePermissions.PermissionId IS 'Référence à la permission associée';

CREATE TABLE IF NOT EXISTS identity.UserRoles (
    UserId UUID NOT NULL REFERENCES identity.Users(Id),
    RoleId UUID NOT NULL REFERENCES identity.Roles(Id),
    PRIMARY KEY (UserId, RoleId)
);

COMMENT ON TABLE identity.UserRoles IS 'Association n-n entre les utilisateurs et les rôles';
COMMENT ON COLUMN identity.UserRoles.UserId IS 'Référence à l''utilisateur';
COMMENT ON COLUMN identity.UserRoles.RoleId IS 'Référence au rôle';

CREATE TABLE IF NOT EXISTS identity.RefreshTokens (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES identity.Users(Id),
    Token TEXT NOT NULL,
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsRevoked BOOLEAN NOT NULL DEFAULT FALSE,
    ReplacedByToken TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE identity.RefreshTokens IS 'Gestion du cycle de vie des jetons de rafraîchissement JWT';
COMMENT ON COLUMN identity.RefreshTokens.Id IS 'ID unique du jeton';
COMMENT ON COLUMN identity.RefreshTokens.UserId IS 'Porteur du jeton';
COMMENT ON COLUMN identity.RefreshTokens.Token IS 'Valeur cryptographique du Refresh Token';
COMMENT ON COLUMN identity.RefreshTokens.ExpiresAt IS 'Date d''expiration du jeton (fin de session)';
COMMENT ON COLUMN identity.RefreshTokens.IsRevoked IS 'Indique si le jeton a été invalidé manuellement';
COMMENT ON COLUMN identity.RefreshTokens.ReplacedByToken IS 'ID du nouveau jeton généré après rotation';
COMMENT ON COLUMN identity.RefreshTokens.CreatedAt IS 'Date de génération du jeton';
COMMENT ON COLUMN identity.RefreshTokens.UpdatedAt IS 'Date de modification (ex: révocation)';
COMMENT ON COLUMN identity.RefreshTokens.IsActive IS 'Validité technique du jeton';

-- ----------------------------------------------------------
-- 2. SCHEMA BILLING (Paiements & Stripe)
-- ----------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS billing;

CREATE TABLE IF NOT EXISTS billing.StripeCustomers (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    StripeCustomerId TEXT NOT NULL UNIQUE,
    Email TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE billing.StripeCustomers IS 'Cartographie entre Konta et les identifiants clients Stripe';
COMMENT ON COLUMN billing.StripeCustomers.Id IS 'ID technique interne';
COMMENT ON COLUMN billing.StripeCustomers.TenantId IS 'Tenant Konta associé';
COMMENT ON COLUMN billing.StripeCustomers.StripeCustomerId IS 'Identifiant client chez Stripe (cus_...)';
COMMENT ON COLUMN billing.StripeCustomers.CreatedAt IS 'Date de liaison initiale';
COMMENT ON COLUMN billing.StripeCustomers.UpdatedAt IS 'Mise à jour de la liaison';
COMMENT ON COLUMN billing.StripeCustomers.IsActive IS 'État de la liaison Billing';

CREATE TABLE IF NOT EXISTS billing.BillingInvoices (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    StripeInvoiceId TEXT NOT NULL,
    InvoiceNumber TEXT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    Status TEXT NOT NULL DEFAULT 'open',
    PdfUrl TEXT,
    PaidAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE billing.BillingInvoices IS 'Suivi des factures d''utilisation SaaS émises via Stripe';
COMMENT ON COLUMN billing.BillingInvoices.Id IS 'ID de transaction interne';
COMMENT ON COLUMN billing.BillingInvoices.TenantId IS 'Bénéficiaire de l''abonnement';
COMMENT ON COLUMN billing.BillingInvoices.StripeInvoiceId IS 'ID facture Stripe (in_...)';
COMMENT ON COLUMN billing.BillingInvoices.InvoiceNumber IS 'Numéro de facture lisible (ex: INV-2026-X)';
COMMENT ON COLUMN billing.BillingInvoices.Amount IS 'Montant TTC de la facture';
COMMENT ON COLUMN billing.BillingInvoices.Currency IS 'Devise du paiement';
COMMENT ON COLUMN billing.BillingInvoices.Status IS 'Statut (open, paid, void, uncollectible)';
COMMENT ON COLUMN billing.BillingInvoices.PdfUrl IS 'Lien vers le document PDF original';
COMMENT ON COLUMN billing.BillingInvoices.PaidAt IS 'Horodatage effectif du paiement';
COMMENT ON COLUMN billing.BillingInvoices.CreatedAt IS 'Date de constatation de la facture';
COMMENT ON COLUMN billing.BillingInvoices.UpdatedAt IS 'Dernière synchronisation Stripe';
COMMENT ON COLUMN billing.BillingInvoices.IsActive IS 'Visibilité de la facture dans l''historique';

CREATE TABLE IF NOT EXISTS billing.WebhookEvents (
    Id UUID PRIMARY KEY,
    StripeEventId TEXT NOT NULL UNIQUE,
    EventType TEXT NOT NULL,
    Data JSONB NOT NULL,
    ProcessedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE billing.WebhookEvents IS 'Journalisation et vérification d''idempotence des webhooks Stripe';
COMMENT ON COLUMN billing.WebhookEvents.Id IS 'ID technique du log';
COMMENT ON COLUMN billing.WebhookEvents.StripeEventId IS 'ID natif de l''event chez Stripe (evt_...)';
COMMENT ON COLUMN billing.WebhookEvents.EventType IS 'Type d''événement (ex: invoice.paid)';
COMMENT ON COLUMN billing.WebhookEvents.Data IS 'Corps complet de l''événement au format JSON';
COMMENT ON COLUMN billing.WebhookEvents.ProcessedAt IS 'Date de prise en compte par le système';

CREATE TABLE IF NOT EXISTS billing.SubscriptionPlans (
    Id UUID PRIMARY KEY,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    Interval TEXT NOT NULL DEFAULT 'month',
    MaxUsers INTEGER,
    StorageGb INTEGER,
    HasPrioritySupport BOOLEAN NOT NULL DEFAULT FALSE,
    HasApiAccess BOOLEAN NOT NULL DEFAULT FALSE,
    Modules JSONB, -- Liste des modules inclus
    Features JSONB, -- Liste des caractéristiques (points à puces)
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE billing.SubscriptionPlans IS 'Catalogue des offres d''abonnement SaaS';

-- ----------------------------------------------------------
-- 3. SCHEMA FINANCE (Comptabilité Générale)
-- ----------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS finance;

CREATE TABLE IF NOT EXISTS finance.Accounts (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL, -- 1:Asset, 2:Liability, 3:Equity, 4:Revenue, 5:Expense
    ParentId UUID REFERENCES finance.Accounts(Id),
    FullPath TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance.Accounts IS 'Plan comptable structuré par tenant';
COMMENT ON COLUMN finance.Accounts.Id IS 'ID unique du compte';
COMMENT ON COLUMN finance.Accounts.TenantId IS 'Propriétaire du plan comptable';
COMMENT ON COLUMN finance.Accounts.Code IS 'Numéro de compte (ex: 512, 401000)';
COMMENT ON COLUMN finance.Accounts.Name IS 'Nom du compte (ex: Banque BNP)';
COMMENT ON COLUMN finance.Accounts.Type IS 'Nature (1:Actif, 2:Passif, 3:Capitaux, 4:Produits, 5:Charges)';
COMMENT ON COLUMN finance.Accounts.ParentId IS 'Compte parent (gestion hiérarchique)';
COMMENT ON COLUMN finance.Accounts.FullPath IS 'Chemin hiérarchique complet pour tri et recherche';
COMMENT ON COLUMN finance.Accounts.CreatedAt IS 'Date de création du compte';
COMMENT ON COLUMN finance.Accounts.UpdatedAt IS 'Dernière modification';
COMMENT ON COLUMN finance.Accounts.IsActive IS 'Disponibilité pour saisie';

CREATE TABLE IF NOT EXISTS finance.Journals (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL, -- 1:General, 2:Sales, 3:Purchase, 4:Cash, 5:Bank
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance.Journals IS 'Définition des journaux d''écritures comptables';
COMMENT ON COLUMN finance.Journals.Id IS 'ID unique du journal';
COMMENT ON COLUMN finance.Journals.TenantId IS 'Tenant propriétaire';
COMMENT ON COLUMN finance.Journals.Code IS 'Identifiant court (ex: VT, HA, OD)';
COMMENT ON COLUMN finance.Journals.Name IS 'Libellé complet (ex: Journal des Ventes)';
COMMENT ON COLUMN finance.Journals.Type IS 'Catégorie de journal pour règles de saisie';
COMMENT ON COLUMN finance.Journals.CreatedAt IS 'Date de création';
COMMENT ON COLUMN finance.Journals.UpdatedAt IS 'Dernière mise à jour';
COMMENT ON COLUMN finance.Journals.IsActive IS 'Activation du journal';

CREATE TABLE IF NOT EXISTS finance.JournalEntries (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    JournalId UUID NOT NULL REFERENCES finance.Journals(Id),
    EntryDate DATE NOT NULL,
    Reference TEXT NOT NULL,
    Description TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance.JournalEntries IS 'Entêtes des pièces ou écritures comptables';
COMMENT ON COLUMN finance.JournalEntries.Id IS 'ID unique de la pièce';
COMMENT ON COLUMN finance.JournalEntries.TenantId IS 'Propriétaire de l''écriture';
COMMENT ON COLUMN finance.JournalEntries.JournalId IS 'Journal dans lequel figure l''écriture';
COMMENT ON COLUMN finance.JournalEntries.EntryDate IS 'Date de l''opération comptable';
COMMENT ON COLUMN finance.JournalEntries.Reference IS 'Numéro de pièce original (ex: FACT-123)';
COMMENT ON COLUMN finance.JournalEntries.Description IS 'Libellé de l''écriture';
COMMENT ON COLUMN finance.JournalEntries.CreatedAt IS 'Horodatage système de création';
COMMENT ON COLUMN finance.JournalEntries.UpdatedAt IS 'Dernière modification de l''entête';
COMMENT ON COLUMN finance.JournalEntries.IsActive IS 'Validité de l''écriture (non-annulée)';

CREATE TABLE IF NOT EXISTS finance.EntryLines (
    Id UUID PRIMARY KEY,
    EntryId UUID NOT NULL REFERENCES finance.JournalEntries(Id),
    AccountId UUID NOT NULL REFERENCES finance.Accounts(Id),
    Label TEXT NOT NULL,
    Debit DECIMAL(18,2) NOT NULL DEFAULT 0,
    Credit DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance.EntryLines IS 'Détails au débit ou au crédit d''une écriture comptable';
COMMENT ON COLUMN finance.EntryLines.Id IS 'ID de ligne unique';
COMMENT ON COLUMN finance.EntryLines.EntryId IS 'Lien vers l''entête de l''écriture';
COMMENT ON COLUMN finance.EntryLines.AccountId IS 'Compte mouvementé';
COMMENT ON COLUMN finance.EntryLines.Label IS 'Libellé spécifique de la ligne';
COMMENT ON COLUMN finance.EntryLines.Debit IS 'Montant porté au débit';
COMMENT ON COLUMN finance.EntryLines.Credit IS 'Montant porté au crédit';
COMMENT ON COLUMN finance.EntryLines.CreatedAt IS 'Date de saisie de la ligne';
COMMENT ON COLUMN finance.EntryLines.UpdatedAt IS 'Dernière correction';
COMMENT ON COLUMN finance.EntryLines.IsActive IS 'Visibilité de la ligne';

-- ----------------------------------------------------------
-- 4. SCHEMA FINANCE CORE (Opérations & Trésorerie)
-- ----------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS finance_core;

CREATE TABLE IF NOT EXISTS finance_core.Tiers (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL, -- 1:Client, 2:Supplier
    Email TEXT,
    TaxId TEXT,
    Address TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance_core.Tiers IS 'Référentiel des tiers (Clients et Fournisseurs)';
COMMENT ON COLUMN finance_core.Tiers.Id IS 'ID unique du tiers';
COMMENT ON COLUMN finance_core.Tiers.TenantId IS 'Propriétaire du tiers';
COMMENT ON COLUMN finance_core.Tiers.Name IS 'Raison sociale ou nom';
COMMENT ON COLUMN finance_core.Tiers.Type IS 'Nature (1:Client, 2:Fournisseur)';
COMMENT ON COLUMN finance_core.Tiers.Email IS 'Email de contact principal';
COMMENT ON COLUMN finance_core.Tiers.TaxId IS 'Identifiant fiscal unique (ex: SIRET, VAT)';
COMMENT ON COLUMN finance_core.Tiers.Address IS 'Adresse physique complète';
COMMENT ON COLUMN finance_core.Tiers.CreatedAt IS 'Date de création';
COMMENT ON COLUMN finance_core.Tiers.UpdatedAt IS 'Dernière modification';
COMMENT ON COLUMN finance_core.Tiers.IsActive IS 'Possibilité de transaction avec le tiers';

CREATE TABLE IF NOT EXISTS finance_core.BusinessInvoices (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    TierId UUID NOT NULL REFERENCES finance_core.Tiers(Id),
    Reference TEXT NOT NULL,
    AmountHt DECIMAL(18,2) NOT NULL,
    AmountTtc DECIMAL(18,2) NOT NULL,
    VatAmount DECIMAL(18,2) NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0, -- 0:Draft, 1:Validated, 2:Paid, 3:Canceled
    IsPurchase BOOLEAN NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance_core.BusinessInvoices IS 'Gestion opérationnelle des factures fournisseurs et clients';
COMMENT ON COLUMN finance_core.BusinessInvoices.Id IS 'ID unique facture';
COMMENT ON COLUMN finance_core.BusinessInvoices.TenantId IS 'Propriétaire de la facture';
COMMENT ON COLUMN finance_core.BusinessInvoices.TierId IS 'Tiers concerné (Client/Fournisseur)';
COMMENT ON COLUMN finance_core.BusinessInvoices.Reference IS 'Numéro de référence facture';
COMMENT ON COLUMN finance_core.BusinessInvoices.AmountHt IS 'Montant Hors Taxes';
COMMENT ON COLUMN finance_core.BusinessInvoices.AmountTtc IS 'Montant Toutes Taxes Comprises';
COMMENT ON COLUMN finance_core.BusinessInvoices.VatAmount IS 'Montant de la TVA calcule';
COMMENT ON COLUMN finance_core.BusinessInvoices.InvoiceDate IS 'Date d''émission de la facture';
COMMENT ON COLUMN finance_core.BusinessInvoices.DueDate IS 'Date d''échéance de paiement';
COMMENT ON COLUMN finance_core.BusinessInvoices.Status IS 'Cycle de vie (0:Draft, 1:Validated, 2:Paid, 3:Canceled)';
COMMENT ON COLUMN finance_core.BusinessInvoices.IsPurchase IS 'Nature (True: Achat, False: Vente)';
COMMENT ON COLUMN finance_core.BusinessInvoices.CreatedAt IS 'Date d''enregistrement';
COMMENT ON COLUMN finance_core.BusinessInvoices.UpdatedAt IS 'Dernière mise à jour';
COMMENT ON COLUMN finance_core.BusinessInvoices.IsActive IS 'Visibilité facture';

CREATE TABLE IF NOT EXISTS finance_core.Budgets (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Category TEXT NOT NULL,
    AllocatedAmount DECIMAL(18,2) NOT NULL,
    SpentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    AlertThresholdPercentage DECIMAL(5,2) DEFAULT 90,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance_core.Budgets IS 'Planification et suivi budgétaire';
COMMENT ON COLUMN finance_core.Budgets.Id IS 'ID unique budget';
COMMENT ON COLUMN finance_core.Budgets.TenantId IS 'Propriétaire du budget';
COMMENT ON COLUMN finance_core.Budgets.Category IS 'Catégorie analytique (ex: Marketing)';
COMMENT ON COLUMN finance_core.Budgets.AllocatedAmount IS 'Enveloppe financière allouée';
COMMENT ON COLUMN finance_core.Budgets.SpentAmount IS 'Consommation réelle actuelle';
COMMENT ON COLUMN finance_core.Budgets.StartDate IS 'Début de période budgétaire';
COMMENT ON COLUMN finance_core.Budgets.EndDate IS 'Fin de période budgétaire';
COMMENT ON COLUMN finance_core.Budgets.AlertThresholdPercentage IS 'Seuil de déclenchement d''alerte (défaut 90%)';
COMMENT ON COLUMN finance_core.Budgets.CreatedAt IS 'Date de création';
COMMENT ON COLUMN finance_core.Budgets.UpdatedAt IS 'Dernière réévaluation';
COMMENT ON COLUMN finance_core.Budgets.IsActive IS 'Budget en cours';

CREATE TABLE IF NOT EXISTS finance_core.TreasuryAccounts (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Name TEXT NOT NULL,
    AccountNumber TEXT,
    CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance_core.TreasuryAccounts IS 'Gestion des liquidités et des comptes bancaires';
COMMENT ON COLUMN finance_core.TreasuryAccounts.Id IS 'ID compte trésorerie';
COMMENT ON COLUMN finance_core.TreasuryAccounts.TenantId IS 'Société détentrice';
COMMENT ON COLUMN finance_core.TreasuryAccounts.Name IS 'Nom (ex: Compte Courant BNP)';
COMMENT ON COLUMN finance_core.TreasuryAccounts.AccountNumber IS 'IBAN ou numéro de compte';
COMMENT ON COLUMN finance_core.TreasuryAccounts.CurrentBalance IS 'Solde actuel en instantané';
COMMENT ON COLUMN finance_core.TreasuryAccounts.Currency IS 'Devise du compte';
COMMENT ON COLUMN finance_core.TreasuryAccounts.CreatedAt IS 'Date d''ouverture logicielle';
COMMENT ON COLUMN finance_core.TreasuryAccounts.UpdatedAt IS 'Dernier mouvement ou solde synchronisé';
COMMENT ON COLUMN finance_core.TreasuryAccounts.IsActive IS 'Compte utilisable pour paiements';

CREATE TABLE IF NOT EXISTS finance_core.FinanceAlerts (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Title TEXT NOT NULL,
    Message TEXT NOT NULL,
    Severity TEXT NOT NULL DEFAULT 'Warning',
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    RelatedEntityId UUID,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE finance_core.FinanceAlerts IS 'Logs d''alertes de gestion financières générées par le système';
COMMENT ON COLUMN finance_core.FinanceAlerts.Id IS 'ID alerte';
COMMENT ON COLUMN finance_core.FinanceAlerts.TenantId IS 'Tenant destinataire';
COMMENT ON COLUMN finance_core.FinanceAlerts.Title IS 'Titre de l''alerte (ex: Budget Dépasse)';
COMMENT ON COLUMN finance_core.FinanceAlerts.Message IS 'Contenu détaillé de l''alerte';
COMMENT ON COLUMN finance_core.FinanceAlerts.Severity IS 'Urgence (Warning, Critical)';
COMMENT ON COLUMN finance_core.FinanceAlerts.IsRead IS 'Indique si l''utilisateur a pris connaissance de l''alerte';
COMMENT ON COLUMN finance_core.FinanceAlerts.RelatedEntityId IS 'ID de l''objet concerné (Facture, Budget, Compte)';
COMMENT ON COLUMN finance_core.FinanceAlerts.CreatedAt IS 'Instant du trigger de l''alerte';
COMMENT ON COLUMN finance_core.FinanceAlerts.UpdatedAt IS 'Date de lecture ou acquittement';
COMMENT ON COLUMN finance_core.FinanceAlerts.IsActive IS 'Alerte toujours d''actualité';

-- ----------------------------------------------------------
-- 5. SCHEMA OCR (Extraction Documentaire)
-- ----------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS ocr;

CREATE TABLE IF NOT EXISTS ocr.ExtractionJobs (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    CreatedBy UUID, -- L'utilisateur qui a téléchargé le document
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0, -- 0:Pending, 1:Processing, 2:Completed, 3:Failed
    DetectedType INTEGER NOT NULL DEFAULT 0,
    ErrorMessage TEXT,
    ProcessedAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE ocr.ExtractionJobs IS 'File d''attente et suivi des traitements de reconnaissance de documents';
COMMENT ON COLUMN ocr.ExtractionJobs.Id IS 'ID job OCR';
COMMENT ON COLUMN ocr.ExtractionJobs.TenantId IS 'Propriétaire du document';
COMMENT ON COLUMN ocr.ExtractionJobs.CreatedBy IS 'Identifiant de l''utilisateur ayant soumis le document';
COMMENT ON COLUMN ocr.ExtractionJobs.FileName IS 'Nom du fichier original';
COMMENT ON COLUMN ocr.ExtractionJobs.FilePath IS 'Chemin de stockage interne du document';
COMMENT ON COLUMN ocr.ExtractionJobs.Status IS 'État (0:Pending, 1:Processing, 2:Completed, 3:Failed)';
COMMENT ON COLUMN ocr.ExtractionJobs.DetectedType IS 'Type identifié par l''IA (Facture, RIB, etc.)';
COMMENT ON COLUMN ocr.ExtractionJobs.ErrorMessage IS 'Détail en cas d''échec d''extraction';
COMMENT ON COLUMN ocr.ExtractionJobs.ProcessedAt IS 'Heure de fin de traitement IA';
COMMENT ON COLUMN ocr.ExtractionJobs.CreatedAt IS 'Date de soumission du document';
COMMENT ON COLUMN ocr.ExtractionJobs.UpdatedAt IS 'Dernière mise à jour du statut';
COMMENT ON COLUMN ocr.ExtractionJobs.IsActive IS 'Indique si le job est archivé ou non';

CREATE TABLE IF NOT EXISTS ocr.ExtractedInvoices (
    Id UUID PRIMARY KEY,
    JobId UUID NOT NULL REFERENCES ocr.ExtractionJobs(Id),
    VendorName TEXT,
    InvoiceNumber TEXT,
    InvoiceDate DATE,
    TotalAmountHt DECIMAL(18,2),
    TotalAmountTtc DECIMAL(18,2),
    VatAmount DECIMAL(18,2),
    Currency TEXT,
    RawJson JSONB,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE ocr.ExtractedInvoices IS 'Données financières structurées issues de l''analyse IA des factures';
COMMENT ON COLUMN ocr.ExtractedInvoices.Id IS 'ID extraction';
COMMENT ON COLUMN ocr.ExtractedInvoices.JobId IS 'Lien vers le job parent';
COMMENT ON COLUMN ocr.ExtractedInvoices.VendorName IS 'Nom du fournisseur détecté';
COMMENT ON COLUMN ocr.ExtractedInvoices.InvoiceNumber IS 'Numéro de facture lu';
COMMENT ON COLUMN ocr.ExtractedInvoices.InvoiceDate IS 'Date de facture lue';
COMMENT ON COLUMN ocr.ExtractedInvoices.TotalAmountHt IS 'Total HT extrait';
COMMENT ON COLUMN ocr.ExtractedInvoices.TotalAmountTtc IS 'Total TTC extrait';
COMMENT ON COLUMN ocr.ExtractedInvoices.VatAmount IS 'TVA extraite';
COMMENT ON COLUMN ocr.ExtractedInvoices.Currency IS 'Devise lue';
COMMENT ON COLUMN ocr.ExtractedInvoices.RawJson IS 'Enregistrement complet du retour de l''IA (JSON)';
COMMENT ON COLUMN ocr.ExtractedInvoices.CreatedAt IS 'Date de création des données';
COMMENT ON COLUMN ocr.ExtractedInvoices.UpdatedAt IS 'Correction manuelle éventuelle';
COMMENT ON COLUMN ocr.ExtractedInvoices.IsActive IS 'Visibilité des données extraites';

CREATE TABLE IF NOT EXISTS ocr.ExtractedRibs (
    Id UUID PRIMARY KEY,
    JobId UUID NOT NULL REFERENCES ocr.ExtractionJobs(Id),
    BankName TEXT,
    Iban TEXT,
    Bic TEXT,
    AccountHolder TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE ocr.ExtractedRibs IS 'Données bancaires extraites des documents (RIB)';

-- ----------------------------------------------------------
-- 6. SCHEMA REPORTING (Analytics)
-- ----------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS reporting;

CREATE TABLE IF NOT EXISTS reporting.ReportingSnapshots (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    SnapshotType TEXT NOT NULL,
    Data JSONB NOT NULL,
    ReferenceDate DATE NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE reporting.ReportingSnapshots IS 'Stockage des indicateurs clés (KPI) pré-calculés pour performance';
COMMENT ON COLUMN reporting.ReportingSnapshots.Id IS 'ID unique du snapshot';
COMMENT ON COLUMN reporting.ReportingSnapshots.TenantId IS 'Tenant concerné';
COMMENT ON COLUMN reporting.ReportingSnapshots.SnapshotType IS 'Type de rapport (ex: MonthlyCashFlow)';
COMMENT ON COLUMN reporting.ReportingSnapshots.Data IS 'Ensemble des indicateurs au format JSONB';
COMMENT ON COLUMN reporting.ReportingSnapshots.ReferenceDate IS 'Date de référence des données (ex: fin de mois)';
COMMENT ON COLUMN reporting.ReportingSnapshots.CreatedAt IS 'Date de calcul initial';
COMMENT ON COLUMN reporting.ReportingSnapshots.UpdatedAt IS 'Date de mise à jour/recalcul';
COMMENT ON COLUMN reporting.ReportingSnapshots.IsActive IS 'Validité du snapshot';

-- ----------------------------------------------------------
-- ----------------------------------------------------------
-- DONNÉES DE TEST (SEEDS CONSOLIDÉS)
-- ----------------------------------------------------------

-- 1. PERMISSIONS SYSTÈME (Référentiel global)
DELETE FROM identity.Permissions;
INSERT INTO identity.Permissions (Id, SystemName, Name, Description) VALUES
('10000000-0000-0000-0000-000000000001', 'identity.users.view', 'Voir utilisateurs', 'Consulter la liste des utilisateurs'),
('10000000-0000-0000-0000-000000000002', 'identity.users.create', 'Créer utilisateur', 'Créer un nouvel utilisateur'),
('10000000-0000-0000-0000-000000000003', 'identity.users.update', 'Modifier utilisateur', 'Modifier les informations utilisateur'),
('10000000-0000-0000-0000-000000000004', 'identity.users.disable', 'Désactiver utilisateur', 'Désactiver un compte utilisateur'),
('10000000-0000-0000-0000-000000000005', 'identity.users.assign_roles', 'Assigner rôles', 'Attribuer des rôles aux utilisateurs'),
('10000000-0000-0000-0000-000000000006', 'identity.roles.view', 'Voir rôles', 'Consulter les rôles'),
('10000000-0000-0000-0000-000000000007', 'identity.roles.create', 'Créer rôle', 'Créer un rôle'),
('10000000-0000-0000-0000-000000000008', 'identity.roles.update', 'Modifier rôle', 'Modifier un rôle'),
('10000000-0000-0000-0000-000000000009', 'identity.roles.delete', 'Supprimer rôle', 'Supprimer un rôle'),
('10000000-0000-0000-0000-000000000010', 'identity.roles.assign_permissions', 'Assigner permissions', 'Associer permissions à un rôle'),
('10000000-0000-0000-0000-000000000011', 'finance.accounts.view', 'Voir plan comptable', 'Consulter le plan comptable'),
('10000000-0000-0000-0000-000000000012', 'finance.accounts.create', 'Créer compte comptable', 'Créer un compte un compte comptable'),
('10000000-0000-0000-0000-000000000013', 'finance.accounts.update', 'Modifier compte comptable', 'Modifier un compte comptable'),
('10000000-0000-0000-0000-000000000014', 'finance.accounts.archive', 'Archiver compte comptable', 'Archiver un compte comptable'),
('10000000-0000-0000-0000-000000000015', 'finance.journals.view', 'Voir journaux', 'Consulter les journaux comptables'),
('10000000-0000-0000-0000-000000000016', 'finance.journals.create', 'Créer journal', 'Créer un journal comptable'),
('10000000-0000-0000-0000-000000000017', 'finance.journals.update', 'Modifier journal', 'Modifier un journal comptable'),
('10000000-0000-0000-0000-000000000018', 'finance.entries.view', 'Voir écritures', 'Consulter les écritures comptables'),
('10000000-0000-0000-0000-000000000019', 'finance.entries.create', 'Créer écriture', 'Créer une écriture comptable'),
('10000000-0000-0000-0000-000000000020', 'finance.entries.update', 'Modifier écriture', 'Modifier une écriture comptable'),
('10000000-0000-0000-0000-000000000021', 'finance.entries.validate', 'Valider écriture', 'Valider une écriture comptable'),
('10000000-0000-0000-0000-000000000022', 'finance.entries.cancel', 'Annuler écriture', 'Annuler une écriture comptable'),
('10000000-0000-0000-0000-000000000023', 'finance_core.tiers.view', 'Voir tiers', 'Consulter clients et fournisseurs'),
('10000000-0000-0000-0000-000000000024', 'finance_core.tiers.create', 'Créer tiers', 'Créer un client ou fournisseur'),
('10000000-0000-0000-0000-000000000025', 'finance_core.tiers.update', 'Modifier tiers', 'Modifier un tiers'),
('10000000-0000-0000-0000-000000000026', 'finance_core.tiers.disable', 'Désactiver tiers', 'Désactiver un tiers'),
('10000000-0000-0000-0000-000000000027', 'finance_core.invoices.view', 'Voir factures', 'Consulter les factures'),
('10000000-0000-0000-0000-000000000028', 'finance_core.invoices.create', 'Créer facture', 'Créer une facture'),
('10000000-0000-0000-0000-000000000029', 'finance_core.invoices.update', 'Modifier facture', 'Modifier une facture'),
('10000000-0000-0000-0000-000000000030', 'finance_core.invoices.validate', 'Valider facture', 'Valider une facture'),
('10000000-0000-0000-0000-000000000031', 'finance_core.invoices.pay', 'Payer facture', 'Enregistrer un paiement'),
('10000000-0000-0000-0000-000000000032', 'finance_core.invoices.cancel', 'Annuler facture', 'Annuler une facture'),
('10000000-0000-0000-0000-000000000033', 'finance_core.budgets.view', 'Voir budgets', 'Consulter les budgets'),
('10000000-0000-0000-0000-000000000034', 'finance_core.budgets.create', 'Créer budget', 'Créer un budget'),
('10000000-0000-0000-0000-000000000035', 'finance_core.budgets.update', 'Modifier budget', 'Modifier un budget'),
('10000000-0000-0000-0000-000000000036', 'finance_core.budgets.close', 'Clôturer budget', 'Clôturer un budget'),
('10000000-0000-0000-0000-000000000037', 'finance_core.treasury.view', 'Voir trésorerie', 'Consulter la trésorerie'),
('10000000-0000-0000-0000-000000000038', 'finance_core.treasury.create', 'Créer compte trésorerie', 'Créer un compte de trésorerie'),
('10000000-0000-0000-0000-000000000039', 'finance_core.treasury.update', 'Modifier trésorerie', 'Modifier un compte de trésorerie'),
('10000000-0000-0000-0000-000000000040', 'finance_core.treasury.reconcile', 'Rapprochement bancaire', 'Effectuer un rapprochement bancaire'),
('10000000-0000-0000-0000-000000000041', 'ocr.jobs.upload', 'Upload document', 'Téléverser un document'),
('10000000-0000-0000-0000-000000000042', 'ocr.jobs.view', 'Voir traitements OCR', 'Consulter les traitements OCR'),
('10000000-0000-0000-0000-000000000043', 'ocr.jobs.retry', 'Relancer OCR', 'Relancer un traitement OCR'),
('10000000-0000-0000-0000-000000000044', 'ocr.jobs.delete', 'Supprimer traitement OCR', 'Supprimer un traitement OCR'),
('10000000-0000-0000-0000-000000000045', 'ocr.extractions.view', 'Voir extractions', 'Consulter les données extraites'),
('10000000-0000-0000-0000-000000000046', 'ocr.extractions.correct', 'Corriger extraction', 'Corriger manuellement une extraction'),
('10000000-0000-0000-0000-000000000047', 'ocr.extractions.validate', 'Valider extraction', 'Valider une extraction OCR'),
('10000000-0000-0000-0000-000000000048', 'reporting.view', 'Voir rapports', 'Consulter les rapports'),
('10000000-0000-0000-0000-000000000049', 'reporting.export', 'Exporter rapports', 'Exporter les rapports'),
('10000000-0000-0000-0000-000000000050', 'reporting.snapshots.generate', 'Générer snapshot', 'Générer un instantané de reporting'),
('10000000-0000-0000-0000-000000000051', 'billing.subscription.view', 'Voir abonnement', 'Consulter l’abonnement'),
('10000000-0000-0000-0000-000000000052', 'billing.subscription.change', 'Changer abonnement', 'Modifier le plan d’abonnement'),
('10000000-0000-0000-0000-000000000053', 'billing.invoices.view', 'Voir factures abonnement', 'Consulter les factures d’abonnement'),
('10000000-0000-0000-0000-000000000054', 'billing.invoices.download', 'Télécharger facture', 'Télécharger une facture'),
('10000000-0000-0000-0000-000000000055', 'billing.webhooks.manage', 'Gérer webhooks', 'Gérer les webhooks de paiement');

-- ----------------------------------------------------------
-- 1.5 TENANT SYSTÈME (KONTA PLATFORM ADMIN)
-- ----------------------------------------------------------
-- Tenant réservé (ID zeros) pour les administrateurs de la plateforme
DELETE FROM identity.Tenants WHERE Id = '00000000-0000-0000-0000-000000000000';
INSERT INTO identity.Tenants (Id, Name, Identifier, Plan) 
VALUES ('00000000-0000-0000-0000-000000000000', 'Konta Platform', 'KONTA-SYS', 'Enterprise');

-- Utilisateurs SuperAdmin (Mot de passe: Admin123!)
DELETE FROM identity.Users WHERE Email IN ('admin@konta.fr', 'support@konta.fr');
INSERT INTO identity.Users (Id, TenantId, Email, PasswordHash, FirstName, LastName, Role) VALUES
('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000000', 'admin@konta.fr', '$2a$11$2FAVVg1aWdMODnF4purQeOxSzQfwXgoZDcfjDk9x2/fParpNLfkeG', 'Super', 'Admin', 'SuperAdmin'),
('00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000000', 'support@konta.fr', '$2a$11$2FAVVg1aWdMODnF4purQeOxSzQfwXgoZDcfjDk9x2/fParpNLfkeG', 'Support', 'Konta', 'SuperAdmin');

-- Donner toutes les permissions au SuperAdmin (Optionnel si on gère le bypass via code, mais propre en base)
-- ...

-- 2. TENANT GLOBEX CORP (Nettoyage et Insertion)
DELETE FROM identity.UserRoles WHERE UserId IN (SELECT Id FROM identity.Users WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890');
DELETE FROM identity.Users WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890';
DELETE FROM identity.RolePermissions WHERE RoleId IN (SELECT Id FROM identity.Roles WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890');
DELETE FROM identity.Roles WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890';
DELETE FROM finance_core.Tiers WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890';
DELETE FROM identity.Tenants WHERE Id = '7c1e95fa-1234-4a5b-8cde-f01234567890';

INSERT INTO identity.Tenants (Id, Name, Identifier, Industry, Address, Siret, Plan)
VALUES ('7c1e95fa-1234-4a5b-8cde-f01234567890', 'Globex Corporation', 'GLOBEX-FR', 'Technologie', '123 Avenue des Champs-Élysées, 75008 Paris', '78467169500079', 'Premium');

-- 3. RÔLES GLOBEX
INSERT INTO identity.Roles (Id, TenantId, Name, Description) VALUES
('30000000-0000-0000-0000-000000000001', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Administrateur', 'Accès total Système'),
('30000000-0000-0000-0000-000000000002', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Expert Comptable', 'Gestion financière'),
('30000000-0000-0000-0000-000000000003', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Gérant / Manager', 'Validation et approbation'),
('30000000-0000-0000-0000-000000000004', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Auditeur', 'Consultation seule');

-- 4. PERMISSIONS GLOBEX
INSERT INTO identity.RolePermissions (RoleId, PermissionId)
SELECT '30000000-0000-0000-0000-000000000001', Id FROM identity.Permissions; -- Admin

INSERT INTO identity.RolePermissions (RoleId, PermissionId)
SELECT '30000000-0000-0000-0000-000000000002', Id FROM identity.Permissions -- Comptable
WHERE SystemName LIKE 'finance.%' OR SystemName LIKE 'finance_core.%' OR SystemName LIKE 'reporting.%';

INSERT INTO identity.RolePermissions (RoleId, PermissionId)
SELECT '30000000-0000-0000-0000-000000000004', Id FROM identity.Permissions -- Auditeur
WHERE SystemName LIKE '%.view';

-- 5. UTILISATEURS GLOBEX
INSERT INTO identity.Users (Id, TenantId, Email, PasswordHash, FirstName, LastName, Role) VALUES
('20000000-0000-0000-0000-000000000001', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'admin@globex.com', '$2a$11$/6eSb63a0CGihuRLwtYP8uKKFmZtVCAqgOyzDm.Ga17ylHDuKAAHu', 'Alice', 'Admin', 'Admin'),
('20000000-0000-0000-0000-000000000002', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'comptable@globex.com', '$2a$11$/6eSb63a0CGihuRLwtYP8uKKFmZtVCAqgOyzDm.Ga17ylHDuKAAHu', 'Bob', 'Comptable', 'Accountant'),
('20000000-0000-0000-0000-000000000003', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'manager@globex.com', '$2a$11$/6eSb63a0CGihuRLwtYP8uKKFmZtVCAqgOyzDm.Ga17ylHDuKAAHu', 'Charlie', 'Manager', 'User');

INSERT INTO identity.UserRoles (UserId, RoleId) VALUES
('20000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001'),
('20000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000002'),
('20000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000003');

-- 6. FINANCE & TIERS
INSERT INTO finance.Accounts (Id, TenantId, Code, Name, Type) VALUES
(gen_random_uuid(), '7c1e95fa-1234-4a5b-8cde-f01234567890', '512000', 'Banque Société Générale', 1),
(gen_random_uuid(), '7c1e95fa-1234-4a5b-8cde-f01234567890', '401000', 'Fournisseurs', 2),
(gen_random_uuid(), '7c1e95fa-1234-4a5b-8cde-f01234567890', '411000', 'Clients', 1),
(gen_random_uuid(), '7c1e95fa-1234-4a5b-8cde-f01234567890', '601000', 'Achats de stockage', 5),
(gen_random_uuid(), '7c1e95fa-1234-4a5b-8cde-f01234567890', '706000', 'Prestations de services', 4);

INSERT INTO finance_core.Tiers (Id, TenantId, Name, Type, Email, Address) VALUES
('50000000-0000-0000-0000-000000000001', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Oracle France', 2, 'billing@oracle.fr', 'Paris, France'),
('50000000-0000-0000-0000-000000000002', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Amazon AWS', 2, 'billing@amazon.com', 'Seattle, USA');

INSERT INTO finance_core.Budgets (Id, TenantId, Category, AllocatedAmount, SpentAmount, StartDate, EndDate) VALUES
(gen_random_uuid(), '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Infrastructure Cloud', 100000.00, 15000.00, '2026-01-01', '2026-12-31');

INSERT INTO finance_core.TreasuryAccounts (Id, TenantId, Name, AccountNumber, CurrentBalance, Currency) VALUES
('60000000-0000-0000-0000-000000000001', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Compte Courant', 'FR760000...456', 25000.00, 'EUR');

-- ----------------------------------------------------------
-- 7. PLANS D'ABONNEMENT
-- ----------------------------------------------------------
DELETE FROM billing.SubscriptionPlans WHERE Code IN ('discovery', 'basic', 'advanced', 'premium', 'expertise');

INSERT INTO billing.SubscriptionPlans (Id, Code, Name, Description, Price, Currency, MaxUsers, StorageGb, HasPrioritySupport, HasApiAccess, Features) VALUES
(gen_random_uuid(), 'discovery', 'Découverte', 'Idéal pour tester la plateforme', 0.00, 'EUR', 1, 1, FALSE, FALSE, '["1 utilisateur inclus", "Modules limités", "1 Go de stockage"]'),
(gen_random_uuid(), 'basic', 'Basique', 'Pour les petites entreprises', 19.00, 'EUR', 3, 10, FALSE, FALSE, '["3 utilisateurs inclus", "Modules standards", "10 Go de stockage", "Support Email"]'),
(gen_random_uuid(), 'advanced', 'Avancée', 'Pour une gestion poussée', 49.00, 'EUR', 6, 50, TRUE, TRUE, '["6 utilisateurs inclus", "Tous les modules", "50 Go de stockage", "Support Email + Chat", "Accès API"]'),
(gen_random_uuid(), 'premium', 'Premium', 'Tout illimité pour les PME', 99.00, 'EUR', 10, 500, TRUE, TRUE, '["10 utilisateurs inclus", "Tous les modules", "Stockage illimité", "Support SLA", "Accès API"]'),
(gen_random_uuid(), 'expertise', 'Expertise', 'Sur mesure pour les grands comptes', 0.00, 'EUR', 0, 0, TRUE, TRUE, '["Utilisateurs sur devis", "Modules sur mesure", "Stockage sur devis", "Accès API complet"]');

-- ----------------------------------------------------------
-- 8. SÉCURITÉ : ROW LEVEL SECURITY (RLS)
-- ----------------------------------------------------------

-- Fonction pour récupérer le TenantId courant depuis la session
CREATE OR REPLACE FUNCTION current_tenant_id() RETURNS UUID AS $$
    SELECT current_setting('app.current_tenant_id', true)::UUID;
$$ LANGUAGE sql STABLE;

COMMENT ON FUNCTION current_tenant_id() IS 'Récupère le TenantId injecté dans la session PostgreSQL pour la RLS';

-- Activation de la RLS et création des politiques par défaut
DO $$
BEGIN
    -- Users
    ALTER TABLE identity.Users ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON identity.Users;
    CREATE POLICY tenant_isolation_policy ON identity.Users USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- Roles
    ALTER TABLE identity.Roles ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON identity.Roles;
    CREATE POLICY tenant_isolation_policy ON identity.Roles USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- Billing
    ALTER TABLE billing.StripeCustomers ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON billing.StripeCustomers;
    CREATE POLICY tenant_isolation_policy ON billing.StripeCustomers USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    ALTER TABLE billing.BillingInvoices ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON billing.BillingInvoices;
    CREATE POLICY tenant_isolation_policy ON billing.BillingInvoices USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- Finance
    ALTER TABLE finance.Accounts ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance.Accounts;
    CREATE POLICY tenant_isolation_policy ON finance.Accounts USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    ALTER TABLE finance.Journals ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance.Journals;
    CREATE POLICY tenant_isolation_policy ON finance.Journals USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    ALTER TABLE finance.JournalEntries ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance.JournalEntries;
    CREATE POLICY tenant_isolation_policy ON finance.JournalEntries USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- Finance Core
    ALTER TABLE finance_core.Tiers ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.Tiers;
    CREATE POLICY tenant_isolation_policy ON finance_core.Tiers USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    ALTER TABLE finance_core.BusinessInvoices ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.BusinessInvoices;
    CREATE POLICY tenant_isolation_policy ON finance_core.BusinessInvoices USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    ALTER TABLE finance_core.Budgets ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.Budgets;
    CREATE POLICY tenant_isolation_policy ON finance_core.Budgets USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    ALTER TABLE finance_core.TreasuryAccounts ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.TreasuryAccounts;
    CREATE POLICY tenant_isolation_policy ON finance_core.TreasuryAccounts USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- OCR
    ALTER TABLE ocr.ExtractionJobs ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON ocr.ExtractionJobs;
    CREATE POLICY tenant_isolation_policy ON ocr.ExtractionJobs USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- Reporting
    ALTER TABLE reporting.ReportingSnapshots ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON reporting.ReportingSnapshots;
    CREATE POLICY tenant_isolation_policy ON reporting.ReportingSnapshots USING (current_tenant_id() = '00000000-0000-0000-0000-000000000000' OR TenantId = current_tenant_id());

    -- Public Access for Plans
    ALTER TABLE billing.SubscriptionPlans ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS public_read_policy ON billing.SubscriptionPlans;
    CREATE POLICY public_read_policy ON billing.SubscriptionPlans FOR SELECT USING (true);
END $$;
