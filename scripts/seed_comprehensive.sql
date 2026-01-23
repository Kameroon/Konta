-- ==========================================================
-- SCRIPT DE SEEDING COMPLET (KONTA ERP)
-- Description: Population de données cohérentes pour démo/test
-- Version: 1.1 (Includes Row Level Security & Subscription Plans)
-- ==========================================================

-- 1. NETTOYAGE DES PERMISSIONS EXISTANTES (Optionnel)
DELETE FROM identity.Permissions;

-- 2. INSERTION DES PERMISSIONS SYSTÈME
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
-- 3. TENANT SYSTÈME (KONTA PLATFORM ADMIN)
-- ----------------------------------------------------------
-- Tenant réservé (ID zeros) pour les administrateurs de la plateforme
DELETE FROM identity.Tenants WHERE Id = '00000000-0000-0000-0000-000000000000';
INSERT INTO identity.Tenants (Id, Name, Identifier, Plan) 
VALUES ('00000000-0000-0000-0000-000000000000', 'Konta Platform', 'KONTA-SYS', 'Enterprise');

-- Utilisateur SuperAdmin (Mot de passe: Admin123!)
DELETE FROM identity.Users WHERE Email = 'admin@konta.fr';
INSERT INTO identity.Users (Id, TenantId, Email, PasswordHash, FirstName, LastName, Role)
VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000000', 'admin@konta.fr', '$2a$11$9W0f2e0/GRIH6mU8R8uYI.L7k6m6v8y8y8y8y8y8y8y8y8y8y8y8y', 'Super', 'Admin', 'SuperAdmin');

-- ----------------------------------------------------------
-- 4. TENANT DE RÉFÉRENCE (GLOBEX)
-- ----------------------------------------------------------
DELETE FROM identity.UserRoles WHERE UserId IN (SELECT Id FROM identity.Users WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890');
DELETE FROM identity.Users WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890';
DELETE FROM identity.RolePermissions WHERE RoleId IN (SELECT Id FROM identity.Roles WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890');
DELETE FROM identity.Roles WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890';
DELETE FROM finance_core.Tiers WHERE TenantId = '7c1e95fa-1234-4a5b-8cde-f01234567890';
DELETE FROM identity.Tenants WHERE Id = '7c1e95fa-1234-4a5b-8cde-f01234567890';

INSERT INTO identity.Tenants (Id, Name, Identifier, Industry, Address, TaxId, Plan)
VALUES ('7c1e95fa-1234-4a5b-8cde-f01234567890', 'Globex Corporation', 'GLOBEX-FR', 'Technologie', '123 Avenue des Champs-Élysées, 75008 Paris', 'FR12345678901', 'Premium');

-- Rôles pour le tenant
INSERT INTO identity.Roles (Id, TenantId, Name, Description) VALUES
('30000000-0000-0000-0000-000000000001', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Administrateur', 'Accès total Système'),
('30000000-0000-0000-0000-000000000002', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Expert Comptable', 'Gestion financière'),
('30000000-0000-0000-0000-000000000003', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Gérant / Manager', 'Validation et approbation'),
('30000000-0000-0000-0000-000000000004', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'Auditeur', 'Consultation seule');

-- Liaison Rôles -> Permissions
INSERT INTO identity.RolePermissions (RoleId, PermissionId)
SELECT '30000000-0000-0000-0000-000000000001', Id FROM identity.Permissions; -- Admin

INSERT INTO identity.RolePermissions (RoleId, PermissionId)
SELECT '30000000-0000-0000-0000-000000000002', Id FROM identity.Permissions -- Comptable
WHERE SystemName LIKE 'finance.%' OR SystemName LIKE 'finance_core.%' OR SystemName LIKE 'reporting.%';

INSERT INTO identity.RolePermissions (RoleId, PermissionId)
SELECT '30000000-0000-0000-0000-000000000004', Id FROM identity.Permissions -- Auditeur
WHERE SystemName LIKE '%.view';

-- Utilisateurs Globex
INSERT INTO identity.Users (Id, TenantId, Email, PasswordHash, FirstName, LastName, Role) VALUES
('20000000-0000-0000-0000-000000000001', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'admin@globex.com', '$2a$11$OvHGeoGRS694M.8YPOrKUuFaHQHh9ixo7VjZeYCSk4IGmqF9Vif/6', 'Alice', 'Admin', 'Admin'),
('20000000-0000-0000-0000-000000000002', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'comptable@globex.com', '$2a$11$OvHGeoGRS694M.8YPOrKUuFaHQHh9ixo7VjZeYCSk4IGmqF9Vif/6', 'Bob', 'Comptable', 'Accountant'),
('20000000-0000-0000-0000-000000000003', '7c1e95fa-1234-4a5b-8cde-f01234567890', 'manager@globex.com', '$2a$11$OvHGeoGRS694M.8YPOrKUuFaHQHh9ixo7VjZeYCSk4IGmqF9Vif/6', 'Charlie', 'Manager', 'User');

INSERT INTO identity.UserRoles (UserId, RoleId) VALUES
('20000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001'),
('20000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000002'),
('20000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000003');

-- ----------------------------------------------------------
-- 5. FINANCE & TIERS
-- ----------------------------------------------------------
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
-- 6. PLANS D'ABONNEMENT
-- ----------------------------------------------------------
DELETE FROM billing.SubscriptionPlans WHERE Code IN ('discovery', 'basic', 'advanced', 'premium', 'expertise');

INSERT INTO billing.SubscriptionPlans (Id, Code, Name, Description, Price, Currency, MaxUsers, StorageGb, HasPrioritySupport, HasApiAccess, Features) VALUES
(gen_random_uuid(), 'discovery', 'Découverte', 'Idéal pour tester la plateforme', 0.00, 'EUR', 1, 1, FALSE, FALSE, '["1 utilisateur inclus", "Modules limités", "1 Go de stockage"]'),
(gen_random_uuid(), 'basic', 'Basique', 'Pour les petites entreprises', 19.00, 'EUR', 3, 10, FALSE, FALSE, '["3 utilisateurs inclus", "Modules standards", "10 Go de stockage", "Support Email"]'),
(gen_random_uuid(), 'advanced', 'Avancée', 'Pour une gestion poussée', 49.00, 'EUR', 6, 50, TRUE, TRUE, '["6 utilisateurs inclus", "Tous les modules", "50 Go de stockage", "Support Email + Chat", "Accès API"]'),
(gen_random_uuid(), 'premium', 'Premium', 'Tout illimité pour les PME', 99.00, 'EUR', 10, 500, TRUE, TRUE, '["10 utilisateurs inclus", "Tous les modules", "Stockage illimité", "Support SLA", "Accès API"]'),
(gen_random_uuid(), 'expertise', 'Expertise', 'Sur mesure pour les grands comptes', 0.00, 'EUR', 0, 0, TRUE, TRUE, '["Utilisateurs sur devis", "Modules sur mesure", "Stockage sur devis", "Accès API complet"]');

-- ----------------------------------------------------------
-- 7. SÉCURITÉ : ROW LEVEL SECURITY (RLS)
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
    CREATE POLICY tenant_isolation_policy ON identity.Users USING (TenantId = current_tenant_id());

    -- Roles
    ALTER TABLE identity.Roles ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON identity.Roles;
    CREATE POLICY tenant_isolation_policy ON identity.Roles USING (TenantId = current_tenant_id());

    -- Billing
    ALTER TABLE billing.StripeCustomers ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON billing.StripeCustomers;
    CREATE POLICY tenant_isolation_policy ON billing.StripeCustomers USING (TenantId = current_tenant_id());

    ALTER TABLE billing.BillingInvoices ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON billing.BillingInvoices;
    CREATE POLICY tenant_isolation_policy ON billing.BillingInvoices USING (TenantId = current_tenant_id());

    -- Finance
    ALTER TABLE finance.Accounts ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance.Accounts;
    CREATE POLICY tenant_isolation_policy ON finance.Accounts USING (TenantId = current_tenant_id());

    ALTER TABLE finance.Journals ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance.Journals;
    CREATE POLICY tenant_isolation_policy ON finance.Journals USING (TenantId = current_tenant_id());

    ALTER TABLE finance.JournalEntries ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance.JournalEntries;
    CREATE POLICY tenant_isolation_policy ON finance.JournalEntries USING (TenantId = current_tenant_id());

    -- Finance Core
    ALTER TABLE finance_core.Tiers ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.Tiers;
    CREATE POLICY tenant_isolation_policy ON finance_core.Tiers USING (TenantId = current_tenant_id());

    ALTER TABLE finance_core.BusinessInvoices ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.BusinessInvoices;
    CREATE POLICY tenant_isolation_policy ON finance_core.BusinessInvoices USING (TenantId = current_tenant_id());

    ALTER TABLE finance_core.Budgets ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.Budgets;
    CREATE POLICY tenant_isolation_policy ON finance_core.Budgets USING (TenantId = current_tenant_id());

    ALTER TABLE finance_core.TreasuryAccounts ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON finance_core.TreasuryAccounts;
    CREATE POLICY tenant_isolation_policy ON finance_core.TreasuryAccounts USING (TenantId = current_tenant_id());

    -- OCR
    ALTER TABLE ocr.ExtractionJobs ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON ocr.ExtractionJobs;
    CREATE POLICY tenant_isolation_policy ON ocr.ExtractionJobs USING (TenantId = current_tenant_id());

    -- Reporting
    ALTER TABLE reporting.ReportingSnapshots ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS tenant_isolation_policy ON reporting.ReportingSnapshots;
    CREATE POLICY tenant_isolation_policy ON reporting.ReportingSnapshots USING (TenantId = current_tenant_id());

    -- Public Access for Plans
    ALTER TABLE billing.SubscriptionPlans ENABLE ROW LEVEL SECURITY;
    DROP POLICY IF EXISTS public_read_policy ON billing.SubscriptionPlans;
    CREATE POLICY public_read_policy ON billing.SubscriptionPlans FOR SELECT USING (true);
END $$;
